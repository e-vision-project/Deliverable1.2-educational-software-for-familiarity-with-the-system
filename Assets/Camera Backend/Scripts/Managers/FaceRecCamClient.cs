using System;
using System.Collections;
using System.Collections.Generic;
using EVISION.Camera.plugin;
using FaceRecognition;
using UnityEngine;
using UnityEngine.UI;

public class FaceRecCamClient : CameraClient
{
    // PRIVATE PROPERTIES
    private WaitForEndOfFrame frameBreak;

    // PUBLIC PROPERTIES
    public Text resultTextPanel;
    public FaceRecMng faceRec;

    // Start is called before the first frame update
    void Start()
    {
        frameBreak = new WaitForEndOfFrame();
        httpLoader = GetComponent<HttpImageLoading>();
    }

    public override async void ProcessScreenShotAsync()
    {
        displayImage.texture = null;

        annotationProccessBusy = true;
        resultTextPanel.text = "Επεξεργασία φωτογραφίας";

        // reference to the texture object.
        var processedTex = await GetScreenShot();
        while (!textureReceived)
        {
            await frameBreak;
        }

        SetDisplayImage(processedTex);
        SetResultLogs(processedTex);


        EventCamManager.onProcessEnded?.Invoke();

        annotationProccessBusy = false;
        textureReceived = false;
        await Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public override void SaveScreenshot(Texture2D camTexture)
    {
        throw new System.NotImplementedException();
    }

    public override void SetResultLogs(Texture2D currTex)
    {
        throw new System.NotImplementedException();
    }
}
