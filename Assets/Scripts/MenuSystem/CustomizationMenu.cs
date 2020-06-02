using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationMenu : MonoBehaviour {

    public Slider SpeechRateSlider = null;
    public Toggle Vibration = null;
    public Toggle EcoModeToggle = null;

    //////////////////////////////////////////////////////////////////////////

    public void OnEnable()
    {
        SpeechRateSlider.value = UAP_AccessibilityManager.GetSpeechRate();
        
        Vibration.isOn = AppManager.PlayerPrefsVibration;
        EcoModeToggle.isOn = AppManager.PlayerPrefsEcoMode;
    }


    //////////////////////////////////////////////////////////////////////////

    public void SetVibration()
    {
        AppManager.SetVibration(Vibration.isOn);
    }

    public void SetEcoMode()
    {
        AppManager.SetEcoMode(EcoModeToggle.isOn);
    }

    public void OnSpeechRateSliderChanged()
    {
        UAP_AccessibilityManager.SetSpeechRate(Mathf.RoundToInt(SpeechRateSlider.value));
    }

    public void ResetOptions()
    {
        SpeechRateSlider.value = 50;
        UAP_AccessibilityManager.SetSpeechRate(50);

        Vibration.isOn = true;
        AppManager.SetVibration(true);
        Vibration.GetComponent<PlayMakerFSM>().SendEvent("Reset");


        EcoModeToggle.isOn = false;
        AppManager.SetEcoMode(false);
        EcoModeToggle.GetComponent<PlayMakerFSM>().SendEvent("Reset");
    }
}
