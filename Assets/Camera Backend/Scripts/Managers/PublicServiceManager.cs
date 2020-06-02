using EVISION.Camera.plugin;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static EVISION.Camera.plugin.GenericUtils;
using System;
using System.IO;
using JetBrains.Annotations;
using OpenCVForUnity.UnityUtils;
using UnityEngine.UI;
using static LogManager;

namespace EVISION.Camera.plugin
{
    public class PublicServiceManager : CameraClient
    {
        // INTERFACES
        private IModelPrediction featureExtractor;
        private IModelPrediction svmClassifier;

        // INSPECTOR PROPERTIES
        [SerializeField] private TextAsset DLModel;
        [SerializeField] private TextAsset LabelsFile;
        [SerializeField] private bool logging;
        [SerializeField] private bool documentViewing;

        //Private Properties
        private SVMClassification svm_model;
        private string annotationText;
        public static bool annotationDone;
        private WaitForEndOfFrame frameBreak;
        private bool faceAnnotationDone = false;
        private bool docAnnotationDone = false;
        private bool signAnnotationDone = false;
        private int blocksIndex = 0;
        private List<string> blocksOfText = new List<string>();
        private List<string> docTitlesList = new List<string>();
        private const int THRESHOLD = 5;
        private Texture2D processedTex;

        // PUBLIC PROPERTIES
        public static int category;
        public Text resultTextPanel;

        // LOGGING
        private float OCRtime;
        private float classificationTime;
        private string documentLogs;
        private string faceLogs;
        private string objectLogs;



