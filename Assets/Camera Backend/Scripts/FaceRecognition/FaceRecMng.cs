using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using EVISION.Camera.plugin;
using HutongGames.PlayMaker.Actions;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;
using UnityEngine.UI;

namespace FaceRecognition
{
    public class FaceRecMng : CameraClient
    {
        public SerializableDictionary<string, FaceEmbeddings> StoredFaces;
        [SerializeField] private VGGFaceInference _vggExtractor;

        public delegate void OnInputNameDuplicate();
        public static event OnInputNameDuplicate onInputNameDuplicate;

        //private Texture2D _faceShot;

        private void Awake()
        {
            _vggExtractor = GetComponent<VGGFaceInference>();
            httpLoader = GetComponent<HttpImageLoading>();

            try
            {
                StoredFaces = LoadStoredFaces();
                Debug.Log("Loaded dictionary with count:" + StoredFaces.serializedDict.Count);
            }
            catch (Exception e)
            {
                Debug.LogError("Dictionary does not exist. Consider creating one by adding faces.");
                throw;
            }
        }

        #region Dictionary IO

        /// <summary>
        /// Deserializes the dictionary and loads its into a dictionary.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SerializableDictionary<string, FaceEmbeddings> LoadStoredFaces(string fileName = "StoredFacesDict")
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream;
            string input_path = $"{Application.persistentDataPath}/{fileName}.dat";
            if (File.Exists(input_path))
            {
                stream = File.OpenRead(input_path);
            }
            else
            {
                return new SerializableDictionary<string, FaceEmbeddings>();
            }
            var dict = formatter.Deserialize(stream) as SerializableDictionary<string, FaceEmbeddings>;
            stream.Close();
            return dict;
        }

