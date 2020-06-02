using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace EVISION.Camera.plugin
{
    public abstract class CameraClient : MonoBehaviour
    {
        // INTERFACES
        protected IAnnotate annotator;
        protected HttpImageLoading httpLoader;
        [SerializeField] protected static bool verboseMode;

        // PRIVATE PROPERTIES
        protected Texture2D camTex;
        protected float captureTime;
        protected WaitForEndOfFrame frameBreak;

        // PUBLIC PROPERTIES
        public static Enums.ScenarioCases scenarioCase;
        public bool externalCamera;
        public bool textureReceived;
        public RawImage displayImage;
        public bool annotationProccessBusy { get; set; }
        public string photoPath;
        public string text_result = "";


        #region Event Listeners

        public void AnnotationFailedHandler()
        {
            StopAllCoroutines();
            annotationProccessBusy = false;
        }

        public virtual void CancelButton()
        {
            Debug.Log("cancel in camera client");
        }
 
        public void ConnectNativeCamera()
        {
            Debug.Log("connecting native cam");
            annotationProccessBusy = false;
        }

        #endregion

        protected async Task<Texture2D> GetScreenShot()
        {
            camTex = new Texture2D(0,0);
            float startCapture = Time.realtimeSinceStartup;
            var frameBreak = new WaitForEndOfFrame();

            if (externalCamera)
            {
                camTex = await httpLoader.LoadTextureFromImage()
;               while (!textureReceived)
                {
                    await frameBreak;
                }
            }
            else
            {
                //camTex = Resources.Load<Texture2D>("Photos/" + $"debug_{idx}");
                camTex = CVUtils.LoadImgToTexture(photoPath, TextureFormat.RGBA32);
            }
            float endCapture = Time.realtimeSinceStartup;
            captureTime = GenericUtils.CalculateTimeDifference(startCapture, endCapture);
            textureReceived = true;
            Handheld.Vibrate();
            return camTex;
        }

        public void SetDisplayImage(Texture2D tex)
        {
            displayImage.texture = tex;
        }

        public static void SetVerbosity(bool value)
        {
            verboseMode = value;
        }

        public abstract void ProcessScreenShotAsync();
 
        public abstract void SaveScreenshot(Texture2D camTexture);

        public abstract void SetResultLogs(Texture2D currTex);

        public virtual void ScreenshotButtonListener()
        {
            if (annotationProccessBusy)
            {
                return;
            }
            //reset image loading properties.
            displayImage.texture = null;
            textureReceived = false;
            ProcessScreenShotAsync();
        }

        public void CheckCameraConnectionStatus()
        {
            if (externalCamera)
            {
                httpLoader.CheckConnectionStatus();
            }
        }

        public void UploadAnalytics()
        {
            Debug.Log("uploading analytics.....");
            StartCoroutine(LogManager.UploadLogs());
            RefreshEditorProjectWindow();
        }

        void RefreshEditorProjectWindow()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void DisposeCameraTexture()
        {
            Destroy(camTex);
        }
    }

}