        public override async void ProcessScreenShotAsync()
        {
            scenarioCase = Enums.ScenarioCases.publicService;
            annotationDone = false;
            blocksOfText.Clear();
            blocksIndex = 0;
            documentViewing = false;
            // lock the process so the user cannot access it.
            annotationProccessBusy = true;
            resultTextPanel.text = "Επεξεργασία φωτογραφίας";

            text_result = "";

            // reference to the texture object.
            processedTex = new Texture2D(0,0);
            processedTex = await GetScreenShot();
            while (!textureReceived)
            {
                await frameBreak;
            }

            SetDisplayImage(processedTex);

            float startOCRt = Time.realtimeSinceStartup;
            annotationText = await annotator.PerformAnnotation(processedTex);
            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            if (annotationText == "GCFAILED")
            {
                //text_result = "Η σύνδεση στο δίκτυο είναι απενεργοποιημένη.";
                ////Used to set the main UI
                //UAP_AccessibilityManager.Say(text_result);
                //EventCamManager.onProcessEnded?.Invoke();

                annotationProccessBusy = false;
                textureReceived = false;
                annotationDone = false;
                await Resources.UnloadUnusedAssets();
                GC.Collect();
                return;
            }

            blocksOfText = SplitStringToList(annotationText, "<end_block>");
            blocksOfText.Remove("");

            //keep only OCR text.
            var textFound = new List<string>(blocksOfText);
            textFound.RemoveAt(0);
            documentLogs = ListToString(textFound);

            while (annotationDone != true)
            {
                await frameBreak;
            }
            switch (PublicServiceManager.category)
            {
                case (int)Enums.PServiceCategories.document:
                    UAP_AccessibilityManager.Say("Αναγνώριση εγγράφου");
                    resultTextPanel.text = "Αναγνώριση εγγράφου";
                    await new WaitForSeconds(1.0f);
                    //var doc = GetDocumentTitle();
                    //if (doc != null)
                    //{
                    //    blocksOfText.Insert(0, doc);
                    //}
                    text_result = blocksOfText[0];
                    documentViewing = true;
                    break;
                case (int)Enums.PServiceCategories.sign:
                    UAP_AccessibilityManager.Say("Αναγνώριση πινακίδας");
                    resultTextPanel.text = "Αναγνώριση πινακίδας";
                    await new WaitForSeconds(1.0f);
                    text_result = annotationText;
                    break;
                case (int)Enums.PServiceCategories.face:
                    UAP_AccessibilityManager.Say("Αναγνώριση προσώπου");
                    resultTextPanel.text = "Αναγνώριση προσώπου";
                    await new WaitForSeconds(1.0f);
                    var results = GetFaceText(annotationText);
                    text_result = results; 
                    break;
                case (int)Enums.PServiceCategories.obj:
                    UAP_AccessibilityManager.Say("Αναγνώριση αντικειμένου");
                    resultTextPanel.text = "Αναγνώριση αντικειμένου";
                    text_result = annotationText;
                    objectLogs = annotationText;
                    break;
                case (int)Enums.PServiceCategories.face_doc:
                    UAP_AccessibilityManager.Say("Βρέθηκαν πρόσωπα και κείμενο");
                    resultTextPanel.text = "Βρέθηκαν πρόσωπα και κείμενο";
                    await new WaitForSeconds(1.0f);
                    var faceResults = GetFaceText(blocksOfText[0]);
                    text_result = faceResults;
                    blocksOfText.RemoveAt(0);
                    // set document name
                    //var docTitle = GetDocumentTitle();
                    //if (docTitle != null)
                    //{
                    //    blocksOfText.Insert(0, docTitle);
                    //}
                    break;
            }

            SetTimeText();
            SetResultLogs(processedTex);

            //Used to set the main UI
            UAP_AccessibilityManager.Say(text_result);
            EventCamManager.onProcessEnded?.Invoke();

            annotationProccessBusy = false;
            textureReceived = false;
            annotationDone = false;
            await Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private string GetDocumentTitle()
        {
            var dist = new List<Tuple<string, float>>();
            for (int i = 0; i < blocksOfText.Count; i++)
            {
                var text = GenericUtils.RemoveGreekAccent(blocksOfText[i]); 
                var bestMatch = GetClosestDescriptionFromDB(text);
                dist.Add(bestMatch);
            }

            var min = dist.Aggregate((minItem, nextItem)
                => minItem.Item2 < nextItem.Item2 ? minItem : nextItem);
            if (min.Item2 < THRESHOLD)
            {
                return $"έγγραφο: {min.Item1}";

            }
            return null;
        }

        [CanBeNull]
        public Tuple<string,float> GetClosestDescriptionFromDB(string capturedWord)
        {
            var dists = new List<float>();
            
            foreach (var docTitle in docTitlesList)
            {
                dists.Add(LevenshteinDistance.Compute(capturedWord, docTitle));
            }
            
            var min = dists.Min();
            var bestMatch = docTitlesList[dists.IndexOf(min)];
            return new Tuple<string, float>(bestMatch, min);
        }

        public void ReadDocumentTitles(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var list = new List<string>(lines);
            docTitlesList = MajorityVoting.RemoveGreekAccentSequential(list);
            Debug.Log("dimarxeio db loaded");
        }

        public void GetNextTextBlock()
        {
            if (!documentViewing) return;

            if (blocksIndex < blocksOfText.Count - 1)
            {
                blocksIndex++;
            }
            else
            {
                blocksIndex = 0;
            }

            try
            {
                text_result = blocksOfText[blocksIndex];
            }
            catch (Exception e)
            {
                Debug.LogError("text block out of bounds");
            }
            if (String.IsNullOrEmpty(text_result))
            {
                text_result = "Δεν αναγνωρίστηκε, ξαναπροσπαθήστε.";
            }
            UAP_AccessibilityManager.Say(text_result);
            EventCamManager.onProcessEnded?.Invoke();
        }

        public void GetPreviousTextBlock()
        {
            if (!documentViewing) return;

            if (blocksIndex < 0)
            {
                blocksIndex = blocksOfText.Count - 1;
            }
            else
            {
                blocksIndex--;
            }

            try
            {
                text_result = blocksOfText[blocksIndex];
            }
            catch (Exception e)
            {
                Debug.LogError("text block out of bounds");
            }
            EventCamManager.onProcessEnded?.Invoke();
        }

        public void DisplayDocumentText()
        {
            blocksIndex = 0;
            text_result = blocksOfText[0];
            documentViewing = true;
            UAP_AccessibilityManager.Say(text_result);
            EventCamManager.onProcessEnded?.Invoke();
        }

        private void SetTimeText()
        {
            if (TimeText != null)
            {
                TimeText = "Full process costed : " + "\nOCRtime: " + OCRtime.ToString() + "\nClassificationTime: " + classificationTime.ToString();
            }
        }

        public override void SaveScreenshot(Texture2D tex)
        {
            throw new System.NotImplementedException();
        }

        public async void ReadFaces(Texture2D tex)
        {
            faceAnnotationDone = false;
            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation
            annotationText = await annotator.PerformAnnotation(tex);

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            var faces = SplitStringToList(annotationText);
            var faceGroups = faces.GroupBy(x => x).ToDictionary(x => x.Key, x => x.ToList().Count);
            faceGroups.Remove("");
            foreach (var face in faceGroups)
            {
                text_result += ", " + face.Value.ToString() + " Πρόσωπα με συναισθήματα " + face.Key;
            }

            faceAnnotationDone = true;
        }

        public string GetFaceText(string annotatedText)
        {
            var returned_str = string.Empty;
            var faces = SplitStringToList(annotatedText);
            var faceGroups = faces.GroupBy(x => x).ToDictionary(x => x.Key, x => x.ToList().Count);
            faceGroups.Remove("");
            foreach (var face in faceGroups)
            {
                returned_str = $"{returned_str}, {face.Value.ToString()} πρόσωπα με συναισθήματα {face.Key}";
            }

            faceLogs = returned_str;
            return returned_str;
        }

        public async void ReadDocument(Texture2D tex)
        {
            docAnnotationDone = false;
            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation
            annotationText = await annotator.PerformAnnotation(tex);
            text_result = annotationText;

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            docAnnotationDone = true;
        }

        public async void ReadSign(Texture2D tex)
        {
            signAnnotationDone = false;
            float startOCRt = Time.realtimeSinceStartup;

            //ocr annotation
            annotationText = await annotator.PerformAnnotation(tex);
            text_result = annotationText;

            float endOCRt = Time.realtimeSinceStartup;
            OCRtime = CalculateTimeDifference(startOCRt, endOCRt);

            signAnnotationDone = true;
        }

        /// <summary>  
        /// This methods aims to classify a 2D texture based on the svm model that has been initialized 
        /// in the start method of this class.  
        /// </summary>
        /// <param name="input_Tex"> Texture2D obj</param>  
        /// <returns>Integer type</returns>
        private int ClassifyCategory(Texture2D input_tex)
        {
            float startclass = Time.realtimeSinceStartup;

            float endclass = Time.realtimeSinceStartup;
            classificationTime = CalculateTimeDifference(startclass, endclass);
            return 0;
        }

        #region Initializers



        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            annotator = GetComponent<IAnnotate>();
            httpLoader = GetComponent<HttpImageLoading>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            annotationProccessBusy = false;
            ReadDocumentTitles(Utils.getFilePath("Files/" + "dimarxeio_titles.txt"));
        }

