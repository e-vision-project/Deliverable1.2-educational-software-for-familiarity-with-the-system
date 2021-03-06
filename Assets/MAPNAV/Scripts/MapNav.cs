﻿//MAPNAV Navigation ToolKit v.1.5.0
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/MapNavInspector.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System;
using System.Text.RegularExpressions;

[AddComponentMenu("MAPNAV/MapNav")]

//MapQuest Markers
[System.Serializable]
public struct Marker
{
    public string m_name, m_latitude, m_longitude;
    public MarkerType m_type;
    public MarkerSize m_size;
    public Color m_InnerColor, m_OuterColor;
    public string m_label;
    
}

public enum MarkerType
{
    marker,
    circle,
    via,
    flag
}

public enum MarkerSize
{
    sm,
    md,
    lg
}

//Main MapNav Class
public class MapNav : MonoBehaviour 
{
    public bool MapQuest;
    public bool GoogleMaps;
    public Transform user;									 	//User(Player) transform
	public bool simGPS = true;								 	//True when the GPS Emulator is enabled
	public float userSpeed = 23.0f;							 	//User speed when using the GPS Emulator (keyboard input). Measured in m/s when realistic speed is enabled.
	public bool realSpeed = true;								//If false, the perceived player speed on the screen will seem to be constant regardless of zoom level  (unrealistic behaviour)
	public float fixLat = 42.363f;					   			//Latitude
	public float fixLon = -71.05686f;							//Longitude
	public float altitude;										//Current GPS altitude
	public float heading;										//Last compass sensor reading (Emulator disabled) or user's eulerAngles.y (Emulator enabled)
	public float accuracy;										//GPS location accuracy (error)
	public int maxZoom = 18;									//Maximum zoom level available. Set according to your maps provider
	public int minZoom = 1;										//Minimum zoom level available
	public int zoom = 15;										//Current zoom level
    public int mapScale = 100;                                  //Scale of the map. Default is 1:100
	private float multiplier; 									//1 for a size=640x640 tile, 2 for size=1280*1280 tile, etc. Automatically set when selecting tile size
	public string MapQuestKey = "XGivCKgE8sNHJcEASW2hNjGAFStk6WrA";     //AppKey (API key) code obtained from your maps provider (MapQuest, Google, etc.). 
                                                                        //Default MapQuest key for demo purposes only (with limitations). Please get your own key before you start yout project.															 
    public string GoogleMapKey = "AIzaSyDj_98aZwLZ_bSegHeSu02ibQnL1_mE5r8";

    public string[] maptype;									//Array including available map types
	public int[] mapSize;										//Array including available map sizes(pixels)
    public string[] mapFormat;                                  //The image format of the map. 
    public int indexType;									    //mapType array index.
    public int indexFormat;                                     //mapFormat array index
	public int indexSize;										//mapSize array index. 
	public float camDist = 15.0f;								//Camera distance(3D) or height(2D) to user
	public int camAngle = 40;									//Camera angle from horizontal plane
	public int initTime = 3;									//Hold time after a successful GPS fix in order to improve location accuracy
	public int maxWait = 30;									//GPS fix timeout
	public bool buttons = true;								 	//Enables GUI sample control buttons 
	public string dmsLat;									 	//Latitude as degrees, minutes and seconds
	public string dmsLon;							 		 	//Longitude as degrees, minutes and seconds
	public float updateRate = 0.1f;								//User's position update rate
	public bool autoCenter = true;							 	//Autocenter and refresh map
	public string status;								     	//GPS and other status messages
	public bool gpsFix;								     		//True after a successful GPS fix 
	public Vector3 iniRef;							         	//First location data retrieved on Start (as Unity cartesian coordinates)	 
	public bool info;									     	//Used by GPS_Status.cs to enable/disable the GPS information window.
	public bool triDView = false;						     	//2D/3D modes toggle
	public bool ready;								     		//true when the map texture has been successfully loaded
	public bool freeCam = false;							 	//when false, MainCamera follows and looks at Player (3D mode only)
	public bool pinchToZoom = true;							 	//Enables Pinch to Zoom interaction on mobile devices
	public bool dragToPan = true;							 	//Enables Drag to Pan interaction on mobile devices
	public bool mapDisabled;								 	//Disables online maps
	public bool mapping = false;							 	//true while map is being downloaded
	public Transform cam;									 	//Reference to the Main Camera transform
	public float userLon;									 	//Current user position longitude
	public float userLat;                                       //Current user position latitude
    public string routeStart;                                   //Route Start. Takes a single line address or latitude,longitude pair
    public string routeEnd;                                     //Route End. Takes a single line address or latitude,longitude pair
    public bool routeEnabled;                                   //Enable or disable route drawing            
    public bool fromMyLocation;                                 //Use current GPS location as route start    
    public Color routeColor = new Color32(0x9f,0xdd,0x36,0xFF); //Route color
    public int routeWidth = 3;                                  //Route width in pixels
    public Marker[] markers;                                    //List of MapQuest markers (these are rendered by MapQuest Open Static Maps service)
    public Text LocationText;
    [Serializable]
    public class OnTextureLoadEvent : UnityEvent { }

    public OnTextureLoadEvent OnTextureLoad = new OnTextureLoadEvent();

    private float levelHeight;
	private float smooth = 1.3f;	 						    
	private float yVelocity = 0.0f;  
	private float speed;
	private Camera mycam;
	private float currentOrtoSize;
	private LocationInfo loc;
	private Vector3 currentPosition;
	private Vector3 newUserPos; 
	private Vector3 currentUserPos;
	private float download;
    private string m_GoogleMapsUrl = "";
    private string m_Status;
    private float m_DownloadProgress;
    GoogleMapsLocation m_GoogleMapsLocationInfo;
    private WWW m_Www;
    private WWW www;
	private string url = ""; 
	private double longitude;
	private double latitude;
	private Rect rect;
	private float screenX;
	private float screenY;
	private Renderer maprender;
	private Transform mymap;
	private float tempLat;
	private float tempLon;
	private bool touchZoom;
	private string centre;
	private bool centering;
	private Texture centerIcon;
	private Texture topIcon;
	private Texture bottomIcon;
	private Texture leftIcon;
	private Texture rightIcon;
	private GUIStyle arrowIcon;
	private float dot;
	private bool centered = true;
	private int borderTile = 0;
	private bool tileLeft;
	private bool tileRight;
	private bool tileTop;
	private bool tileBottom;
	private Rect topCursorPos;
	private Rect rightCursorPos;
	private Rect bottomCursorPos;
	private Rect leftCursorPos;
    private GameObject infoCanvas;

