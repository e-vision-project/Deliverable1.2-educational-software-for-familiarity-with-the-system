using System.Collections;
using UnityEngine;
using static EVISION.Camera.plugin.MasoutisView;
using System.Collections.Generic;
using System.Linq;
using static EVISION.Camera.plugin.GenericUtils;
using static LogManager;
using System;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{
    public class MasoutisManager : CameraClient
    {
        // INTERFACES
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;

        // INSPECTOR PROPERTIES
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private bool logging;

        // PRIVATE PROPERTIES
        private MasoutisItem masoutis_obj;
        private SVMClassification svm_model;
        private MajorityVoting majVoting;
        private bool productAnnotationDone; 
        private bool shelfAnnotationDone;
        private string annotationText;
        private Texture2D processedTex;

        // PUBLIC PROPERTIES
        public static int category;
        public bool DB_LoadProccessBusy;
        public Text resultTextPanel;

        // LOGGING
        private float OCRtime;
        private float Majoritytime;
        private float classificationTime;
        public static string OCRLogs;
        public static string validWordsLogs;

        #region CameraClient Callbacks

        public override async void ProcessScreenShotAsync()
        {
            scenarioCase = Enums.ScenarioCases.masoutis;
            productAnnotationDone = false;
            shelfAnnotationDone = false;
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;
            resultTextPanel.text = "Επεξεργασία φωτογραφίας";

            text_result = "";

            // reference to the texture object.
            processedTex = new Texture2D(0, 0);
            processedTex = await GetScreenShot();
            while (!textureReceived)
            {
                await frameBreak;
            }

            category = ClassifyCategory(processedTex);
            SetDisplayImage(processedTex);
            
            // product case
            if (category == (int)Enums.MasoutisCategories.product)
            {
                GetProductDescription(processedTex);
                while (!productAnnotationDone)
                {
                    await frameBreak;
                }
            }
            else
            {
                GetTrailShelfDescription(category, processedTex);
                while (!shelfAnnotationDone)
                {
                    await frameBreak;
                }
            }
            SetTimeText();
            SetResultLogs(processedTex);

            //Used to set the main UI
            if (String.IsNullOrEmpty(text_result))
            {
                text_result = "Δεν αναγνωρίστηκε, ξαναπροσπαθήστε.";
            }
            UAP_AccessibilityManager.Say(text_result);
            EventCamManager.onProcessEnded?.Invoke();

            annotationProccessBusy = false;
            await Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public override void SaveScreenshot(Texture2D camTexture)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            annotator = GetComponent<IAnnotate>();
            httpLoader = GetComponent<HttpImageLoading>();
            majVoting = new MajorityVoting();
            SetSVM();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LoadDatabase());
            DB_LoadProccessBusy = true;
            annotationProccessBusy = false;
            frameBreak = new WaitForEndOfFrame();
            //Create the unique id of the log manager, so each user get different log id.
            //TODO : This is a bad place to be initialized.
            if (!PlayerPrefs.HasKey("uniqueID"))
            {
                Debug.Log("creating player unique id");
                LogManager.SetUniqueId();
            }
            else
            {
                Debug.Log("unique id exists");
                LogManager.uniqueId = PlayerPrefs.GetString("uniqueID");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (MajorityVoting.database_ready && DB_LoadProccessBusy == true) { DB_LoadProccessBusy = false; }
        }

        #endregion

        #region Initializers
  
        private void SetSVM()
        {
            //Get feautures from model
            featureExtractor = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, DLModel, LabelsFile, 180, 0.01f);
            // set and load svm model
            svm_model = new SVMClassification();
            svm_model.SetModelParameters("SVM_Weights", "mu", "sigma");
            svmClassifier = svm_model;
        }

        //Load database
        public IEnumerator LoadDatabase()
        {
            annotationProccessBusy = true;
            MajorityVoting.LoadDatabaseFiles(MajorityVoting.masoutisFiles);
            while (MajorityVoting.database_ready != true)
            {
                yield return null;
            }
            annotationProccessBusy = false;
        }

        #endregion

        #region Logging

        public override void SetResultLogs(Texture2D currTex)
        {
            
            if (logging)
            {
                var sum = captureTime + OCRtime + classificationTime + Majoritytime;
                string fileName = $"masoutis_{System.DateTime.Now.ToString()}";
                fileName = fileName.Replace(" ", "_");
                fileName = fileName.Replace(":", "_");
                fileName = fileName.Replace("/", "_");
                fileName = fileName.Replace(".", "");
                fileName = fileName.Replace("μμ", "MM");
                fileName = fileName.Replace("πμ", "PM");
                string imageName = GetResultLogs("Image Name", fileName);
                string response =  GetResponseTime(captureTime.ToString(), OCRtime.ToString(), classificationTime.ToString(), 
                    Majoritytime.ToString(), sum.ToString());
                string ocrResults = GetResultLogs("OCR Results", OCRLogs);
                string classificationResults = GetResultLogs("Classification", category.ToString());
                string validWordsResults = GetResultLogs("Valid Words", validWordsLogs);
                string finalResult = GetResultLogs("ReturnedResult", text_result);
                string logText = imageName + "\n" + response + "\n" + ocrResults + "\n" + classificationResults + "\n" + validWordsResults + "\n" + 
                    finalResult + "\n\n";
                SaveResultLogs(logText);
                SaveImageFile(currTex, fileName);
                //StartCoroutine( LogManager.UploadLog("", $"{fileName}.jpg"));
            }
        }

        private void SetTimeText()
        {
            if (TimeText != null)
            {
                TimeText = "Full process costed : " + (OCRtime + Majoritytime + classificationTime).ToString() + "\nOCRtime: " + OCRtime.ToString()
                    + "\nMajorityTime: " + Majoritytime.ToString() + "\nClassificationTime: " + classificationTime.ToString();
            }
        }

        #endregion

        /// <summary>  
        /// This methods aims to classify a 2D texture based on the svm model that has been initialized 
        /// in the start method of this class.  
        /// </summary>
        /// <param name="input_Tex"> Texture2D obj</param>  
        /// <returns>Integer type</returns>
        private int ClassifyCategory(Texture2D input_tex)
        {
            Texture2D tempTex = new Texture2D(input_tex.width, input_tex.height, TextureFormat.RGB24, false);

#if UNITY_ANDROID
            //Graphics.CopyTexture(input_tex, tempTex);
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {
                Color[] sourcePixels = input_tex.GetPixels();
                tempTex.SetPixels(sourcePixels);
                tempTex.Apply();
            }
            else
            {
                Graphics.CopyTexture(input_tex, tempTex);
            }
#endif

#if UNITY_IOS
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {
                Color[] sourcePixels = input_tex.GetPixels();
                tempTex.SetPixels(sourcePixels);
                tempTex.Apply();
            }
            else
            {
                Graphics.CopyTexture(input_tex, tempTex);
            }
#endif

            float startclass = Time.realtimeSinceStartup;

            // extract feautures from network.
            var featureVector = featureExtractor.FetchOutput<List<float>, Texture2D>(tempTex);

            // normalize feature vector
            var output_array = ConvertToDouble(featureVector.ToArray()); // convert to double format.
            var norm_fv = svm_model.NormalizeElements(output_array, svm_model.muData, svm_model.sigmaData);
            List<double> norm_fv_list = new List<double>(norm_fv);

            // calculate propabilities
            var probs = svmClassifier.FetchOutput<List<float>, List<double>>(norm_fv_list);

            // get max propability class.
            float maxValue = probs.Max();
            int categoryIndex = probs.IndexOf(maxValue);

            float endclass = Time.realtimeSinceStartup;
            classificationTime = CalculateTimeDifference(startclass, endclass);
            DestroyImmediate(tempTex);
            return categoryIndex;
        }

        public async void GetProductDescription(Texture2D camTex)
        {
            // output message to user.
            UAP_AccessibilityManager.Say("Αναγνώριση Προϊόντος");
            resultTextPanel.text = "Αναγνώριση προϊόντος";

            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            annotationText = await annotator.PerformAnnotation(camTex);

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText) && annotationText != "GCFAILED")
            {
                float startMajt = Time.realtimeSinceStartup;
                var wordsOCR = SplitStringToList(annotationText);
                OCRLogs = GenericUtils.ListToString(wordsOCR);
                var valid_words = MajorityVoting.GetValidWords(wordsOCR);
                validWordsLogs = GenericUtils.ListToString(valid_words);
                string product_formatted = FormatDescription(ListToString(valid_words));
                if (MajorityValidText != null)
                {
                    MajorityValidText = product_formatted;
                }
                majorityFinal = product_formatted;
                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);
                text_result = product_formatted.ToLower();
            }
            else if (annotationText == "GCFAILED")
            {
                text_result = "Η σύνδεση στο δίκτυο είναι απενεργοποιημένη.";
            }
            else
            {
                text_result = "Δεν αναγνωρίστηκαν λέξεις";
            }
            productAnnotationDone = true;
        }

        private string FormatDescription(string product)
        {
            var edit = SplitStringToList(product);
            edit = MajorityVoting.KeepElementsWithLen(edit, 4);
            var description = ListToString(edit);
            return description;
        }

        /// <summary>  
        /// This methods based on the category given as input, finds the description of the trail, shelf, inner shelf  
        /// database based on the Majority Voting algorithm of the homonymous class.
        /// </summary>
        /// <param name="category"> int </param>  
        /// <returns>IEnumarator object</returns>
        public async void GetTrailShelfDescription(int category, Texture2D camTex)
        {
            // output message to user.
            UAP_AccessibilityManager.Say("Αναγνώριση ραφιού-διαδρόμου");
            resultTextPanel.text = "Αναγνώριση ραφιού-διαδρόμου";
            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation.
            annotationText = await annotator.PerformAnnotation(camTex);
            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (!string.IsNullOrEmpty(annotationText) && annotationText != "GCFAILED")
            {
                float startMajt = Time.realtimeSinceStartup;

                List<string> OCR_List = SplitStringToList(annotationText);
                majVoting.PerformMajorityVoting(OCR_List);
                
                OCR_List = null;
                float endMajt = Time.realtimeSinceStartup;
                Majoritytime = CalculateTimeDifference(startMajt, endMajt);

                if (!verboseMode)
                {
                    switch (category)
                    {
                        case (int)Enums.MasoutisCategories.trail:
                            text_result = majVoting.masoutis_item.category_2;
                            break;
                        case (int)Enums.MasoutisCategories.shelf:
                            text_result = majVoting.masoutis_item.category_4;
                            break;
                        case (int)Enums.MasoutisCategories.other:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    text_result = "διάδρομος " + majVoting.masoutis_item.category_2 + ", " +
                                   "ράφι " + majVoting.masoutis_item.category_3 + ", " +
                                   "υποκατηγορία ραφιού " + majVoting.masoutis_item.category_4;
                }
            }
            else if (annotationText == "GCFAILED")
            {
                text_result = "Η σύνδεση στο δίκτυο είναι απενεργοποιημένη.";
            }
            else
            {
                switch (category)
                {
                    case (int)Enums.MasoutisCategories.trail:
                        text_result = "Δεν αναγνωρίστηκαν λέξεις";
                        break;
                    case (int)Enums.MasoutisCategories.shelf:
                        text_result = "Δεν αναγνωρίστηκαν λέξεις";
                        break;
                    case (int)Enums.MasoutisCategories.other:
                        break;
                    default:
                        break;
                }
                if (MajorityFinalText != null)
                {
                    MajorityFinalText = "Δεν αναγνωρίστηκαν διαθέσιμες λέξεις";
                }
                if (MajorityValidText != null)
                {
                    MajorityValidText = "κενό";
                }
            }
            shelfAnnotationDone = true;
        }

        public void CancelButton()
        {
            //if (!annotationProccessBusy && !DB_LoadProccessBusy)
            //{
            //    return;
            //}
            StopAllCoroutines();
            UAP_AccessibilityManager.StopSpeaking();
            //displayImage.texture = null;
            annotationProccessBusy = false;
            textureReceived = false;

            DestroyImmediate(processedTex);
            DestroyImmediate(camTex);
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Debug.Log("masoutis cancel");
        }
    }
}
