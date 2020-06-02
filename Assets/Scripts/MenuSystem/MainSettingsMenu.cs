using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSettingsMenu : MonoBehaviour {

    public Toggle Vibration;
    public Toggle VoiceCommands;


    public void OnEnable()
    {
        Vibration.isOn =AppManager.PlayerPrefsVibration;
    }


    public void SetVibration()
    {
        AppManager.SetVibration(Vibration.isOn);
    }

    public void SetVoiceCommands()
    {
        //AppManager.SetVoiceCommands(VoiceCommands.isOn);
    }

}
