using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RemoveFaceMenu : MonoBehaviour
{

    public InputField NameToBeDeletedInputField;
    public GameObject ListContainer;


    private void OnEnable()
    {
        DestroyList();
        PopulateListAll();
    }

    public void PopulateListAll()
    {
        // Uncomment when Back End Dictionary is Ready (EKETA)

        //Check for null ref
        if (AppManager.FaceRecManager.StoredFaces.serializedDict == null)
        {
            Debug.Log("Dictionary is null");
            UAP_AccessibilityManager.Say("Δεν έχουν προστεθεί οικεία πρόσωπα");
            return;
        }

        // Get the whole dictionary 
        Dictionary<string, FaceRecognition.FaceEmbeddings> facesDictionary = AppManager.FaceRecManager.StoredFaces.serializedDict;
        if (facesDictionary.Count == 0)
        {
            Debug.Log("Dictionary is empty");
            UAP_AccessibilityManager.Say("Δεν έχουν προστεθεί οικεία πρόσωπα");
            return;
        }

        //Populate Scroller
        int index = 0;
        foreach (KeyValuePair<string, FaceRecognition.FaceEmbeddings> entry in facesDictionary)
        {
            //Instantiate Prefab from Resources
            GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryUiItem")) as GameObject;

            //Get the RemoveFaceUIItem script
            RemoveFaceUIItem uiItem = pointObject.GetComponent<RemoveFaceUIItem>();

            //Set object's parent to the list Container
            pointObject.transform.SetParent(ListContainer.transform);

            //Every second item change the background color
            if (index % 2 != 0)
            {
                //Changing the color to white
                uiItem.BackgroundColor.color = Color.white;
            }

            //Set the UI text to the dictionary's key
            uiItem.NameText.text = entry.Key;

            //Save the string to a value for using it in the Listener
            string dictionaryKey = entry.Key;

            //Setting the event for clicking each Face for deletion
            uiItem.DeleteButton.onClick.AddListener(() =>
            {

                //Initializing the next menu
                AppManager.RemoveFaceConfirmMenu.InitializeMenu(dictionaryKey);

                //Disabling this menu
                this.gameObject.SetActive(false);
            });
            index++;
        }

        //Hacking a bit the scroller so that faces start on top
        //We are adding empty items to populate the list
        if (facesDictionary.Count < 6)
        {
            switch (facesDictionary.Count)
            {
                case (5):
                    for (int i = 0; i < 1; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (4):
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (3):
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (2):
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (1):
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                default:
                    break;
            }
        }



        //TEST loop for debuging
        //for (int i = 0; i < 10; i++)
        //{
        //    //Instantiate Prefab from Resources
        //    GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryUiItem")) as GameObject;

        //    //Get the RemoveFaceUIItem script
        //    RemoveFaceUIItem uiItem = pointObject.GetComponent<RemoveFaceUIItem>();

        //    //Set object's parent to the list Container
        //    pointObject.transform.SetParent(ListContainer.transform);

        //    //Every second item change the background color
        //    if (i % 2 != 0)
        //    {
        //        //Changing the color to white
        //        uiItem.BackgroundColor.color = Color.white;
        //    }

        //    //Set the UI text to the dictionary's key
        //    uiItem.NameText.text = "Ανθή" + i;

        //    //Save the string to a value for using it in the Listener
        //    string dictionaryKey = "Ανθή"; //<---- Must ADD the string key from dictionary 

        //    //Setting the event for clicking each Face for deletion
        //    uiItem.DeleteButton.onClick.AddListener(() =>
        //    {

        //        //Initializing the next menu
        //        AppManager.RemoveFaceConfirmMenu.InitializeMenu(dictionaryKey);

        //        //Disabling this menu
        //        this.gameObject.SetActive(false);
        //    });
        //}
    }

    private void DestroyList()
    {
        //Destroying all children of the scroll list
        for (int i = 0; i < ListContainer.transform.childCount; i++)
        {
            Destroy(ListContainer.transform.GetChild(i).gameObject);
        }
    }

    public void PopulateList(Dictionary<string, Texture> faceDictionary)
    {
        // Emptying the list
        DestroyList();

        //Setting up indexing for making the background white
        int index = 0;

        //Adding items in the Scroll list looping through the Dictionary
        foreach (KeyValuePair<string, Texture> entry in faceDictionary)
        {
            //Instantiate Prefab from Resources
            GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryUiItem")) as GameObject;

            //Get the RemoveFaceUIItem script
            RemoveFaceUIItem uiItem = pointObject.GetComponent<RemoveFaceUIItem>();

            //Set object's parent to the list Container
            pointObject.transform.SetParent(ListContainer.transform);

            //Every second item change the background color
            if (index % 2 != 0)
            {
                uiItem.BackgroundColor.color = Color.white;
            }

            //Set the UI text to the dictionary's key
            uiItem.NameText.text = entry.Key;

            //Save the string to a value for using it in the Listener
            string dictionaryKey = entry.Key;

            //Setting the event for clicking each Face for deletion
            uiItem.DeleteButton.onClick.AddListener(() => {

                //Initializing the next menu
                AppManager.RemoveFaceConfirmMenu.InitializeMenu(dictionaryKey);

                //Disabling this menu
                this.gameObject.SetActive(false);
            }); 

            //Increasing the indexing
            index++;
        }

        //Hacking a bit the scroller so that faces start on top
        //We are adding empty items to populate the list
        if (faceDictionary.Count < 6)
        {
            switch (faceDictionary.Count)
            {
                case (5):
                    for (int i = 0; i < 1; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (4):
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (3):
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (2):
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                case (1):
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/FaceRecEntryEmptyUiItem")) as GameObject;
                        pointObject.transform.SetParent(ListContainer.transform);
                    }
                    break;
                default:
                    break;
            }
        }

    }

}