        #endregion

        public override void SetResultLogs(Texture2D tex)
        {
            if (logging)
            {
                var sum = captureTime + OCRtime + classificationTime;
                string fileName = $"dimarxeio_{System.DateTime.Now.ToString()}";
                fileName = fileName.Replace(" ", "_");
                fileName = fileName.Replace(":", "_");
                fileName = fileName.Replace("/", "_");
                fileName = fileName.Replace(".", "");
                fileName = fileName.Replace("μμ", "MM");
                fileName = fileName.Replace("πμ", "PM");
                string imageName = LogManager.GetResultLogs("Image Name", fileName);
                string response = LogManager.GetResponseTime(captureTime.ToString(), OCRtime.ToString(),
                    classificationTime.ToString(), sum.ToString());
                string classificationResults = LogManager.GetResultLogs("Classification", category.ToString());
                string ocrResults = GetResultLogs("OCR Results", documentLogs);
                string faceResults = GetResultLogs("Face Results", faceLogs);
                string objectResults = GetResultLogs("Object Results", objectLogs);
                string logText = imageName + "\n" + response + "\n" + classificationResults + "\n" + ocrResults + 
                                 "\n" + faceResults + "\n" + objectResults + "\n\n";
                LogManager.SaveResultLogs(logText);
                SaveImageFile(tex, fileName);
                //StartCoroutine(LogManager.UploadLog("", $"{fileName}.jpg"));
            }
        }

        public void CancelButton()
        {
            //if (!annotationProccessBusy)
            //{
            //    return;
            //}
            StopAllCoroutines();
            UAP_AccessibilityManager.StopSpeaking();
            annotationProccessBusy = false;
            textureReceived = false;

            DestroyImmediate(processedTex);
            DestroyImmediate(camTex);
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Debug.Log("pc cancel");
        }
    }
}


//switch (PublicServiceManager.category)
//{
//case (int) Enums.PServiceCategories.document:
//UAP_AccessibilityManager.Say("Αναγνώριση εγγράφου");
//resultTextPanel.text = "Αναγνώριση εγγράφου";
//ReadDocument(processedTex);
//    while (docAnnotationDone != true)
//{
//    await frameBreak;
//}
//break;
//case (int) Enums.PServiceCategories.sign:
//// output message to user.
//UAP_AccessibilityManager.Say("Αναγνώριση πινακίδας");
//resultTextPanel.text = "Αναγνώριση πινακίδας";
//ReadSign(processedTex);
//    while (signAnnotationDone != true)
//{
//    await frameBreak;
//}
//break;
//case (int) Enums.PServiceCategories.face:
//UAP_AccessibilityManager.Say("Αναγνώριση προσώπου");
//resultTextPanel.text = "Αναγνώριση προσώπου";
//ReadFaces(processedTex);
//    while (faceAnnotationDone != true)
//{
//    await frameBreak;
//}
//break;
//case (int) Enums.PServiceCategories.other:
//break;
//}
