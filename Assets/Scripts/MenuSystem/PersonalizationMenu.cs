using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonalizationMenu : MonoBehaviour {

    public Button ParaliaInputButton;
    public Toggle SuperMarketOutputToggle = null;
    public Text ParaliaInputSliderText;
    public GameObject ParaliaInputAuto, ParaliaInputLow, ParaliaInputMod, ParaliaInputHigh, ParaliaInputManual;

    public void OnEnable()
    {
        SetParaliaInput(AppManager.PlayerPrefsParaliaInput);
        Debug.Log("Super Market Detailed is: " + AppManager.PlayerPrefsSuperMarketDetailed);
        SuperMarketOutputToggle.isOn = AppManager.PlayerPrefsSuperMarketDetailed;
    }

    public void SetSuperMarketDetailed()
    {
        AppManager.SetSuperMarketDetailed(SuperMarketOutputToggle.isOn);
        Debug.Log("Sending bool to Supermarket Detailed" + SuperMarketOutputToggle.isOn);
    }

    private void SetParaliaInput(int status)
    {
        ParaliaInputAuto.SetActive(false);
        ParaliaInputLow.SetActive(false);
        ParaliaInputMod.SetActive(false);
        ParaliaInputHigh.SetActive(false);
        ParaliaInputManual.SetActive(false);
        if (status == 0)
        {
            ParaliaInputAuto.SetActive(true);
            ParaliaInputSliderText.text = "Αυτόματη";
        }
        else if (status == 1)
        {
            ParaliaInputLow.SetActive(true);
            ParaliaInputSliderText.text = "Χαμηλή";
        }
        else if (status == 2)
        {
            ParaliaInputMod.SetActive(true);
            ParaliaInputSliderText.text = "Μεσσαία";
        }
        else if (status == 3)
        {
            ParaliaInputHigh.SetActive(true);
            ParaliaInputSliderText.text = "Υψηλή";
        }
        else if (status == 4)
        {
            ParaliaInputManual.SetActive(true);
            ParaliaInputSliderText.text = "Χειροκίνητη";
        }
    }

    public void SetParaliaInput()
    {
        ParaliaInputAuto.SetActive(false);
        ParaliaInputLow.SetActive(false);
        ParaliaInputMod.SetActive(false);
        ParaliaInputHigh.SetActive(false);
        ParaliaInputManual.SetActive(false);
        if (AppManager.PlayerPrefsParaliaInput == 0)
        {
            AppManager.PlayerPrefsParaliaInput = 1;
            ParaliaInputLow.SetActive(true);
            ParaliaInputSliderText.text = "Χαμηλή";
            UAP_AccessibilityManager.Say("Συχνότητα λήψης, χαμηλή");
        }
        else if (AppManager.PlayerPrefsParaliaInput == 1)
        {
            AppManager.PlayerPrefsParaliaInput = 2;
            ParaliaInputMod.SetActive(true);
            ParaliaInputSliderText.text = "Μεσσαία";
            UAP_AccessibilityManager.Say("Συχνότητα λήψης, μεσσαία");
        }
        else if (AppManager.PlayerPrefsParaliaInput == 2)
        {
            AppManager.PlayerPrefsParaliaInput = 3;
            ParaliaInputHigh.SetActive(true);
            ParaliaInputSliderText.text = "Υψηλή";
            UAP_AccessibilityManager.Say("Συχνότητα λήψης, Υψηλή");
        }
        else if (AppManager.PlayerPrefsParaliaInput == 3)
        {
            AppManager.PlayerPrefsParaliaInput = 4;
            ParaliaInputManual.SetActive(true);
            ParaliaInputSliderText.text = "Χειροκίνητη";
            UAP_AccessibilityManager.Say("Συχνότητα λήψης, χειροκίνητη");
        }
        else if (AppManager.PlayerPrefsParaliaInput == 4)
        {
            AppManager.PlayerPrefsParaliaInput = 0;
            ParaliaInputAuto.SetActive(true);
            ParaliaInputSliderText.text = "Αυτόματη";
            UAP_AccessibilityManager.Say("Συχνότητα λήψης, αυτόματη");
        }
        AppManager.SetParaliaInput(AppManager.PlayerPrefsParaliaInput);
    }

    public void ResetOptions()
    {
        SuperMarketOutputToggle.isOn = true;
        AppManager.SetSuperMarketDetailed(true);
        SuperMarketOutputToggle.GetComponent<PlayMakerFSM>().SendEvent("Reset");

        ParaliaInputAuto.SetActive(false);
        ParaliaInputLow.SetActive(false);
        ParaliaInputMod.SetActive(false);
        ParaliaInputHigh.SetActive(false);
        ParaliaInputManual.SetActive(false);
        ParaliaInputAuto.SetActive(true);
        ParaliaInputSliderText.text = "Αυτόματη";
        AppManager.SetParaliaInput(0);
        ParaliaInputButton.GetComponent<PlayMakerFSM>().SendEvent("Reset");
        SetParaliaInput();
    }
}
