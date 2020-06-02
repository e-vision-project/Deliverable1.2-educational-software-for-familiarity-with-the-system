using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HutongGames.PlayMaker;

public struct PinPoint
{
    public double Latitude, Longitude;

    public PinPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

public class GpsManager : MonoBehaviour {
    private int maxWait = 30;
    private float initTime = 3;
    public bool gpsFix;
    private float updateRate = 0.8f;
    public List<PointsOfInterest> SortedPoints = new List<PointsOfInterest>();

    public bool PointsSorted { get; private set; }

    public PointsOfInterest[] PointsOfInterest;

    private bool m_CheckDistanceForPopup;
    private bool m_DoPopupCalculations;


    private void PoiDistanceCheckForPopup()
    {
        Debug.Log("Calculating POpus");

        if (AppManager.RadarPoiPopup.gameObject.activeInHierarchy == false)
        {
            for (int i = 0; i < PointsOfInterest.Length; i++)
            {
                if (PointsOfInterest[i].DistanceFromTrigger <= 30)
                {
                    if (!PointsOfInterest[i].HaveOpenedOnce)
                    {

                        AppManager.CloseAllCanvases();
                        AppManager.RadarPoiPopup.PoiText.text = PointsOfInterest[i].Name;

                        if (PointsOfInterest[i].Name == "Αρχαιολογικό Μουσείο")
                        {
                            AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 300).ToString() + " μ.";
                        }
                        else if (PointsOfInterest[i].Name == "Μουσείο Βυζαντινού Πολιτισμού")
                        {
                            AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 300).ToString() + " μ.";
                        }
                        else if (PointsOfInterest[i].Name == "Βασιλικό Θέατρο")
                        {
                            AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 120).ToString() + " μ.";
                        }
                        else
                        {
                            AppManager.RadarPoiPopup.DistanceToPoint.text = PointsOfInterest[i].DistanceFromTrigger.ToString() + " μ.";
                        }

                        AppManager.RadarPoiPopup.WalkingTime.text = PointsOfInterest[i].WalkTimeToPoint.ToString() + " λ.";
                        AppManager.RadarPoiPopup.Map.sprite = PointsOfInterest[i].MapSnapShot;
                        AppManager.RadarPoiPopup.GoToPointOfInterestMenu.onClick.RemoveAllListeners();

                        AppManager.RadarPoiPopup.gameObject.SetActive(true);
                        GameObject pointCanvas = PointsOfInterest[i].PointCanvas;
                        AppManager.RadarPoiPopup.GoToPointOfInterestMenu.onClick.AddListener(() =>
                        {
                            pointCanvas.SetActive(true);
                            AppManager.RadarPoiPopup.gameObject.SetActive(false);
                        });

                        StartCoroutine(PresentPopup(2f, i));
                        PointsOfInterest[i].TimeOpened = Time.time;
                        PointsOfInterest[i].HaveOpenedOnce = true;
                    }
                    else
                    {
                        float currentTime = Time.time;
                        if ((currentTime - PointsOfInterest[i].TimeOpened) >180)
                        {
                            AppManager.CloseAllCanvases();
                            AppManager.RadarPoiPopup.PoiText.text = PointsOfInterest[i].Name;

                            if (PointsOfInterest[i].Name == "Αρχαιολογικό Μουσείο")
                            {
                                AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 300).ToString() + " μ.";
                            }
                            else if (PointsOfInterest[i].Name == "Μουσείο Βυζαντινού Πολιτισμού")
                            {
                                AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 300).ToString() + " μ.";
                            }
                            else if (PointsOfInterest[i].Name == "Βασιλικό Θέατρο")
                            {
                                AppManager.RadarPoiPopup.DistanceToPoint.text = (PointsOfInterest[i].DistanceFromTrigger + 120).ToString() + " μ.";
                            }
                            else
                            {
                                AppManager.RadarPoiPopup.DistanceToPoint.text = PointsOfInterest[i].DistanceFromTrigger.ToString() + " μ.";
                            }

                            AppManager.RadarPoiPopup.WalkingTime.text = PointsOfInterest[i].WalkTimeToPoint.ToString() + " λ.";
                            AppManager.RadarPoiPopup.Map.sprite = PointsOfInterest[i].MapSnapShot;
                            AppManager.RadarPoiPopup.GoToPointOfInterestMenu.onClick.RemoveAllListeners();

                            AppManager.RadarPoiPopup.gameObject.SetActive(true);
                            GameObject pointCanvas = PointsOfInterest[i].PointCanvas;
                            AppManager.RadarPoiPopup.GoToPointOfInterestMenu.onClick.AddListener(() =>
                            {
                                pointCanvas.SetActive(true);
                                AppManager.RadarPoiPopup.gameObject.SetActive(false);
                            });

                            StartCoroutine(PresentPopup(2f, i));
                        }
                    }
                }
            }
        }
    }

    private void PoiDistanceCheckForSorting()
    {
        if (gpsFix ==true)
        {
            SortedPoints.Clear();
            
            for (int i = 0; i < PointsOfInterest.Length; i++)
            {
                if (PointsOfInterest[i].DistanceFromTrigger <= 150)
                {
                    SortedPoints.Add(PointsOfInterest[i]);
                }
            }
            SortedPoints = SortedPoints.OrderBy(o => o.DistanceFromTrigger).ToList();
            PointsSorted = true;
        }
    }

    IEnumerator PresentPopup(float delay, int pointIndex)
    {
        yield return new WaitForSeconds(delay);
        UAP_AccessibilityManager.StopSpeaking();

        if (PointsOfInterest[pointIndex].Name == "Αρχαιολογικό Μουσείο")
        {
            UAP_AccessibilityManager.Say("Βρίσκεστε σε απόσταση " + (PointsOfInterest[pointIndex].DistanceFromTrigger + 300).ToString() + " μέτρα από το " + PointsOfInterest[pointIndex].Name +
                       ". Για να μεταβείτε στο μενού του σημείου ενδιαφέροντος, κάντε διπλό ταπ, αλλιώς κάντε σλάηντ αριστερά για επιστροφή");
        }
        else if (PointsOfInterest[pointIndex].Name == "Μουσείο Βυζαντινού Πολιτισμού")
        {
            UAP_AccessibilityManager.Say("Βρίσκεστε σε απόσταση " + (PointsOfInterest[pointIndex].DistanceFromTrigger + 300).ToString() + " μέτρα από το " + PointsOfInterest[pointIndex].Name +
                       ". Για να μεταβείτε στο μενού του σημείου ενδιαφέροντος, κάντε διπλό ταπ, αλλιώς κάντε σλάηντ αριστερά για επιστροφή");
        }
        else if (PointsOfInterest[pointIndex].Name == "Βασιλικό Θέατρο")
        {
            UAP_AccessibilityManager.Say("Βρίσκεστε σε απόσταση " + (PointsOfInterest[pointIndex].DistanceFromTrigger + 120).ToString() + " μέτρα από το " + PointsOfInterest[pointIndex].Name +
                        ". Για να μεταβείτε στο μενού του σημείου ενδιαφέροντος, κάντε διπλό ταπ, αλλιώς κάντε σλάηντ αριστερά για επιστροφή");
        }
        else
        {
            UAP_AccessibilityManager.Say("Βρίσκεστε σε απόσταση " + PointsOfInterest[pointIndex].DistanceFromTrigger.ToString() + " μέτρα από το " + PointsOfInterest[pointIndex].Name +
                        ". Για να μεταβείτε στο μενού του σημείου ενδιαφέροντος, κάντε διπλό ταπ, αλλιώς κάντε σλάηντ αριστερά για επιστροφή");
        }        
    }

    public void StartGpsSystem()
    {
        StartCoroutine(GpsSystem());
    }

    private IEnumerator GpsSystem()
    {
        PointsSorted = false;
        Debug.Log("STARTING GPS");
        //STARTING LOCATION SERVICES
        // First, check if user has location service enabled
#if (UNITY_IOS && !UNITY_EDITOR)
		//if (!Input.location.isEnabledByUser){
 
		//	//This message prints to the Editor Console
		//	print("Please enable location services and restart the App");
		//	//You can use this "status" variable to show messages in your custom user interface (GUIText, etc.)
		//	status = "Please enable location services\n and restart the App";
		//	yield return new WaitForSeconds(4);
		//	Application.Quit();
		//}
#endif
        // Start service before querying location
        Input.location.Start(3.0f, 3.0f);
        Input.compass.enabled = true;
        print("Initializing Location Services.." + Input.location.status.ToString());

        // Wait until service initializes
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("Initial Wait");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 30 seconds
        if (maxWait < 1)
        {
            Debug.Log("Still waiting after 30s");
            print("Unable to initialize location services.\nPlease check your location settings and restart the App");
            //yield return new WaitForSeconds(4);
            UAP_AccessibilityManager.Say("Δεν είναι δυνατή η εκκίνηση των υπηρεσιών τοποθεσίας. Βεβαιωθείτε ότι έχετε ενεργοποιήσει τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed || Input.location.status == LocationServiceStatus.Stopped)
        {
            print("Unable to determine your location.\nPlease check your location setting and restart this App");
            //yield return new WaitForSeconds(4);
            UAP_AccessibilityManager.Say("Δεν είναι δυνατό να προσδιοριστεί η τοποθεσία σας. Βεβαιωθείτε ότι έχετε ενεργοποιήσει τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
        }

        // Access granted and location value could be retrieved
        else if (Input.location.status == LocationServiceStatus.Running)
        {
            //Wait in order to find enough satellites and increase GPS accuracy
            yield return new WaitForSeconds(initTime);
            //Successful GPS fix
            gpsFix = true;
            FsmVariables.GlobalVariables.FindFsmBool("GpsFix").Value = true;
        }
    }

    public void StartCalculateDistance()
    {
        StartCoroutine(CalculateDistance());
    }

    private IEnumerator CalculateDistance()
    {
       // Debug.Log("Waiting for GPS FIX");
        //Wait until the gps sensor provides a valid location.
        while (!gpsFix)
        {
            yield return null;
        }
        //Debug.Log("GPS FIXED");
        InvokeRepeating("MyDistance", 1, updateRate);

    }

    public void StopGpsSystem()
    {
        StopCoroutine(GpsSystem());
        StopCoroutine("CalculateDistance");
        CancelInvoke();
        gpsFix = false;
        FsmVariables.GlobalVariables.FindFsmBool("GpsFix").Value = false;
    }

    public void SetPopupCalculation(bool status)
    {
        m_DoPopupCalculations = status;
    }

    void MyDistance()
    {
        if (gpsFix)
        {
            for (int i = 0; i < PointsOfInterest.Length; i++)
            {
                //Distance between each Point of Interest and User Position
                double distanceMeters = GeoDistance(new PinPoint(PointsOfInterest[i].TriggerLatitude, PointsOfInterest[i].TriggerLongitude), new PinPoint(Input.location.lastData.latitude, Input.location.lastData.longitude));
                PointsOfInterest[i].DistanceFromTrigger = (int)distanceMeters;
            }
            if (m_DoPopupCalculations)
            {
                PoiDistanceCheckForPopup();
            }
            PoiDistanceCheckForSorting();
        }
    }

    private double GeoDistance(PinPoint loc1, PinPoint loc2)
    {
        double radius = 6371;
        double dLat = this.toRadian(loc2.Latitude - loc1.Latitude);
        double dLon = this.toRadian(loc2.Longitude - loc1.Longitude);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(this.toRadian(loc1.Latitude)) * Math.Cos(this.toRadian(loc2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        double d = radius * c;
        return Math.Round(d * 1000, 3); 
    }

    // Convert to Radians.
    private double toRadian(double val)
    {
        return (Math.PI / 180) * val;
    }
}
