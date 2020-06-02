//MAPNAV Navigation ToolKit v.1.5.0
//This script is for illustrative purposes only. Feel free to modify, extend or customize it to fit your own needs.
//This scripts requires the "InfoCanvas" prefab to be included in your scene in order to display data.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
[AddComponentMenu("MAPNAV/GPS_Status")]

public class GPS_Status : MonoBehaviour
{
    public float refreshRate = 0.5f;
    Transform infoCanvas;
    private Text UI_latitude;
    private Text UI_longitude;
    private Text UI_altitude;
    private Text UI_status;
    private Text UI_heading;
    private Text UI_zoom;
    private Text UI_accuracy;
    private Text UI_speed;
    private MapNav gps;


    [HideInInspector]
    public float instantVel;
    [HideInInspector]
    public bool speedometer;

    void Awake()
    {
        //infoCanvas = GameObject.FindGameObjectWithTag("LocationInfo").transform;
        //UI_latitude = infoCanvas.Find("Latitude").GetComponent<Text>();
        //UI_longitude = infoCanvas.Find("Longitude").GetComponent<Text>();
        //UI_altitude = infoCanvas.Find("Altitude").GetComponent<Text>();
        //UI_status = infoCanvas.Find("Status").GetComponent<Text>();
        //UI_heading = infoCanvas.Find("Heading").GetComponent<Text>();
        //UI_zoom = infoCanvas.Find("Zoom").GetComponent<Text>();
        //UI_accuracy = infoCanvas.Find("Accuracy").GetComponent<Text>();
        //UI_speed = infoCanvas.Find("Speed").GetComponent<Text>();

        //Reference to MapNav.cs script. Make sure that the map object containing the MapNav.cs script is tagged as "GameController"
        gps = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
    }

    void Start()
    {
        //Get gps Status Data every "refreshRate" seconds
        InvokeRepeating("GetData", 1.0f, refreshRate);
    }

    void GetData()
    {
        //if (gps.info)
        //{
        //    //Current latitude (degrees, minutes, seconds)
        //    UI_latitude.text = "Latitude: " + gps.dmsLat;

        //    //Current longitude (degrees, minutes, seconds)
        //    UI_longitude.text = "Longitude: " + gps.dmsLon;

        //    //Current altitude(meters)
        //    UI_altitude.text = "Altitude: " + gps.altitude + " m";

        //    //Current status
        //    UI_status.text = "Status: " + gps.status;

        //    //Current heading/orientation
        //    UI_heading.text = "Heading: " + gps.heading + "\u00B0 ";

        //    //Current Zoom Level
        //    UI_zoom.text = "Zoom level: " + gps.zoom;

        //    //Current GPS sensor accuracy
        //    if(!gps.simGPS)
        //        UI_accuracy.text = "Accuracy: " + gps.accuracy + " m";
        //    else
        //        UI_accuracy.text = "Accuracy: 30 m";

        //    //Current instant velocity (speed)
        //    UI_speed.text = "Speed: " + instantVel + " Km/h";
        //}
    }
}