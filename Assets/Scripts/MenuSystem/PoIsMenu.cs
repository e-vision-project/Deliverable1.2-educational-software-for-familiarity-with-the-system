using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Extensions;

public class PoIsMenu : MonoBehaviour {
    public GameObject ScrollerContainer;
    private List<PointsOfInterest> allPointsSorted = new List<PointsOfInterest>();

    private void OnEnable()
    {
        for (int i = 0; i < ScrollerContainer.transform.childCount; i++)
        {
            Destroy(ScrollerContainer.transform.GetChild(i).gameObject);
        }
        Populate();
    }

    private void Populate()
    {
        allPointsSorted.Clear();
        for (int i = 0; i < AppManager.GpsManager.PointsOfInterest.Length; i++)
        {
            allPointsSorted.Add(AppManager.GpsManager.PointsOfInterest[i]);
        }

        allPointsSorted = allPointsSorted.OrderBy(o => o.Name).ToList();
        for (int i = 0; i < allPointsSorted.Count; i++)
        {
            GameObject pointObject = Instantiate(Resources.Load("Prefabs/PointUiItemSimple")) as GameObject;
            PointOfInterestUiItem uiItem = pointObject.GetComponent<PointOfInterestUiItem>();
            pointObject.transform.SetParent(ScrollerContainer.transform);
            int num = i + 1;
            uiItem.PointNumberText.text = num.ToString();
            uiItem.PointNameText.text = allPointsSorted[i].Name;
            uiItem.PointAccessibleButton.m_Text = allPointsSorted[i].Name ;
            GameObject pointCanvas = allPointsSorted[i].PointCanvas;
            uiItem.index = i;
            uiItem.PointButton.onClick.AddListener(() => {
                pointCanvas.SetActive(true);
                this.gameObject.SetActive(false);
            });
            if (i == (allPointsSorted.Count -1))
            {
                Destroy(uiItem.Seperator);
            }
        }

    }

    public void SeperatorCheckHighlight(int index)
    {
        for (int i = 0; i < ScrollerContainer.transform.childCount - 1; i++)
        {
            ScrollerContainer.transform.GetChild(i).GetComponent<PointOfInterestUiItem>().Seperator.SetActive(true);
        }

        if (index != ScrollerContainer.transform.childCount - 1)
        {
            ScrollerContainer.transform.GetChild(index).GetComponent<PointOfInterestUiItem>().Seperator.SetActive(false);
        }
        
        if ((index - 1) >= 0)
        {
            ScrollerContainer.transform.GetChild(index - 1).GetComponent<PointOfInterestUiItem>().Seperator.SetActive(false);
        }

        Debug.Log("Testing event on scroll " + index);
    }

    public void SeperatorCheckIddle(int index)
    {
        if (index != ScrollerContainer.transform.childCount - 1)
        {
            if (ScrollerContainer.transform.GetChild(index).GetComponent<PointOfInterestUiItem>().Seperator != null)
            {
                ScrollerContainer.transform.GetChild(index).GetComponent<PointOfInterestUiItem>().Seperator.SetActive(true);
            }            
        }
    }


    public IEnumerator WaitAndPopulate()
    {
        // Debug.Log("Waiting for GPS FIX");
        //Wait until the gps sensor provides a valid location.
        while (!AppManager.GpsManager.PointsSorted)
        {
            yield return null;
        }
        //Debug.Log("GPS FIXED");

    }
}