	//Touch Screen Variables
	private Vector2 prevDist;
	private float actualDist;
	private Transform target;
	private Touch touch;
	private Touch touch2;
	private Vector2 curDist;
	private float dragSpeed;
	private Rect viewArea;
	private float targetOrtoSize;
	private bool firstTime = true;
	private Vector2 focusScreenPoint;
	private Vector3 focusWorldPoint;

	void Awake(){
		//Set the map's tag to GameController
		transform.tag = "GameController";
		
		//References to the Main Camera and Player. 
		//Please make sure your camera is tagged as "MainCamera" and your user visualization/character as "Player"
		cam = Camera.main.transform;
		mycam = Camera.main;
		user = GameObject.FindGameObjectWithTag("Player").transform;

        //Reference to the UI information canvas
        //infoCanvas = GameObject.FindGameObjectWithTag("LocationInfo");
		
		//Store most used components and values into variables for faster access.
		mymap = transform;
		maprender = GetComponent<Renderer>();
		screenX = Screen.width;
		screenY = Screen.height;

        //Add possible values to maptype, mapSize and mapFormat arrays 
        //ATTENTION: Modify if using a maps provider other than MapQuest Static Maps.
        
            //maptype = new string[] { "map", "sat", "hyb", "light", "dark" };
            //mapSize = new int[] { 640, 960, 1280, 1920 }; //in pixels
            //mapFormat = new string[] { "jpg70", "jpg80", "jpg90", "png" }; //Available formats are png, jpg70 (70 % quality), jpg80(80 % quality) and jpg90(90 % quality).
        


        // GOOGLE MAPS IMPLEMENTATION
        //Add possible values to maptype, mapSize and mapFormat arrays (GOOGLE)
      
            maptype = new string[] { "satellite", "roadmap", "hybrid", "terrain" };
            mapSize = new int[] { 1280 }; //in pixels
            mapFormat = new string[] { "png", "gif", "jpg" };
        
        

        //Set GUI "center" button label
        if (triDView){
			centre = "refresh";
		}
		//Enable autocenter on 2D-view (default)
		else{
			autoCenter = true;
		}
		
		//Load required interface textures
		centerIcon = Resources.Load("centerIcon") as Texture2D;
		topIcon = Resources.Load("cursorTop") as Texture2D;
		bottomIcon = Resources.Load("cursorBottom") as Texture2D;
		leftIcon = Resources.Load("cursorLeft") as Texture2D;
		rightIcon = Resources.Load("cursorRight") as Texture2D;
		
		//Resize GUI according to screen size/orientation 
		if(screenY >= screenX){
			dot = screenY/800.0f;
		}else{
			dot = screenX/800.0f;
		}
	}

    public void BeginMapNav()
    {
        StartCoroutine(StartMapNavProccess());
    }

	IEnumerator StartMapNavProccess () {
        LocationText.text = "Τοποθεσία:";
		//Initializing variables
		gpsFix=false;
		rect = new Rect (screenX/10, screenY/10, 8*screenX/10, 8*screenY/10);
		topCursorPos = new Rect(screenX/2-25*dot, 0, 50*dot, 50*dot);
		rightCursorPos = new Rect(screenX-50*dot, screenY/2-25*dot, 50*dot, 50*dot);
		if(!buttons)
			bottomCursorPos = new Rect(screenX/2-25*dot, screenY-50*dot, 50*dot, 50*dot);
		else
			bottomCursorPos = new Rect(screenX/2-25*dot, screenY-50*dot-screenY/12, 50*dot, 50*dot);
		leftCursorPos = new Rect(0, screenY/2-25*dot, 50*dot, 50*dot);
		Vector3 tmp = mymap.eulerAngles;
		tmp.y=180;
		mymap.eulerAngles = tmp;
		user.position = new Vector3(0, user.position.y / mapScale, 0); //Centering user in the scene 
        user.localScale = new Vector3(user.localScale.x / mapScale, user.localScale.y / mapScale, user.localScale.z / mapScale); //rescale user according to map scale

        //Initial Camera Settings
        //3D 
        if (triDView){
			mycam.orthographic = false;
			pinchToZoom = false;
			dragToPan = false;

			//Set the camera's field of view according to Screen size so map's visible area is maximized.
			if(screenY > screenX){
				mycam.fieldOfView = 72.5f;
			}else{
				mycam.fieldOfView = 95 - (28*(screenX/screenY));
			}
		}
		//2D
		else{
			mycam.orthographic = true;

            Vector3 temp = cam.position;   
            temp.y = 1000 / mapScale;
            cam.position = temp;

            mycam.nearClipPlane = 0.1f;
            mycam.farClipPlane = cam.position.y+1;
            	
			if(screenY >= screenX){
				mycam.orthographicSize = mymap.localScale.z*5.0f;
			}else{
				mycam.orthographicSize = (screenY/screenX)*mymap.localScale.x*5.0f;		
			}
		}
		
		//The "ready" variable will be true when the map texture has been successfully loaded.
		ready = false; 
		
		//STARTING LOCATION SERVICES
		// First, check if user has location service enabled
		#if (UNITY_IOS && !UNITY_EDITOR)
		if (!Input.location.isEnabledByUser){
 
			//This message prints to the Editor Console
			print("Please enable location services and restart the App");
			//You can use this "status" variable to show messages in your custom user interface (GUIText, etc.)
			status = "Please enable location services\n and restart the App";
			yield return new WaitForSeconds(4);
			Application.Quit();
		}
		#endif
		// Start service before querying location
		Input.location.Start (3.0f, 3.0f); 
		Input.compass.enabled = true;
		print("Initializing Location Services..");
		status = "Initializing Location Services..";

		// Wait until service initializes
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds (1);
			maxWait--;
		}

