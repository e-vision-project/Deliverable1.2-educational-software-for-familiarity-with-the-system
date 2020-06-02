using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MapQuestAPI : MonoBehaviour
{
    private static string m_MapQuestKey = "XGivCKgE8sNHJcEASW2hNjGAFStk6WrA";     //AppKey (API key) code obtained from your maps provider (MapQuest, Google, etc.). 
    private static string m_GoogleMapsKey = "AIzaSyDj_98aZwLZ_bSegHeSu02ibQnL1_mE5r8";
    private WWW m_Www;
    private string m_MapQuestUrl = "";
    private string m_GoogleMapsUrl = "";
    private string m_Status;
    private float m_DownloadProgress;
    public TextMeshProUGUI Status;
    public TextMeshProUGUI Location;
    public float TestLatitude;
    public float TestLongtitude;
    MapQuestLocation m_MapQuestLocationInfo;
    GoogleMapsLocation m_GoogleMapsLocationInfo;

    // Start is called before the first frame update
    void Start()
    {
        m_MapQuestUrl = "http://www.mapquestapi.com/geocoding/v1/reverse?key=" + m_MapQuestKey + "&location="+ TestLatitude + ","+ TestLongtitude + "&includeRoadMetadata=true&includeNearestIntersection=false";
        m_MapQuestLocationInfo = new MapQuestLocation();

        m_GoogleMapsUrl = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + TestLatitude + "," + TestLongtitude + "&key=" + m_GoogleMapsKey+"&language=el";
        m_GoogleMapsLocationInfo = new GoogleMapsLocation();
    }

    public void CheckLocation()
    {
        StartCoroutine(Online());
    }

    public void PrintDeserial()
    {
        // Debug.Log(m_MapQuestLocationInfo.results[0].locations[0].street.ToString());
        Debug.Log(m_GoogleMapsLocationInfo.results[0].formatted_address);
    }

    IEnumerator Online()
    {
        // Start a download of the given URL
        m_Www = new WWW(m_GoogleMapsUrl);
        // Wait for download to complete
        m_DownloadProgress = (m_Www.progress);
        while (!m_Www.isDone)
        {
            print("Downloading request " + Mathf.Round(m_DownloadProgress * 100) + " %");
            //use the status string variable to print messages to your own user interface (GUIText, etc.)
            m_Status = "Downloading request " + Mathf.Round(m_DownloadProgress * 100) + " %";
            yield return null;
        }
        //Show download progress and apply texture
        if (m_Www.error == null)
        {
            print("Request Ready 100 %");
            //use the status string variable to print messages to your own user interface (GUIText, etc.)
            m_Status = "Request  100 % Ready!";
            yield return new WaitForSeconds(0.5f);

            //m_MapQuestLocationInfo = JsonUtility.FromJson<MapQuestLocation>(m_Www.text);

            m_GoogleMapsLocationInfo = JsonUtility.FromJson<GoogleMapsLocation>(m_Www.text);

            Location.text = "Τοποθεσία: "+ m_GoogleMapsLocationInfo.results[0].formatted_address;
        }
        //Download Error. Switching to offline mode
        else
        {
            print("Map Error:" + m_Www.error);
            //use the status string variable to print messages to your own user interface (GUIText, etc.)
            m_Status = "Map Error:" + m_Www.error;
            yield return new WaitForSeconds(1);

        }


    }

    private void Update()
    {
        Status.text = m_Status;
    }

}
