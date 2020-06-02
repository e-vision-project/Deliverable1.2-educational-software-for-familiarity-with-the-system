using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EVISION.Camera.plugin;

public class ResultText : MonoBehaviour
{
    private Text result_text;
    public CameraClient client;

    // Start is called before the first frame update
    void Awake()
    {
        result_text = gameObject.GetComponent<Text>();
        EventCamManager.onProcessEnded += SetText;
        EventCamManager.onAnnotationFailed += AnnotationFailed;
        //EventCamManager.onProcessEnded += SpeakResult;
    }

    public void AnnotationFailed()
    {
        result_text.text = "Δεν υπάρχει σύνδεση στο δίκτυο. Ενεργοποιήστε τα δεδομένα κινητής τηλεφωνίας.";
    }

    public void SetText()
    {
        if (String.IsNullOrEmpty(client.text_result))
        {
            result_text.text = "Δεν αναγνωρίστηκε, ξαναπροσπαθήστε.";
        }
        else
        {
            result_text.text = client.text_result;
        }
    }

    public void SpeakResult()
    {
        //if (String.IsNullOrEmpty(client.text_result))
        //{
        //    client.text_result = "Δεν αναγνωρίστηκε, ξαναπροσπαθήστε.";
        //}
        //UAP_AccessibilityManager.Say(client.text_result);
        //client.text_result = "";
    }
}