        /// <summary>
        /// Serializes the dictionary and saves it into a binary file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveStoredFaces(string filename = "StoredFacesDict")
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string input_path = $"{Application.persistentDataPath}/{filename}.dat";
            FileStream stream;
            if (System.IO.File.Exists(input_path))
            {
                stream = File.OpenWrite(input_path);
            }
            else
            {
                stream = File.Create(input_path);
            }
            Debug.Log($"save at {input_path}");
            formatter.Serialize(stream, StoredFaces);
            stream.Close();
        }

        /// <summary>
        /// Add a face with a name specified as input into the dictionary.
        /// </summary>
        /// <param name="faceName"></param>
        /// <param name="inputImg"></param>
        /// <returns> bool </returns>
        public bool AddFace(string faceName, Texture2D inputImg)
        {
            //handle duplicate key exception
            if (StoredFaces.serializedDict.ContainsKey(faceName))
            {
                onInputNameDuplicate?.Invoke();
                //Debug.LogError("Duplicate name in faces dictionary");
                UAP_AccessibilityManager.Say("Το όνομα υπάρχει ήδη στην βάση. Η προσθήκη απέτυχε.");
                return false; 
            }
            //extract face embeddings from image.
            var embeddings = ExtractEmbeddings(inputImg);
            if (embeddings != null)
            {
                StoredFaces.serializedDict.Add(faceName, embeddings);
                SaveStoredFaces();
                UAP_AccessibilityManager.Say($"Το οικείο πρόσωπο {faceName} αποθηκέυτηκε επιτυχώς.");
                return true;
            }
            else
            {
                //Debug.LogError($"Error occured in saving face with name {faceName}");
                UAP_AccessibilityManager.Say("Δεν εντοπίστηκε πρόσωπο. Η προσθήκη απέτυχε.");
                return false;
            }
        }

        /// <summary>
        /// Delete a face from the dictionary.
        /// </summary>
        /// <param name="faceName"></param>
        public void DeleteFace(string faceName)
        {
            if (StoredFaces.serializedDict.Remove(faceName))
            {
                //Debug.Log($"face with name {faceName} removed");
                SaveStoredFaces();
            }
            else
            {
                Debug.Log($"face with name {faceName} WAS NOT removed");
            }
        }

        #endregion

        #region Listeners

        public void AddFaceListener()
        {
            var name = AppManager.AddFaceResultMenu.AddNameInputField.text;
            if (AddFace(name, camTex))
            {
                AppManager.AddFaceResultMenu.SetResult(camTex, true);
            }
            else
            {
                AppManager.AddFaceResultMenu.SetResult(camTex, false);
            }
        }

        public async void GetFaceScreenshot()
        {
            await GetScreenShot();
            AppManager.AddFaceResultMenu.displayImg.texture = camTex;
            while (!textureReceived)
            {
                await new WaitForEndOfFrame();
            }
        }

        public async void RecognizeFaceListener()
        {
            await GetScreenShot();
            while (!textureReceived)
            {
                await new WaitForEndOfFrame();
            }

            text_result = RecognizeFace(camTex);
            if (text_result == "Δεν εντοπίστηκε πρόσωπο" || text_result == "Δεν αναγνωρίστηκε")
            {
                AppManager.FaceRecResultMenu.SetResult(text_result, camTex, false);
                UAP_AccessibilityManager.Say(text_result);
            }
            else
            {
                AppManager.FaceRecResultMenu.SetResult(text_result, camTex, true);
                UAP_AccessibilityManager.Say(text_result);
            }
        }

        public void ConnectExternalCamera()
        {
            if (externalCamera)
            {
                httpLoader.CheckConnectionStatus();
            }
        }

        #endregion

        /// <summary>
        /// Use the face recognition model to extract face embeddings vector.
        /// </summary>
        /// <param name="imageTex"></param>
        /// <returns>FaceEmbeddings object</returns>
        public FaceEmbeddings ExtractEmbeddings(Texture2D imageTex)
        {
            var croppedFace = _vggExtractor.GetCroppedFace(imageTex);
            if (croppedFace == null)
            {
                return null;
            }
            //Imgproc.resize(croppedFace, croppedFace, new Size(224,224);
            var embeddings = _vggExtractor.ExtractEmbeddings(croppedFace);
            return embeddings;
        }

        /// <summary>
        /// Calculate the euclidean distance between 2 FaceEmbeddings objects. 
        /// </summary>
        /// <param name="emb1"></param>
        /// <param name="emb2"></param>
        /// <returns> double </returns>
        public double CompareEmbeddings(FaceEmbeddings emb1, FaceEmbeddings emb2)
        {
            var mat1 = emb1.CreateMatFromEmbeddings(emb1.Embeddings);
            var mat2 = emb2.CreateMatFromEmbeddings(emb2.Embeddings);

            if (mat1 == null || mat2 == null)
            {
                return 1000.0;
            }

            var diff = mat1 - mat2;
            var result = diff.dot(diff);
            result = Mathf.Sqrt((float)result);
            return result;
        }

        public double CompareSimilarity(FaceEmbeddings emb1, FaceEmbeddings emb2)
        {
            var mat1 = emb1.CreateMatFromEmbeddings(emb1.Embeddings);
            var mat2 = emb2.CreateMatFromEmbeddings(emb2.Embeddings);

            // l2 normalize the vectors
            for (int i = 0; i < 128; i++)
            {
                mat1.put(0, i, mat1.get(0, i)[0] / Core.norm(mat1,4));
            }

            for (int i = 0; i < 128; i++)
            {
                mat2.put(0, i, mat2.get(0, i)[0] / Core.norm(mat2,4));
            }

            if (mat1 == null || mat2 == null)
            {
                return 1000.0;
            }

            var ab = mat1.dot(mat2);
            var aMagni = Core.norm(mat1);
            var bMagni = Core.norm(mat2);
            var cos_sim = ab / (aMagni * bMagni);

            mat1.Dispose(); mat2.Dispose();

            return 1 - cos_sim;
        }

        public string RecognizeFace(Texture2D captureFace)
        {
            //TickMeter tm = new TickMeter();
            //tm.start();

            var input_emb = ExtractEmbeddings(captureFace);
            if (input_emb != null)
            {
                var dict = StoredFaces.serializedDict;
                var matchResults = new Dictionary<string, double>();
                foreach (var key in dict.Keys)
                {
                    var dist = CompareSimilarity(input_emb, dict[key]);
                    matchResults.Add(key, dist);
                }

                var matched = (from match in matchResults where match.Value < 0.5 orderby match.Value select match)
                    .ToList();
                if (matched.Count > 0)
                {
                    //tm.stop();
                    //Debug.Log("face rec: " + tm.getTimeMilli());
                    return matched[0].Key;
                }

                //tm.stop();
                //Debug.Log("face rec: " + tm.getTimeMilli());
                return "Δεν αναγνωρίστηκε";
            }
            else
            {
                return "Δεν εντοπίστηκε πρόσωπο";
            }

           
        }

        public override void ProcessScreenShotAsync()
        {
            throw new NotImplementedException();
        }

        public override void SaveScreenshot(Texture2D camTexture)
        {
            throw new NotImplementedException();
        }

        public override void SetResultLogs(Texture2D currTex)
        {
            throw new NotImplementedException();
        }

        public void CancelButton()
        {
            StopAllCoroutines();
            UAP_AccessibilityManager.StopSpeaking();
            //displayImage.texture = null;
            annotationProccessBusy = false;
            textureReceived = false;
            AppManager.AddFaceResultMenu.AddNameInputField.text = "";

            DestroyImmediate(camTex);
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Debug.Log("face cancel");

        }
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> _keys = new List<TKey>();
    [SerializeField] private List<TValue> _values = new List<TValue>();

    //Unity doesn't know how to serialize a Dictionary
    public Dictionary<TKey, TValue> serializedDict = new Dictionary<TKey, TValue>();

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in serializedDict)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        serializedDict = new Dictionary<TKey, TValue>();
        for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
        {
            serializedDict.Add(_keys[i], _values[i]);
        }
    }

}