		// Service didn't initialize in 30 seconds
		if (maxWait < 1) {
			print("Unable to initialize location services.\nPlease check your location settings and restart the App");
			status = "Unable to initialize location services.\nPlease check your location settings\n and restart the App";
			yield return new WaitForSeconds(4);
            UAP_AccessibilityManager.Say("Δεν είναι δυνατή η εκκίνηση των υπηρεσιών τοποθεσίας.Ελέγξτε τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed) {
			print("Unable to determine your location.\nPlease check your location setting and restart this App");
			status = "Unable to determine your location.\nPlease check your location settings\n and restart this App";
			yield return new WaitForSeconds(4);
            UAP_AccessibilityManager.Say("Δεν είναι δυνατό να προσδιοριστεί η τοποθεσία σας. Ελέγξτε τις ρυθμίσεις τοποθεσίας και κάντε επανεκκίνηση την εφαρμογή");
        }

        // Access granted and location value could be retrieved
        else {
			if(!mapDisabled){
				print("GPS Fix established. Setting position..");
				status = "GPS Fix established!\nSetting position ...";
			}
			else{
				print("GPS Fix established.");
				status = "GPS Fix established!";
			}
					
			if(!simGPS){
				//Wait in order to find enough satellites and increase GPS accuracy
				yield return new WaitForSeconds(initTime);
				//Set position
				loc  = Input.location.lastData;          
                iniRef = WGS84toWebMercator(loc.longitude, loc.latitude, 0f);
                fixLon = loc.longitude;
				fixLat = loc.latitude; 	
			}  
			else{
				//Simulate initialization time
				yield return new WaitForSeconds(initTime);
                //Set Position
                iniRef = WGS84toWebMercator(fixLon, fixLat, 0f);
			}

            //Successful GPS fix
            gpsFix = true;
            //Update Map for the current location
            StartCoroutine(MapPosition());
        }

		//Rescale map, set new camera height, and resize user pointer according to new zoom level
		 StartCoroutine(ReScale()); 

		//Set player's position using new location data (every "updateRate" seconds)
		//Default value for updateRate is 0.1. Increase if necessary to improve performance
		InvokeRepeating("MyPosition", 1, updateRate); 

		//Read incoming compass data (every 0.1s)
		InvokeRepeating("Orientate", 1, 0.1f);
		
		//Get altitude and horizontal accuracy readings using new location data (Default: every 2s)
		InvokeRepeating("AccuracyAltitude", 1, 2);
		
		//Auto-Center Map on 2D View Mode 
		InvokeRepeating("Check", 1, 0.2f);
	}


	void MyPosition(){
		if(gpsFix){
			if(!simGPS){
				loc = Input.location.lastData;
                newUserPos = WGS84toWebMercator(loc.longitude, loc.latitude, newUserPos.y) - iniRef;

				dmsLat = convertdmsLat(loc.latitude);
				dmsLon = convertdmsLon(loc.longitude);

				userLon = loc.longitude;
				userLat = loc.latitude;
			}
			else{
                Vector2 userCoordinates = WebMercatortoWGS84(user.position.x + iniRef.x, user.position.z + iniRef.z);
                userLon = userCoordinates.x;
                userLat = userCoordinates.y;
                
				dmsLat = convertdmsLat(userLat);
				dmsLon = convertdmsLon(userLon);
			}
		}	
	} 

	
	void Orientate(){
		if(!simGPS && gpsFix){
			heading = Input.compass.trueHeading;
		}
		else{
			heading = user.eulerAngles.y;
		}
	}
	 
	void AccuracyAltitude(){
		if(gpsFix)
			altitude = loc.altitude;
			accuracy = loc.horizontalAccuracy;
	}

    //Auto-Center Map on 2D View Mode when exiting viewport
    void Check(){
		if(autoCenter && triDView == false){
			if(ready == true && mapping == false && gpsFix){
				if (rect.Contains(Vector2.Scale(mycam.WorldToViewportPoint (user.position), new Vector2(screenX, screenY)))){
					//DoNothing
				}
				else{
				    centering=true;
					StartCoroutine(MapPosition());
					StartCoroutine(ReScale());	
				}
			}
		}
	}

	//Auto-Center Map on 3D View Mode when exiting map's collider
	void OnTriggerExit(Collider other){
		if(other.tag == "Player" && autoCenter && triDView){
			 StartCoroutine(MapPosition());
			 StartCoroutine(ReScale());
		}
	}

	//Update Map with the corresponding map images for the current location ============================================
	IEnumerator MapPosition(){

		//The mapping variable will only be true while the map is being updated
		mapping = true;
		
		CursorsOff();
		
		//CHECK GPS STATUS AND RESTART IF NEEDED
		
		if (Input.location.status == LocationServiceStatus.Stopped || Input.location.status == LocationServiceStatus.Failed){
			// Start service before querying location
			Input.location.Start (3.0f, 3.0f);

			// Wait until service initializes
			int maxWait = 20;
			while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
				yield return new WaitForSeconds (1);
				maxWait--;
			}

			// Service didn't initialize in 20 seconds
			if (maxWait < 1) {
				print ("Timed out");
				//use the status string variable to print messages to your own user interface (GUIText, etc.)
				status = "Timed out";
			}

			// Connection has failed
			if (Input.location.status == LocationServiceStatus.Failed) {
				print ("Unable to determine device location");
				//use the status string variable to print messages to your own user interface (GUIText, etc.)
				status = "Unable to determine device location";
			}
		
		}
		
	   //------------------------------------------------------------------	//
	   
