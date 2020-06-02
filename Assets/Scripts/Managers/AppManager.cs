using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using FaceRecognition;
using EVISION.Camera.plugin;

public class AppManager : MonoBehaviour {

    public static bool PlayerPrefsVibration;
    public static bool PlayerPrefsPoiRadar;
    public static float PlayerPrefsSpeechSpeed;
    public static bool PlayerPrefsEcoMode;
    public static bool PlayerPrefsSuperMarketDetailed;
    public static int PlayerPrefsParaliaInput;

    public static GameObject LastOpenCanvas;
    public static GpsManager GpsManager;
    public static UAP_AccessibilityManager UapManager;

    private static Canvas[] m_AllCanvases;
    public static GameObject IntroMenu;
    public static GameObject MainMenu;
    public static GameObject EcoModeCanvas;
    public static RadarPoiPopupMenu RadarPoiPopup;
    public static SupermarketResultMenu SuperMarketResultsMenu;
    public static DimosiaIpiresiaResultsMenu DimosiaIpiresiaResultsMenu;
    public static AnagnosiPerivResultsMenu AnagnosiPerivalontosResultsMenu;
    public static FaceRecResultMenu FaceRecResultMenu;
    public static AddFaceResultMenu AddFaceResultMenu;
    public static PoIsMenu PoIsMenu;
    public static RemoveFaceMenu RemoveFaceMenu;
    public static RemoveFaceConfirmMenu RemoveFaceConfirmMenu;
    public static FaceRecMng FaceRecManager;

    private void Awake()
    {
        m_AllCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        for (int i = 0; i < m_AllCanvases.Length; i++)
        {
            if (m_AllCanvases[i].name == "Intro Menu")
            {
                IntroMenu = m_AllCanvases[i].gameObject;
            }
            if (m_AllCanvases[i].name == "Main Menu")
            {
                MainMenu = m_AllCanvases[i].gameObject;
            }
            if (m_AllCanvases[i].name == "EcoModeCanvas")
            {
                EcoModeCanvas = m_AllCanvases[i].gameObject;
            }
            if (m_AllCanvases[i].name == "RadarPoiPopup Menu")
            {
                RadarPoiPopup = m_AllCanvases[i].gameObject.GetComponent< RadarPoiPopupMenu>();
            }
            if (m_AllCanvases[i].name == "SupermarketResult Menu")
            {
                SuperMarketResultsMenu = m_AllCanvases[i].gameObject.GetComponent<SupermarketResultMenu>();
            }
            if (m_AllCanvases[i].name == "DimosiaIpiresiaResult Menu")
            {
                DimosiaIpiresiaResultsMenu = m_AllCanvases[i].gameObject.GetComponent<DimosiaIpiresiaResultsMenu>();
            }
            if (m_AllCanvases[i].name == "AnagnosiPerivalontosResult Menu")
            {
                AnagnosiPerivalontosResultsMenu = m_AllCanvases[i].gameObject.GetComponent<AnagnosiPerivResultsMenu>();
            }
            if (m_AllCanvases[i].name == "Face Rec Result Menu")
            {
                FaceRecResultMenu = m_AllCanvases[i].gameObject.GetComponent<FaceRecResultMenu>();
            }
            if (m_AllCanvases[i].name == "AddFace Result Menu")
            {
                AddFaceResultMenu = m_AllCanvases[i].gameObject.GetComponent<AddFaceResultMenu>();
            }
            if (m_AllCanvases[i].name == "RemoveFace Menu")
            {
                RemoveFaceMenu = m_AllCanvases[i].gameObject.GetComponent<RemoveFaceMenu>();
            }
            if (m_AllCanvases[i].name == "RemoveFace Confirm Menu")
            {
                RemoveFaceConfirmMenu = m_AllCanvases[i].gameObject.GetComponent<RemoveFaceConfirmMenu>();
            }
            if (m_AllCanvases[i].name == "PoIs Menu")
            {
                PoIsMenu = m_AllCanvases[i].gameObject.GetComponent<PoIsMenu>();
            }
        }
        GpsManager = GetComponent<GpsManager>();
        UapManager = GameObject.Find("Accessibility Manager").GetComponent<UAP_AccessibilityManager>();
        if (GameObject.Find("Face Rec Mng") == null)
        {
            return;
        }
        FaceRecManager = GameObject.Find("Face Rec Mng").GetComponent<FaceRecMng>();

        //CloseAllCanvases();
    }

