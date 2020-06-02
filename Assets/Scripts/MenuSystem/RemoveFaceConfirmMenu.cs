using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RemoveFaceConfirmMenu : MonoBehaviour
{
    public Text NameToBeDeleted;
    public Button DeleteFaceButton;
    public GameObject SuccessIcon, SuccessLine;

    private string m_FaceDictionaryKey;

    public void InitializeMenu(string dictionaryKey)
    {
        //Pass the name (key) of the face to the UI Text
        NameToBeDeleted.text = dictionaryKey;

        //Save key to variable
        m_FaceDictionaryKey = dictionaryKey;

        //Add a method in the button's click listener
        DeleteFaceButton.onClick.AddListener(() => DeleteSequence() );

        //Enable Menu
        this.gameObject.SetActive(true);
    }

    private void DeleteSequence()
    {
        //Call the delete function from BackEnd (EKETA)
        AppManager.FaceRecManager.DeleteFace(m_FaceDictionaryKey);
        DeleteStatus(true);
    }

    public void DeleteStatus(bool deletionStatus)
    {
        if (deletionStatus)  //Deletion Successed - Make appropriate actions
        {
            //Enable success icon
            SuccessIcon.SetActive(true);

            //Enable success line icon
            SuccessLine.SetActive(true);

            //Pass the result to user
            UAP_AccessibilityManager.Say("Ο χρήστης " + m_FaceDictionaryKey + " διαγράφτηκε επιτυχώς");

        }
        else    //Deletion Failed - Make appropriate actions
        {
            //Disable success icon
            SuccessIcon.SetActive(false);

            //Disable success line
            SuccessLine.SetActive(false);

            //Pass the result to user
            UAP_AccessibilityManager.Say("Η διεγραφή ήταν ανεπιτυχής");
        }
    }

}
