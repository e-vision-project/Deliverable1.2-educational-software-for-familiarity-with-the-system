using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AddFaceResultMenu : MonoBehaviour
{
    public InputField AddNameInputField;
    public GameObject SaveNameButton;
    public RawImage displayImg;
    public Sprite FailSprite, SuccessSprite;

    private void OnEnable()
    {
        SaveNameButton.SetActive(false);        
    }

    private void Start()
    {
        AddNameInputField.onEndEdit.AddListener(delegate { EnableSaveButton(); });
    }

    private void EnableSaveButton()
    {
        SaveNameButton.SetActive(true);
        Invoke("IncrementUI", 0.1f);
    }

    private void IncrementUI()
    {
        AppManager.UapManager.IncrementUIElement();
    }

    public void SetResult(Texture faceSnapShot, bool resultStatus)
    {
        displayImg.texture = faceSnapShot;
        if (resultStatus)
        {
            SaveNameButton.GetComponent<Image>().sprite = SuccessSprite;
        }
        else
        {
            SaveNameButton.GetComponent<Image>().sprite = FailSprite;
        }
    }
}