    public static void CloseAllCanvases()
    {
        for (int i = 0; i < m_AllCanvases.Length; i++)
        {
            if (m_AllCanvases[i].gameObject.activeInHierarchy && m_AllCanvases[i].name != "Canvas" && m_AllCanvases[i].name != "RadarPoiPopup Menu")
            {
                Debug.Log("Canvas " + m_AllCanvases[i].gameObject.name + " IS ACTIVE");
                LastOpenCanvas = m_AllCanvases[i].gameObject;
                FsmVariables.GlobalVariables.FindFsmGameObject("LastOpenCanvas").Value = LastOpenCanvas;

                m_AllCanvases[i].gameObject.SetActive(false);
            }
        }
    }

    // Use this for initialization
    void Start()
    {

        if (!PlayerPrefs.HasKey("FirstTime") )
        {
            PlayerPrefs.SetInt("FirstTime", 0);
            //StartTutorial();
        }


        if (!PlayerPrefs.HasKey("Vibration"))
        {
            PlayerPrefs.SetInt("Vibration", 1);
            PlayerPrefsVibration = true;
            FsmVariables.GlobalVariables.FindFsmBool("Vibration").Value = PlayerPrefsVibration;
        }
        else
        {
            PlayerPrefsVibration = PlayerPrefs.GetInt("Vibration") == 0 ? false : true;
            FsmVariables.GlobalVariables.FindFsmBool("Vibration").Value = PlayerPrefsVibration;
        }

        if (!PlayerPrefs.HasKey("SuperMarketDetailed"))
        {
            PlayerPrefs.SetInt("SuperMarketDetailed", 1);
            PlayerPrefsSuperMarketDetailed = true;
            FsmVariables.GlobalVariables.FindFsmBool("SuperMarketDetailed").Value = PlayerPrefsSuperMarketDetailed;
            // TODO: Add call to EKETA function for setting Verbus/Non verbus setting.
            MasoutisManager.SetVerbosity(true);
        }
        else
        {
            PlayerPrefsSuperMarketDetailed = PlayerPrefs.GetInt("SuperMarketDetailed") == 0 ? false : true;
            FsmVariables.GlobalVariables.FindFsmBool("SuperMarketDetailed").Value = PlayerPrefsSuperMarketDetailed;
            // TODO: Add call to EKETA function for setting Verbus/Non verbus setting.
            MasoutisManager.SetVerbosity(PlayerPrefsSuperMarketDetailed);
        }

        if (!PlayerPrefs.HasKey("PoiRadar"))
        {
            PlayerPrefs.SetInt("PoiRadar", 1);
            PlayerPrefsPoiRadar = true;
            FsmVariables.GlobalVariables.FindFsmBool("PoiRadar").Value = PlayerPrefsPoiRadar;
        }
        else
        {
            PlayerPrefsPoiRadar = PlayerPrefs.GetInt("PoiRadar") == 0 ? false : true;
            FsmVariables.GlobalVariables.FindFsmBool("PoiRadar").Value = PlayerPrefsPoiRadar;
        }

        if (!PlayerPrefs.HasKey("SpeechTone"))
        {
            PlayerPrefs.SetInt("SpeechTone", 1);
            PlayerPrefsSpeechSpeed = PlayerPrefs.GetInt("SpeechTone");
            FsmVariables.GlobalVariables.FindFsmFloat("SpeechTone").Value = PlayerPrefsSpeechSpeed;
            print("PASSED");
        }
        else
        {
            PlayerPrefsSpeechSpeed = PlayerPrefs.GetInt("SpeechTone");
        }

        if (!PlayerPrefs.HasKey("ParaliaInput"))
        {
            PlayerPrefs.SetInt("ParaliaInput", 0);
            PlayerPrefsParaliaInput = PlayerPrefs.GetInt("ParaliaInput");
            FsmVariables.GlobalVariables.FindFsmInt("ParaliaInput").Value = PlayerPrefsParaliaInput;
            // TODO: Add call to EKETA function for setting Paralia Snapshot Frequency setting.

        }
        else
        {
            PlayerPrefsParaliaInput = PlayerPrefs.GetInt("ParaliaInput");
            FsmVariables.GlobalVariables.FindFsmInt("ParaliaInput").Value = PlayerPrefsParaliaInput;

            // TODO: Add call to EKETA function for setting Paralia Snapshot Frequency setting.
        }

        if (!PlayerPrefs.HasKey("EcoMode"))
        {
            PlayerPrefs.SetInt("EcoMode", 0);
            PlayerPrefsEcoMode = false;
            FsmVariables.GlobalVariables.FindFsmBool("EcoMode").Value = PlayerPrefsEcoMode;
        }
        else
        {
            PlayerPrefsEcoMode = PlayerPrefs.GetInt("EcoMode") == 0 ? false : true;
            FsmVariables.GlobalVariables.FindFsmBool("EcoMode").Value = PlayerPrefsEcoMode;
            EcoModeCanvas.SetActive(PlayerPrefsEcoMode == true ? true : false);          
        }
        PlayerPrefs.Save();
    }
    