		www = null; 
		//Get last available location data
		loc = Input.location.lastData;
		//Make player invisible while updating map
		user.gameObject.GetComponent<Renderer>().enabled = false;
		
		
		//Set target latitude and longitude
		if(triDView){
			if(simGPS){
                Vector2 coordinates = WebMercatortoWGS84(user.position.x + iniRef.x, user.position.z + iniRef.z);
                fixLon = coordinates.x;
                fixLat = coordinates.y;
                	
			}else{
				fixLon = loc.longitude;
				fixLat = loc.latitude;
			}
		}else{
			if(centering){
                if (simGPS){
                    Vector2 coordinates = WebMercatortoWGS84(user.position.x + iniRef.x, user.position.z + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;

                }
                else {
                    fixLon = loc.longitude;
                    fixLat = loc.latitude;
                }
            }
			else{
				if(borderTile == 0){
                    Vector2 coordinates = WebMercatortoWGS84(cam.position.x + iniRef.x, cam.position.z + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;
				}
				//North tile
				if (borderTile == 1){
                    Vector2 coordinates = WebMercatortoWGS84(cam.position.x + iniRef.x, cam.position.z + 3 * mycam.orthographicSize / 2 + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;
                    borderTile =0;	
					tileTop=false;
				}
				//East Tile
				if (borderTile == 2){
                    Vector2 coordinates = WebMercatortoWGS84(cam.position.x + 3 * (screenX * mycam.orthographicSize / screenY) / 2 + iniRef.x, cam.position.z + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;
                    borderTile = 0;
				}
				//South Tile
				if (borderTile == 3){
                    Vector2 coordinates = WebMercatortoWGS84(cam.position.x + iniRef.x, cam.position.z - 3 * mycam.orthographicSize / 2 + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;
					borderTile=0;
				}
				//West Tile
				if (borderTile == 4){
                    Vector2 coordinates = WebMercatortoWGS84(cam.position.x - 3 * (screenX * mycam.orthographicSize / screenY) / 2 + iniRef.x, cam.position.z + iniRef.z);
                    fixLon = coordinates.x;
                    fixLat = coordinates.y;
					borderTile=0;
				}
			}
		}

        #region MAPQUEST
  //      //MAPQUEST=========================================================================================

  //      //Build a valid MapQuest OpenMaps tile request for the current location
  //      multiplier = mapSize[indexSize]/640.0f;  //Tile Size= 640*multiplier

  //      //ATENTTION: If you want to implement maps from a different tiles provider, modify the following url accordingly to create a valid request
  //      //Example code can be found at http://recursivearts.com/mapnav/faq.html
		//url = "https://open.mapquestapi.com/staticmap/v5/map?key="+MapQuestKey+"&size="+mapSize[indexSize].ToString()+","+mapSize[indexSize].ToString()+"@2x&zoom="+ zoom + "&format=" + mapFormat[indexFormat] + "&type=" +maptype[indexType]+"&center="+fixLat+","+fixLon;

  //      //MapQuest Route Options
  //      if (routeEnabled) {
  //          MyPosition();
            
  //          if(fromMyLocation)
  //              url += ("&routeColor=" + ColorUtility.ToHtmlStringRGBA(routeColor) + "&start=" + userLat + "," + userLon +"|marker=none"+ "&end=" + Regex.Replace(routeEnd, @"\s", ""));
  //          else
  //              url += ("&routeColor=" + ColorUtility.ToHtmlStringRGBA(routeColor) + "&start=" + Regex.Replace(routeStart, @"\s", "") + "&end=" + Regex.Replace(routeEnd, @"\s", ""));

  //          url += "&routeWidth=" + routeWidth;
  //      }
  //      //MapQuest Markers
  //      if (markers != null) {

  //          for (int i = 0; i < markers.Length; i++) {

  //              if (i == 0)
  //                  url += ("&locations=" + markers[0].m_latitude + "," + markers[0].m_longitude + "|" + markers[0].m_type + "-" + markers[0].m_size + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_OuterColor) + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_InnerColor) + "-" + markers[0].m_label);
  //              else {
  //                  if(markers[i].m_type != MarkerType.via)
  //                      url += ("||" + markers[i].m_latitude + "," + markers[i].m_longitude + "|" + markers[i].m_type + "-" + markers[i].m_size + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_OuterColor) + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_InnerColor) + "-" + markers[i].m_label);
  //                  else
  //                      url += ("||" + markers[i].m_latitude + "," + markers[i].m_longitude + "|" + markers[i].m_type + "-" + markers[i].m_size + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_InnerColor) + "-" + ColorUtility.ToHtmlStringRGB(markers[i].m_OuterColor));
  //              }
  //          }
  //      }

  //      tempLat = fixLat; 
		//tempLon = fixLon;
        #endregion

        #region GOOGLEMAPS
        // GOOGLE ================================================================================
        //Build a valid Google Maps tile request for the current location 
        multiplier = 1;
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + fixLat + "," + fixLon + "&zoom=" + zoom + "&size="+ mapSize[indexSize].ToString()+"x"+ mapSize[indexSize].ToString() + "&maptype=" + maptype[indexType] + "&key=" + GoogleMapKey;
        //url = "http://maps.googleapis.com/maps/api/staticmap?center=" + fixLat + "," + fixLon + "&zoom=" + zoom + "&scale=2&size=640x640&format=" + mapFormat[indexFormat] + "&maptype=" + maptype[indexType] + "&sensor=false&key=" + GoogleMapKey;
        m_GoogleMapsUrl = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + fixLat + "," + fixLon + "&key=" + GoogleMapKey + "&language=el";
        m_GoogleMapsLocationInfo = new GoogleMapsLocation();
        tempLat = fixLat;
        tempLon = fixLon;
        #endregion
        print("in mapposition coroutine....");

        //=================================================================================================
        //Proceed with download if a Wireless internet connection is available 
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork){
			StartCoroutine(Online());
        }        
		//Proceed with download if a 3G/4G internet connection is available 
		else if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork){
			 StartCoroutine(Online());
		}
		//No internet connection is available. Switching to Offline mode.	 
		else{
			Offline();
		}	
	}

	//ONLINE MAP DOWNLOAD
	IEnumerator Online(){
        print("in online coroutine....");

        if (!mapDisabled){
			// Start a download of the given URL
			www = new WWW(url); 
			// Wait for download to complete
			download = (www.progress);
			while(!www.isDone){
				print("Updating map "+Math.Round(download*100)+" %");
				//use the status string variable to print messages to your own user interface (GUIText, etc.)
				status="Updating map "+Math.Round(download*100)+" %";
				yield return null;
			}
			//Show download progress and apply texture
			if(www.error == null){
				print("Updating map 100 %");
				print("Map Ready!");
				//use the status string variable to print messages to your own user interface (GUIText, etc.)
				status = "Updating map 100 %\nMap Ready!";
				yield return new WaitForSeconds (0.5f);
				maprender.material.mainTexture = null;
				Texture2D tmp;
				tmp = new Texture2D(1280, 1280, TextureFormat.RGB24, false);
				maprender.material.mainTexture = tmp;
				www.LoadImageIntoTexture(tmp);
                if (OnTextureLoad!=null)
                {
                    OnTextureLoad.Invoke();
                    UAP_AccessibilityManager.Say("Τοποθεσία βρέθηκε. Κάντε διπλό ταπ για να ενημερωθείτε");
                }
                StartCoroutine(GetPosition());
			}
			//Download Error. Switching to offline mode
			else{
				print("Map Error:"+www.error);
				//use the status string variable to print messages to your own user interface (GUIText, etc.)
				status = "Map Error:"+www.error;
				yield return new WaitForSeconds (1);
				maprender.material.mainTexture = null;
				Offline();
			}
			maprender.enabled = true;
		}
		ReSet();
		user.gameObject.GetComponent<Renderer>().enabled = true;
		ready = true;
		mapping = false;
		
	}
    
    public void SayLocation()
    {
        UAP_AccessibilityManager.Say("Βρίσκεστε στην Τοποθεσία : " + m_GoogleMapsLocationInfo.results[0].formatted_address.Split(',')[0]);
    }

    //GET USER POSITION
    IEnumerator GetPosition()
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

            LocationText.text =  m_GoogleMapsLocationInfo.results[0].formatted_address;
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

    //USING OFFLINE BACKGROUND TEXTURE
    void Offline()
    {
        Debug.Log("No internet!!");

        if (!mapDisabled)
        {
            maprender.material.mainTexture = Resources.Load("offline") as Texture2D;
            maprender.enabled = true;
        }
        ReSet();
        ready = true;
        mapping = false;
        user.gameObject.GetComponent<Renderer>().enabled = true;
    }

	//Re-position map and camera using updated data
	void ReSet(){
		transform.position = WGS84toWebMercator(tempLon, tempLat, transform.position.y) - iniRef;
		if(!freeCam){
			cam.position = new Vector3(transform.position.x, cam.position.y, transform.position.z);
		}
		if(triDView == false && centering){
			centered = true;
			autoCenter = true;
			centering = false;
		}
	}

	//RE-SCALE =========================================================================================================
	IEnumerator ReScale(){
		while(mapping){
			yield return null;
		}
       
		//Rescale map according to new zoom level to maintain scale factor
        double newScale = multiplier * (10053224.4/mapScale) / (Mathf.Pow(2, zoom));
        mymap.localScale = new Vector3((float)newScale, mymap.localScale.y, (float)newScale);

        //3D View. Free/custom camera
        if (triDView && freeCam){
			//Do Nothing
		}
		
		//3D View and Camera follows player. Set camera position
		else if(triDView && !freeCam){
			Vector3 tmp = cam.localPosition;
			tmp.z = -((13107200 /mapScale / user.localScale.x) * camDist*Mathf.Cos(camAngle*Mathf.PI/180))/Mathf.Pow(2, zoom);
			tmp.y = (13107200 / mapScale / user.localScale.x) * camDist*Mathf.Sin(camAngle*Mathf.PI/180)/Mathf.Pow(2, zoom);
			cam.localPosition = tmp;
		}
		
		//2D View. Set camera position 
		else{
			if(firstTime){
				cam.localEulerAngles = new Vector3(90, 0, 0);
				if(screenY >= screenX){
					mycam.orthographicSize = mymap.localScale.z*5.0f*0.75f;
				}else{
					mycam.orthographicSize = (screenY/screenX)*mymap.localScale.x*5.0f*0.75f;		
				}
			}
			firstTime = false;

			if(screenY >= screenX){
				targetOrtoSize = Mathf.Round(mymap.localScale.z*5.0f*100.0f)/100.0f;
			}else{
				targetOrtoSize = Mathf.Round((screenY/screenX)*mymap.localScale.x*5.0f*100.0f)/100.0f;		
			}
			
			while(Mathf.Abs(mycam.orthographicSize-targetOrtoSize*0.8f) > 0.01f){ 
			currentOrtoSize = mycam.orthographicSize;
			currentOrtoSize = Mathf.MoveTowards (currentOrtoSize, targetOrtoSize*0.8f, 2.5f*3276800*Time.deltaTime/Mathf.Pow(2, zoom)/mapScale);
			mycam.orthographicSize = currentOrtoSize;
			yield return null;
			}
			
			//Drag-to-pan speed according to zoom level
			dragSpeed = 0.8f/9.594413f*mycam.orthographicSize;
		}
	}

	void Update(){

		//Rename GUI "center" button label
		if(!triDView){
			if(cam.position.x != user.position.x || cam.position.z != user.position.z)
				centre ="center";
			else
				centre ="refresh";
		}
		
		//User pointer speed
		if(realSpeed){
			speed = userSpeed * ( 1 / Mathf.Cos(userLat*Mathf.PI/180));
		}
		else{
			speed = userSpeed*32700/(Mathf.Pow(2, zoom)*1.0f) * (1 / Mathf.Cos(userLat * Mathf.PI / 180));
		}
		
		//3D-2D View Camera Toggle (use only while game is stopped) 
		if(triDView && !freeCam){
			cam.parent = user;
			if(ready)
				cam.LookAt(user);
		}	
		
		if(ready){	
			if(!simGPS){
				//Smoothly move pointer to updated position
				currentUserPos.x = user.position.x;
				currentUserPos.x = Mathf.Lerp (user.position.x, newUserPos.x, 2.0f*Time.deltaTime);
				currentUserPos.z = user.position.z;
				currentUserPos.z = Mathf.Lerp (user.position.z, newUserPos.z, 2.0f*Time.deltaTime);
				user.position = new Vector3(currentUserPos.x, user.position.y, currentUserPos.z);

				//Update rotation
				if(Math.Abs(user.eulerAngles.y-heading) >= 5){
					float newAngle = Mathf.SmoothDampAngle(user.eulerAngles.y, heading, ref yVelocity, smooth);
					user.eulerAngles = new Vector3(user.eulerAngles.x, newAngle, user.eulerAngles.z);
				}
			}
			else{
				//When GPS Emulator is enabled, user position is controlled by keyboard input.
				if(mapping == false){
					//Use keyboard input to move the player
					if (Input.GetKey ("up") || Input.GetKey ("w")){
						user.transform.Translate(Vector3.forward*speed*Time.deltaTime/mapScale);
					}
					if (Input.GetKey ("down") || Input.GetKey ("s")){
						user.transform.Translate(-Vector3.forward*speed*Time.deltaTime/mapScale);
					}
					//rotate pointer when pressing Left and Right arrow keys
					user.Rotate(Vector3.up, Input.GetAxis("Horizontal")*80*Time.deltaTime);
				}
			}	
		}
		
		if(mapping && !mapDisabled){
			//get download progress while images are still downloading
			if(www != null)
                download = www.progress;
		}	
		
		//Enable/Disable map renderer 
		if(mapDisabled)
			maprender.enabled = false;
		else
			maprender.enabled = true;
		
		//PINCH TO ZOOM ================================================================================================
		if(pinchToZoom){
			if(Input.touchCount == 2 && mapping == false){
				touch = Input.GetTouch(0);
				touch2 = Input.GetTouch(1);
				
				if(touch.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began){
					focusScreenPoint = (touch.position+touch2.position)/2;
					focusWorldPoint = mycam.ScreenToWorldPoint(new Vector3(focusScreenPoint.x, focusScreenPoint.y, cam.position.y));
				}
				
				if(touch.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
					touchZoom = true;
					curDist = touch.position-touch2.position;
					prevDist = (touch.position-touch.deltaPosition)-(touch2.position-touch2.deltaPosition);
					actualDist = prevDist.magnitude-curDist.magnitude;
				}else{
					touchZoom = false;
				}
			}
		}
		if(touchZoom){								
																	
			//Modify camera orthographic size
			mycam.orthographicSize = mycam.orthographicSize+actualDist*Time.deltaTime*mycam.orthographicSize/30;
			mycam.orthographicSize = Mathf.Clamp(mycam.orthographicSize, 3*targetOrtoSize/8, targetOrtoSize);
			
			if(actualDist < 0){
                cam.position = ZoomMoveCamera(focusWorldPoint);
			}
			else if (actualDist == 0){
				//Do nothing
			}
			else{
                cam.position = ZoomMoveCamera(mymap.position);
			}

			//Get touch drag speed for new zoom level
			dragSpeed = 0.8f/9.594413f*mycam.orthographicSize;
			
			//Clamp the camera position to avoid displaying any off-map areas
			ClampCam();
			CursorsOff();
					
			//Decrease zoom level
			if(Mathf.Round(mycam.orthographicSize*1000.0f)/1000.0f >= Mathf.Round(targetOrtoSize*1000.0f)/1000.0f && zoom>minZoom){
				if(!mapping){
					touchZoom = false;
					zoom = zoom-1;
					 StartCoroutine(MapPosition());
					 StartCoroutine(ReScale());
				}
			}
			//Increase zoom level
			if(Mathf.Round(mycam.orthographicSize*1000.0f)/1000.0f <= Mathf.Round((targetOrtoSize/2)*1000.0f)/1000.0f && zoom<maxZoom){
				if(!mapping){
					touchZoom = false;
					zoom = zoom+1;
					 StartCoroutine(MapPosition());
					 StartCoroutine(ReScale());
				}
			}
		}
		
		//DRAG TO PAN ==================================================================================================
		if(dragToPan){
			if(!mapping && ready){

				#if (UNITY_EDITOR || UNITY_STANDALONE)
				//mouse drag
				if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0)) {
					if(Input.mousePosition.y > screenY/12){

						//Check if any of the tile borders has been reached
						CheckBorders();

                        //Translate the camera
                        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
                        {
                            cam.Translate(-Input.GetAxis("Mouse X") * dragSpeed * 0.7f, -Input.GetAxis("Mouse Y") * dragSpeed * 0.7f, 0);
                            autoCenter = false;
                            centered = false;
                        }
                        //Clamp the camera position to avoid displaying any off the map areas
                        ClampCam();
					}
				}
				#endif

				//Touch drag
				if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
					autoCenter = false;
					centered = false;
					if(Input.GetTouch(0).position.y > screenY/12){
						Vector2 touchDeltaPosition  = Input.GetTouch(0).deltaPosition;
						
						//Check if any of the tile borders has been reached
						CheckBorders();
						//Translate the camera
						cam.Translate(-touchDeltaPosition.x*dragSpeed*Time.deltaTime, -touchDeltaPosition.y*dragSpeed*Time.deltaTime, 0);
						
						//Clamp the camera position to avoid displaying any off the map areas
						ClampCam();
					}
				}
			}	
		}																														
	}

	void CheckBorders(){
		//Reached left tile border
		if(Mathf.Round((mycam.ScreenToWorldPoint(new Vector3(0, 0.5f, cam.position.y)).x)*100.0f)/100.0f <= Mathf.Round((mymap.position.x-mymap.localScale.x*5)*100.0f)/100.0f){
			//show button for borderTile=4;
			tileLeft = true;
		}else{
			//hide button
			tileLeft = false;
		}
		//Reached right tile border
		if(Mathf.Round((mycam.ScreenToWorldPoint(new Vector3(mycam.pixelWidth, 0.5f, cam.position.y)).x)*100.0f)/100.0f >= Mathf.Round((mymap.position.x+mymap.localScale.x*5)*100.0f)/100.0f){
			//show button for borderTile=2;
			tileRight = true;
		}else{
			//hide button
			tileRight = false;
		}
		//Reached bottom tile border
		if(Mathf.Round((mycam.ScreenToWorldPoint(new Vector3(0.5f, 0, cam.position.y)).z)*100.0f)/100.0f <= Mathf.Round((mymap.position.z-mymap.localScale.z*5)*100.0f)/100.0f){
			//show button for borderTile=3;
			tileBottom = true;
		}else{
			//hide button
			tileBottom = false;
		}
		//Reached top tile border
		if(Mathf.Round((mycam.ScreenToWorldPoint(new Vector3(0.5f, mycam.pixelHeight, cam.position.y)).z)*100.0f)/100.0f >= Mathf.Round((mymap.position.z+mymap.localScale.z*5)*100.0f)/100.0f){
			//show button for borderTile=1;
			tileTop = true;
		}else{
			//hide button
			tileTop = false;
		}
	}

	//Disable surrounding tiles cursors
	void CursorsOff(){
		tileTop = false;
		tileBottom = false;
		tileLeft = false;
		tileRight = false;
	}

	//Clamp the camera position
	void ClampCam(){
		Vector3 tmp = cam.position;
		tmp.x = Mathf.Clamp(cam.position.x, 
		                    mymap.position.x-(mymap.localScale.x*5)+(mycam.ScreenToWorldPoint(new Vector3(mycam.pixelWidth, 0.5f, cam.position.y)).x-mycam.ScreenToWorldPoint(new Vector3(0, 0.5f, cam.position.y)).x)/2, 
		                    mymap.position.x+(mymap.localScale.x*5)-(mycam.ScreenToWorldPoint(new Vector3(mycam.pixelWidth, 0.5f, cam.position.y)).x-mycam.ScreenToWorldPoint(new Vector3(0, 0.5f, cam.position.y)).x)/2 );
		tmp.z = Mathf.Clamp(cam.position.z, 
		                    mymap.position.z-(mymap.localScale.z*5)+(mycam.ScreenToWorldPoint(new Vector3(0.5f, mycam.pixelHeight, cam.position.y)).z-mycam.ScreenToWorldPoint(new Vector3(0.5f, 0, cam.position.y)).z)/2, 
		                    mymap.position.z+(mymap.localScale.z*5)-(mycam.ScreenToWorldPoint(new Vector3(0.5f, mycam.pixelHeight, cam.position.y)).z-mycam.ScreenToWorldPoint(new Vector3(0.5f, 0, cam.position.y)).z)/2 );				
		cam.position = tmp;
	}

    ////SAMPLE USER INTERFACE. MODIFY OR EXTEND IF NECESSARY =============================================================
    //void OnGUI()
    //{
    //    GUI.skin.box.alignment = TextAnchor.MiddleCenter;
    //    GUI.skin.box.font = Resources.Load("Neuropol") as Font;
    //    GUI.skin.box.normal.background = Resources.Load("grey") as Texture2D;
    //    if (Screen.width >= Screen.height)
    //    {
    //        GUI.skin.button.fontSize = (int)Mathf.Round(10 * Screen.width / 480);
    //        GUI.skin.box.fontSize = (int)Mathf.Round(10 * Screen.width / 320);
    //    }
    //    else
    //    {
    //        GUI.skin.button.fontSize = (int)Mathf.Round(10 * Screen.height / 480);
    //        GUI.skin.box.fontSize = (int)Mathf.Round(10 * Screen.height / 320);
    //    }

    //    //Display Updating Map message
    //    if (ready && mapping)
    //    {
    //        GUI.Box(new Rect(0, screenY - screenY / 12, screenX, screenY / 12), "Updating...");
    //    }

    //    //Display button to center camera at user position if GUI buttons are not enabled
    //    if (ready && !mapping && !buttons && !centered)
    //    {
    //        if (GUI.Button(new Rect(10 * dot, screenY - 74 * dot, 64 * dot, 64 * dot), centerIcon))
    //        {
    //            centering = true;
    //            StartCoroutine(MapPosition());
    //            StartCoroutine(ReScale());
    //        }
    //    }

    //    //Display surrounding tiles buttons 
    //    if (ready && !mapping)
    //    {
    //        if (tileTop)
    //        {
    //            GUI.DrawTexture(topCursorPos, topIcon);
    //            if (GUI.Button(topCursorPos, "", "label"))
    //            {
    //                borderTile = 1;
    //                StartCoroutine(MapPosition());
    //                StartCoroutine(ReScale());
    //            }
    //        }
    //        if (tileRight)
    //        {
    //            GUI.DrawTexture(rightCursorPos, rightIcon);
    //            if (GUI.Button(rightCursorPos, "", "label"))
    //            {
    //                borderTile = 2;
    //                StartCoroutine(MapPosition());
    //                StartCoroutine(ReScale());
    //            }
    //        }
    //        if (tileBottom)
    //        {
    //            GUI.DrawTexture(bottomCursorPos, bottomIcon);
    //            if (GUI.Button(bottomCursorPos, "", "label"))
    //            {
    //                borderTile = 3;
    //                StartCoroutine(MapPosition());
    //                StartCoroutine(ReScale());
    //            }
    //        }
    //        if (tileLeft)
    //        {
    //            GUI.DrawTexture(leftCursorPos, leftIcon);
    //            if (GUI.Button(leftCursorPos, "", "label"))
    //            {
    //                borderTile = 4;
    //                StartCoroutine(MapPosition());
    //                StartCoroutine(ReScale());
    //            }
    //        }
    //    }

    //    if (ready && !mapping && buttons)
    //    {
    //        GUI.BeginGroup(new Rect(0, screenY - screenY / 12, screenX, screenY / 12));
    //        GUI.Box(new Rect(0, 0, screenX, screenY / 12), "");

    //        //Map type toggle button
    //        if (GUI.Button(new Rect(0, 0, screenX / 5, screenY / 12), maptype[indexType]))
    //        {
    //            if (mapping == false)
    //            {
    //                if (indexType < maptype.Length - 1)
    //                    indexType = indexType + 1;
    //                else
    //                    indexType = 0;
    //                StartCoroutine(MapPosition());
    //                StartCoroutine(ReScale());
    //            }
    //        }

    //        //3D Zoom Buttons
    //        if (triDView)
    //        {
    //            //Zoom In button
    //            if (GUI.Button(new Rect(2 * screenX / 5, 0, screenX / 5, screenY / 12), "zoom +"))
    //            {
    //                if (zoom < maxZoom)
    //                {
    //                    zoom = zoom + 1;
    //                    StartCoroutine(MapPosition());
    //                    StartCoroutine(ReScale());
    //                }
    //            }
    //            //Zoom Out button
    //            if (GUI.Button(new Rect(screenX / 5, 0, screenX / 5, screenY / 12), "zoom -"))
    //            {
    //                if (zoom > minZoom)
    //                {
    //                    zoom = zoom - 1;
    //                    StartCoroutine(MapPosition());
    //                    StartCoroutine(ReScale());
    //                }
    //            }

    //            //2D Zoom Buttons
    //        }
    //        else
    //        {
    //            //Zoom In button
    //            if (GUI.RepeatButton(new Rect(2 * screenX / 5, 0, screenX / 5, screenY / 12), "zoom +"))
    //            {
    //                if (Input.GetMouseButton(0))
    //                {
    //                    currentOrtoSize = mycam.orthographicSize;
    //                    currentOrtoSize = Mathf.MoveTowards(currentOrtoSize, targetOrtoSize / 2, (100 / mapScale) * 32768 * Time.deltaTime / Mathf.Pow(2, zoom));
    //                    mycam.orthographicSize = currentOrtoSize;

    //                    //Clamp the camera position to avoid displaying any off the map areas
    //                    ClampCam();
    //                    CursorsOff();

    //                    //Get touch drag speed for new zoom level
    //                    dragSpeed = 0.8f / 9.594413f * mycam.orthographicSize;

    //                    //Increase zoom level
    //                    if (Mathf.Round(mycam.orthographicSize * 1000.0f) / 1000.0f <= Mathf.Round((targetOrtoSize / 2) * 1000.0f) / 1000.0f && zoom < maxZoom)
    //                    {
    //                        if (!mapping)
    //                        {
    //                            zoom = zoom + 1;
    //                            StartCoroutine(MapPosition());
    //                            StartCoroutine(ReScale());
    //                        }
    //                    }
    //                }
    //            }

    //            //Zoom Out button
    //            if (GUI.RepeatButton(new Rect(screenX / 5, 0, screenX / 5, screenY / 12), "zoom -"))
    //            {
    //                if (Input.GetMouseButton(0))
    //                {
    //                    currentOrtoSize = mycam.orthographicSize;
    //                    currentOrtoSize = Mathf.MoveTowards(currentOrtoSize, targetOrtoSize, (100 / mapScale) * 32768 * Time.deltaTime / Mathf.Pow(2, zoom));
    //                    mycam.orthographicSize = currentOrtoSize;

    //                    //Center camera on map as we zoom out
    //                    currentPosition.x = cam.position.x;
    //                    currentPosition.x = Mathf.MoveTowards(currentPosition.x, mymap.position.x, 10 * 32768 * Time.deltaTime / Mathf.Pow(2, zoom));
    //                    currentPosition.z = cam.position.z;
    //                    currentPosition.z = Mathf.MoveTowards(currentPosition.z, mymap.position.z, 10 * 32768 * Time.deltaTime / Mathf.Pow(2, zoom));
    //                    cam.position = new Vector3(currentPosition.x, cam.position.y, currentPosition.z);

    //                    //Clamp the camera position to avoid displaying any off the map areas
    //                    ClampCam();
    //                    CursorsOff();

    //                    //Get touch drag speed for new zoom level
    //                    dragSpeed = 0.8f / 9.594413f * mycam.orthographicSize;

    //                    //Decrease zoom level
    //                    if (Mathf.Round(mycam.orthographicSize * 1000.0f) / 1000.0f >= Mathf.Round(targetOrtoSize * 1000.0f) / 1000.0f && zoom > minZoom)
    //                    {
    //                        if (!mapping)
    //                        {
    //                            zoom = zoom - 1;
    //                            StartCoroutine(MapPosition());
    //                            StartCoroutine(ReScale());
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        //Update map and center user position 
    //        if (GUI.Button(new Rect(3 * screenX / 5, 0, screenX / 5, screenY / 12), centre))
    //        {
    //            centering = true;
    //            StartCoroutine(MapPosition());
    //            StartCoroutine(ReScale());
    //        }

    //        //Show GPS Status info. Please make sure the GPS_Status.cs script is attached and enabled in the map object.
    //        if (GUI.Button(new Rect(4 * screenX / 5, 0, screenX / 5, screenY / 12), "info"))
    //        {
    //            if (info)
    //            {
    //                infoCanvas.GetComponent<Canvas>().enabled = false;
    //                info = false;
    //            }
    //            else
    //            {
    //                infoCanvas.GetComponent<Canvas>().enabled = true;
    //                info = true;
    //            }
    //        }
    //        GUI.EndGroup();
    //    }
    //}

    //Convert  WebMercator to WGS84
    Vector2 WebMercatortoWGS84 (double x, double z)
    {
        double _lon = (x / 20037508.34) * 180 * mapScale;
        double _lat = (z / 20037508.34) * 180 * mapScale;
        _lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(_lat * Math.PI / 180)) - Math.PI / 2);
        return new Vector2 ((float)_lon, (float)_lat);
    }

    //Convert  WGS84 to WebMercator
    public Vector3 WGS84toWebMercator (float _lon, float _lat, float elevation)
    {
        double x = (_lon * 20037508.34 / 180) / mapScale;
        double z = (Math.Log(Math.Tan((90 + _lat) * Math.PI / 360)) / (Math.PI / 180));
        z = (z * 20037508.34 / 180) / mapScale;
        float y = elevation;
        return new Vector3 ((float)x, y, (float)z);
    }

    //Convert decimal latitude to Degrees Minutes and Seconds
    string convertdmsLat(float lat) {
        string result;
        float degrees;
        float minutes;
        float seconds; 
        degrees = Mathf.Floor(Mathf.Abs(lat)); 
        minutes = (float)((Mathf.Abs(lat)-Mathf.Floor(Mathf.Abs(lat)))*60.0);
        seconds = (float)((minutes-Mathf.Floor(minutes))*60.0);
		result = degrees+"\u00B0 "+Mathf.Floor(minutes)+"' "+seconds.ToString("F2")+"\" "+((lat > 0) ? "N" : "S");
	    return result;
    }  
 
    //Convert decimal longitude to Degrees Minutes and Seconds  
    string convertdmsLon(float lon) {
        string result;
        float degrees;
        float minutes;
        float seconds;
        degrees = Mathf.Floor(Mathf.Abs(lon));
        minutes = (float)((Mathf.Abs(lon)-Mathf.Floor(Mathf.Abs(lon)))*60.0);
        seconds = (float)((minutes-Mathf.Floor(minutes))*60.0);
        result = degrees+"\u00B0 "+Mathf.Floor(minutes)+"' "+seconds.ToString("F2")+"\" "+((lon > 0) ? "E" : "W");
        return result;
    }

    //Move camera on the XZ plane while zooming
    Vector3 ZoomMoveCamera(Vector3 target){
        Vector3 temp = cam.position;
        temp.x = Mathf.MoveTowards(temp.x, target.x, Mathf.Abs(actualDist) * 0.7f * 32768 * Time.deltaTime / Mathf.Pow(2, zoom)); 
        temp.z = Mathf.MoveTowards(temp.z, target.z, Mathf.Abs(actualDist) * 0.7f * 32768 * Time.deltaTime / Mathf.Pow(2, zoom));
        temp.y = cam.position.y;
        return temp;
    }
}
