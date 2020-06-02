using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FaceRecResultMenu : MonoBehaviour
{
    public Text FaceRecResultText;
    public RawImage FaceSnapShotImage;
    public GameObject CorrectResultIcon, WrongResultIcon;

    public void SetResult(string name, Texture2D faceSnapShot, bool resultStatus)
    {
        if (resultStatus)
        {
            CorrectResultIcon.SetActive(true);
            WrongResultIcon.SetActive(false);
        }
        else
        {
            CorrectResultIcon.SetActive(false);
            WrongResultIcon.SetActive(true);
        }
        FaceRecResultText.text = name;
        FaceSnapShotImage.texture = faceSnapShot;
    }
}