    public void StartTutorial()
    {
        MainMenu.SetActive(false);
        IntroMenu.SetActive(true);
        //REMEMBER TO UNCOMMENT ON RELEASE
        PlayerPrefs.SetInt("FirstTime", 1);
    }

    public static void SetVibration(bool status)
    {
        PlayerPrefs.SetInt("Vibration", status ? 1 : 0);
        PlayerPrefsVibration = status;
        FsmVariables.GlobalVariables.FindFsmBool("Vibration").Value = PlayerPrefsVibration;
        
        PlayerPrefs.Save();
    }

    public static void SetSuperMarketDetailed(bool status)
    {
        PlayerPrefs.SetInt("SuperMarketDetailed", status ? 1 : 0);
        PlayerPrefsSuperMarketDetailed = status;
        FsmVariables.GlobalVariables.FindFsmBool("SuperMarketDetailed").Value = PlayerPrefsSuperMarketDetailed;
        // TODO: Add call to EKETA function for setting Verbus/Non verbus setting.
        MasoutisManager.SetVerbosity(status);
        PlayerPrefs.Save();
    }

    public static void SetPoiRadar(bool status)
    {
        PlayerPrefs.SetInt("PoiRadar", status ? 1 : 0);
        PlayerPrefsPoiRadar = status;
        FsmVariables.GlobalVariables.FindFsmBool("PoiRadar").Value = PlayerPrefsPoiRadar;

        PlayerPrefs.Save();
    }

    public static void SetEcoMode(bool status)
    {
        PlayerPrefs.SetInt("EcoMode", status ? 1 : 0);
        PlayerPrefsEcoMode = status;
        FsmVariables.GlobalVariables.FindFsmBool("EcoMode").Value = PlayerPrefsEcoMode;
        EcoModeCanvas.SetActive(PlayerPrefsEcoMode == true ? true : false);
        PlayerPrefs.Save();
    }

    public static void SetParaliaInput(int status)
    {
        PlayerPrefs.SetInt("ParaliaInput", status);
        PlayerPrefsParaliaInput = status;
        // TODO: Add call to EKETA function for setting Paralia Snapshot Frequency setting.
        PlayerPrefs.Save();
    }

    public static void SetVoiceSpeed(int status)
    {
        PlayerPrefs.SetInt("SpeechTone", status);
        PlayerPrefsSpeechSpeed = status;
        PlayerPrefs.Save();
    }

    public void IncrementUapUIElement()
    {
        UapManager.IncrementUIElement();
    }

    public void DecrementUapUIElement()
    {
        UapManager.DecrementUIElement();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(0); 
    }

    public static void SetSupermarketResults(string text, Sprite screenshot)
    {
        SuperMarketResultsMenu.ResultText.text = text;
        SuperMarketResultsMenu.ScreenshotImage.sprite = screenshot;

        SuperMarketResultsMenu.gameObject.SetActive(true);
        SuperMarketResultsMenu.SayResultsButton.onClick.AddListener(()=> {
            UAP_AccessibilityManager.Say(text);
        });
    }

    public static void SetDimosiaIpiresiaResults(string text, Sprite screenshot)
    {
        DimosiaIpiresiaResultsMenu.ResultText.text = text;
        DimosiaIpiresiaResultsMenu.ScreenshotImage.sprite = screenshot;

        DimosiaIpiresiaResultsMenu.gameObject.SetActive(true);
        DimosiaIpiresiaResultsMenu.SayResultsButton.onClick.AddListener(() => {
            UAP_AccessibilityManager.Say(text);
        });
    }

    public static void SetAnagnosiPerivalontosResults(string text, Sprite screenshot)
    {
        AnagnosiPerivalontosResultsMenu.ResultText.text = text;
        AnagnosiPerivalontosResultsMenu.ScreenshotImage.sprite = screenshot;

        AnagnosiPerivalontosResultsMenu.gameObject.SetActive(true);
        AnagnosiPerivalontosResultsMenu.SayResultsButton.onClick.AddListener(() => {
            UAP_AccessibilityManager.Say(text);
        });
    }

}
