using System.Collections;
using System;
using UnityEngine;

public class AroundMeMenu : MonoBehaviour
{

    public GameObject ScrollerContainer;


    private void OnEnable()
    {
        for (int i = 0; i < ScrollerContainer.transform.childCount; i++)
        {
            Destroy(ScrollerContainer.transform.GetChild(i).gameObject);
        }
        StartCoroutine(WaitAndPopulate());
    }

    private void Populate()
    {


        for (int i = 0; i < AppManager.GpsManager.SortedPoints.Count; i++)
        {
            GameObject pointObject = Instantiate(Resources.Load("Prefabs/PointUiItem")) as GameObject;
            PointOfInterestUiItem uiItem = pointObject.GetComponent<PointOfInterestUiItem>();
            pointObject.transform.SetParent(ScrollerContainer.transform);
            int num = i + 1;
            uiItem.PointNumberText.text = num.ToString();
            uiItem.PointNameText.text = AppManager.GpsManager.SortedPoints[i].Name;

            if (AppManager.GpsManager.SortedPoints[i].Name == "Αρχαιολογικό Μουσείο")
            {
                uiItem.PointDistanceText.text = (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 300).ToString() + " μ.";
                uiItem.PointAccessibleButton.m_Text = AppManager.GpsManager.SortedPoints[i].Name +
               " ,  Απόσταση " + (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 300).ToString() +
               " μέτρα, εκτιμώμενος χρόνος" + AppManager.GpsManager.SortedPoints[i].WalkTimeToPoint + " λεπτά";
            }
            else if (AppManager.GpsManager.SortedPoints[i].Name == "Μουσείο Βυζαντινού Πολιτισμού")
            {
                uiItem.PointDistanceText.text = (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 300).ToString() + " μ.";
                uiItem.PointAccessibleButton.m_Text = AppManager.GpsManager.SortedPoints[i].Name +
               " ,  Απόσταση " + (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 300).ToString() +
               " μέτρα, εκτιμώμενος χρόνος" + AppManager.GpsManager.SortedPoints[i].WalkTimeToPoint + " λεπτά";
            }
            else if (AppManager.GpsManager.SortedPoints[i].Name == "Βασιλικό Θέατρο")
            {
                uiItem.PointDistanceText.text = (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 120).ToString() + " μ.";
                uiItem.PointAccessibleButton.m_Text = AppManager.GpsManager.SortedPoints[i].Name +
               " ,  Απόσταση " + (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger + 120).ToString() +
               " μέτρα, εκτιμώμενος χρόνος" + AppManager.GpsManager.SortedPoints[i].WalkTimeToPoint + " λεπτά";
            }
            else
            {
                uiItem.PointDistanceText.text = (AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger).ToString() + " μ.";
                uiItem.PointAccessibleButton.m_Text = AppManager.GpsManager.SortedPoints[i].Name +
               " ,  Απόσταση " + AppManager.GpsManager.SortedPoints[i].DistanceFromTrigger +
               " μέτρα, εκτιμώμενος χρόνος" + AppManager.GpsManager.SortedPoints[i].WalkTimeToPoint + " λεπτά";
            }
            
            uiItem.PointWalkTimeText.text = AppManager.GpsManager.SortedPoints[i].WalkTimeToPoint.ToString() + " λ.";
           
            GameObject pointCanvas = AppManager.GpsManager.SortedPoints[i].PointCanvas;
            uiItem.PointButton.onClick.AddListener(() =>
            {
                pointCanvas.SetActive(true);
                this.gameObject.SetActive(false);
            });
        }
        if (AppManager.GpsManager.SortedPoints.Count < 4)
        {
            switch (AppManager.GpsManager.SortedPoints.Count)
            {
                case (3):
                    for (int i = 0; i < 1; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/EmptyPointUiItem")) as GameObject;
                        pointObject.transform.SetParent(ScrollerContainer.transform);
                    }
                    break;
                case (2):
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/EmptyPointUiItem")) as GameObject;
                        pointObject.transform.SetParent(ScrollerContainer.transform);
                    }
                    break;
                case (1):
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/EmptyPointUiItem")) as GameObject;
                        pointObject.transform.SetParent(ScrollerContainer.transform);
                    }
                    break;
                case (0):
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject pointObject = Instantiate(Resources.Load("Prefabs/EmptyPointUiItem")) as GameObject;
                        pointObject.transform.SetParent(ScrollerContainer.transform);
                    }
                    break;
                default:
                    break;
            }
        }
        StartCoroutine(WaitAndTalk());
    }

    private IEnumerator WaitAndTalk()
    {
        yield return new WaitForSeconds(3);
        if (AppManager.GpsManager.SortedPoints.Count > 0)
        {
            UAP_AccessibilityManager.StopSpeaking();
            UAP_AccessibilityManager.Say("Τα παρακάτω σημεία ενδιαφέροντος βρίσκονται σε εμβέλεια 150 μέτρων ");
        }
        else
        {
            UAP_AccessibilityManager.StopSpeaking();
            UAP_AccessibilityManager.Say("Όλα τα σημεία ενδιαφέροντος βρίσκονται σε εμβέλεια μεγαλύτερη των 150 μέτρων.");
        }
    }

    public IEnumerator WaitAndPopulate()
    {
        // Debug.Log("Waiting for GPS FIX");
        //Wait until the gps sensor provides a valid location.
        while (!AppManager.GpsManager.PointsSorted && !AppManager.GpsManager.gpsFix)
        {
            yield return null;
        }
        //Debug.Log("GPS FIXED");
        Populate();
    }
}
