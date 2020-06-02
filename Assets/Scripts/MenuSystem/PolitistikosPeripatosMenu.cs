using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PolitistikosPeripatosMenu : MonoBehaviour {
    public Toggle PoiRadar = null;
    //public PointsOfInterest[] PointsOfInterest;
    private int maxWait =30;
    private float initTime =3;
    private bool gpsFix;
    private float updateRate= 0.8f;

    public void OnEnable()
    {
        PoiRadar.isOn = AppManager.PlayerPrefsPoiRadar;
        if (PoiRadar.isOn == true)
        {
            AppManager.GpsManager.SetPopupCalculation(true);
            AppManager.GpsManager.StartGpsSystem();
            AppManager.GpsManager.StartCalculateDistance();
        }
    }

    public void OnDisable()
    {
        AppManager.GpsManager.StopGpsSystem();
        AppManager.GpsManager.SetPopupCalculation(false);
    }

    public void SetPoiRadar()
    {
        AppManager.GpsManager.StopGpsSystem();
        
        AppManager.SetPoiRadar(PoiRadar.isOn);
        if (PoiRadar.isOn == true)
        {
            AppManager.GpsManager.StartGpsSystem();
            AppManager.GpsManager.StartCalculateDistance();
        }
    }

//    IEnumerator GpsSystem()
//    {
//        Debug.Log("STARTING GPS");
//        //STARTING LOCATION SERVICES
//        // First, check if user has location service enabled
//#if (UNITY_IOS && !UNITY_EDITOR)
//		if (!Input.location.isEnabledByUser){
 
//			//This message prints to the Editor Console
//			print("Please enable location services and restart the App");
//			//You can use this "status" variable to show messages in your custom user interface (GUIText, etc.)
//			status = "Please enable location services\n and restart the App";
//			yield return new WaitForSeconds(4);
//			Application.Quit();
//		}
//#endif
//        // Start service before querying location
//        Input.location.Start(3.0f, 3.0f);
//        Input.compass.enabled = true;
//        print("Initializing Location Services.." + Input.location.status.ToString());

//        // Wait until service initializes
//        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
//        {
//            Debug.Log("Initial Wait");
//            yield return new WaitForSeconds(1);
//            maxWait--;
//        }

//        // Service didn't initialize in 30 seconds
//        if (maxWait < 1)
//        {
//            Debug.Log("Still waiting after 30s");
//            print("Unable to initialize location services.\nPlease check your location settings and restart the App");
//            yield return new WaitForSeconds(4);
//            UAP_AccessibilityManager.Say("Δεν είναι δυνατή η εκκίνηση των υπηρεσιών τοποθεσίας. Βεβαιωθείτε ότι έχετε ενεργοποιήσει τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
//        }

//        // Connection has failed
//        if (Input.location.status == LocationServiceStatus.Failed || Input.location.status == LocationServiceStatus.Stopped)
//        {
//            print("Unable to determine your location.\nPlease check your location setting and restart this App");
//            yield return new WaitForSeconds(4);
//            UAP_AccessibilityManager.Say("Δεν είναι δυνατό να προσδιοριστεί η τοποθεσία σας. Βεβαιωθείτε ότι έχετε ενεργοποιήσει τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
//        }

//        // Access granted and location value could be retrieved
//        else
//        {

//            //Wait in order to find enough satellites and increase GPS accuracy
//            yield return new WaitForSeconds(initTime);
//            //Successful GPS fix
//            gpsFix = true;
//        }
//    }

//    IEnumerator CalculateDistance()
//    {
//        Debug.Log("Waiting for GPS FIX");
//        //Wait until the gps sensor provides a valid location.
//        while (!gpsFix)
//        {
//            yield return null;
//        }
//        Debug.Log("GPS FIXED");
//        InvokeRepeating("MyDistance", 1, updateRate);
//    }

//    void MyDistance()
//    {
//        Debug.Log("MEASURING DISTANCE$$$");
//        if (gpsFix)
//        {
//            for (int i = 0; i < PointsOfInterest.Length; i++)
//            {
//                Debug.Log(Input.location.lastData.latitude + " " +  Input.location.lastData.longitude);
//                //Distance between each Point of Interest and User Position
//                double leg = GeoDistance(new locWGS84(PointsOfInterest[i].TriggerLatitude, PointsOfInterest[i].TriggerLongitude), new locWGS84(Input.location.lastData.latitude, Input.location.lastData.longitude));
//                PointsOfInterest[i].DistanceFromTrigger = Math.Round(leg*1000,3);
//            }
//        }
//    }

//    public double GeoDistance(locWGS84 loc1, locWGS84 loc2)
//    {
//        double radius = 6371;
//        double dLat = this.toRadian(loc2.Latitude - loc1.Latitude);
//        double dLon = this.toRadian(loc2.Longitude - loc1.Longitude);
//        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//            Math.Cos(this.toRadian(loc1.Latitude)) * Math.Cos(this.toRadian(loc2.Latitude)) *
//                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
//        double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
//        double d = radius * c;
//        return d;
//    }

//    // Convert to Radians.
//    private double toRadian(double val)
//    {
//        return (Math.PI / 180) * val;
//    }

}
