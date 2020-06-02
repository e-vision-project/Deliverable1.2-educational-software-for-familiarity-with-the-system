using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour {

    private bool m_CamAvailable;
    private WebCamTexture m_BackCam;
    private Texture m_DefaultBackground;

    public RawImage m_Background;
    public AspectRatioFitter m_Fit;

    private void Start()
    {

        m_DefaultBackground = m_Background.texture;
        WebCamDevice[] webCamDevices = WebCamTexture.devices;

        if (webCamDevices.Length == 0)
        {
            m_CamAvailable = false;
            return;
        }

        for (int i = 0; i < webCamDevices.Length; i++)
        {
#if UNITY_EDITOR
            if (webCamDevices[i].isFrontFacing)
#else
            if (!webCamDevices[i].isFrontFacing)
#endif
            {
                m_BackCam = new WebCamTexture(webCamDevices[i].name, (int)m_Background.rectTransform.rect.width, (int)m_Background.rectTransform.rect.height);

            }
        }

        if (m_BackCam == null)
        {
            Debug.Log("Unable to find back camera");
            return;
        }
    }

    public void BeginCamera()
    {
        Debug.Log("Begin Camera");
        m_BackCam.Play();
        m_Background.texture = m_BackCam;
        m_CamAvailable = true;

    }

    public void StopCamera()
    {
        m_BackCam.Stop();
        m_Background.texture = null;
        m_CamAvailable = false;
    }
    //EKETA : αυτή τη μέθοδο μπορείτε να την καλείτε όταν θα βγάζει screenshot ο χρήστης
    //        προς ανάλυση.
    public void TakeScreenshot()
    {

    }

    private void Update()
    {
        if (!m_CamAvailable)
        {
            return;
        }

        //float ratio = (float)m_BackCam.width / (float)m_BackCam.height;
        //m_Fit.aspectRatio = ratio;

        float scaleY = m_BackCam.videoVerticallyMirrored ? -1f : 1f;
        m_Background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -m_BackCam.videoRotationAngle;
        m_Background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

    }

}
