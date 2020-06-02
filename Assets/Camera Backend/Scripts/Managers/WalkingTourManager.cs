using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EVISION.Camera.plugin;
using FaceRecognition;
using OpenCVForUnity.CoreModule;
using TensorFlow.Utils;
using UnityEngine;
using UnityEngine.UI;
using TFShaqian;

namespace EVISION.Camera.plugin
{
    public class WalkingTourManager : CameraClient
    {
        // INSPECTOR PROPERTIES
        [SerializeField] private bool logging;
        [SerializeField] private FaceRecMng faceMng;

        // PRIVATE PROPERTIES
        private WaitForEndOfFrame frameBreak;
        private ObjectDetectionInference _detector;
        private bool inferenceComplete;
        private Texture2D processedTex;

        // PUBLIC PROPERTIES
        public Text resultTextPanel;

        //Logging
        private float detectionTime;

        // Start is called before the first frame update
        void Start()
        {
            frameBreak = new WaitForEndOfFrame();
            _detector = GetComponent<ObjectDetectionInference>();
            httpLoader = GetComponent<HttpImageLoading>();
        }

        public void UploadLogs()
        {
            StartCoroutine(LogManager.UploadLogs());
        }

        public override async void ProcessScreenShotAsync()
        {
            scenarioCase = Enums.ScenarioCases.tour;

            annotationProccessBusy = true;
            resultTextPanel.text = "Επεξεργασία φωτογραφίας";

            // reference to the texture object.
            processedTex = new Texture2D(0, 0);
            processedTex = await GetScreenShot();
            while (!textureReceived)
            {
                await frameBreak;
            }

            DetectObjects(processedTex);

            SetDisplayImage(processedTex);
            SetResultLogs(processedTex);

            if (String.IsNullOrEmpty(text_result))
            {
                text_result = "Δεν αναγνωρίστηκε, ξαναπροσπαθήστε.";
            }
            UAP_AccessibilityManager.Say(text_result);
            EventCamManager.onProcessEnded?.Invoke();

            annotationProccessBusy = false;
            inferenceComplete = false;
            textureReceived = false;
            await Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void DetectObjects(Texture2D input_tex)
        {
            text_result = "";

            float startOCRt = Time.realtimeSinceStartup;

            //face and object detection.
            var outputs = _detector.RunInference(input_tex);
            var face = faceMng.RecognizeFace(input_tex);

            var classNames = outputs.Select(x => x.Key).ToList();
            var classes = classNames.GroupBy(x => x).ToDictionary(x => x.Key, x => x.ToList().Count);
            foreach (var label in classes)
            {
                if (text_result == String.Empty)
                {
                    text_result = $"{label.Key} {label.Value}";
                }
                else
                {
                    text_result = $"{text_result}, {label.Key} {label.Value}";
                }
            }

            if (face != "Δεν εντοπίστηκε πρόσωπο" && face != "Δεν αναγνωρίστηκε")
            {
                text_result = $"{text_result}, εντοπίστηκε οικείο πρόσωπο με όνομα {face}";
            }

            float endOCRt = Time.realtimeSinceStartup;
            detectionTime = GenericUtils.CalculateTimeDifference(startOCRt, endOCRt);

            inferenceComplete = true;
        }

        public override void SaveScreenshot(Texture2D camTexture)
        {
            throw new System.NotImplementedException();
        }

        public override void SetResultLogs(Texture2D tex)
        {
            if (logging)
            {
                var sum = captureTime + detectionTime;
                string fileName = $"paralia_{System.DateTime.Now.ToString()}";
                fileName = fileName.Replace(" ", "_");
                fileName = fileName.Replace(":", "_");
                fileName = fileName.Replace("/", "_");
                fileName = fileName.Replace(".", "");
                fileName = fileName.Replace("μμ", "MM");
                fileName = fileName.Replace("πμ", "PM");
                string imageName = LogManager.GetResultLogs("Image Name", fileName);
                string response = LogManager.GetResponseTime(captureTime.ToString(), detectionTime.ToString(), sum.ToString());
                string detectionResults = LogManager.GetResultLogs("Detection", text_result.ToString());
                string logText = imageName + "\n" + response + "\n" + detectionResults + "\n\n";
                LogManager.SaveResultLogs(logText);
                LogManager.SaveImageFile(tex, fileName);
                //StartCoroutine( LogManager.UploadLog("", $"{fileName}.jpg"));
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
            //displayImage.texture = null;
            annotationProccessBusy = false;
            textureReceived = false;

            DestroyImmediate(processedTex);
            DestroyImmediate(camTex);
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Debug.Log("walking cancel");
        }
    }
}
