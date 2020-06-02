//MAPNAV Navigation ToolKit v.1.5.0
//Please attach to empty object

using UnityEngine;
using System;
using System.Collections;
[AddComponentMenu("MAPNAV/GetDistance")]

public struct locWGS84
{
	public double Latitude, Longitude;

    public locWGS84(double latitude, double longitude)
    {
		Latitude = latitude;
		Longitude = longitude;
    }
}

public class GetDistance : MonoBehaviour
{
	public Transform[] waypoints;
	public bool renderPath = true;
	public Color pathColor = new Color (0.1412f, 0.0f, 0.1137f, 1f);
	public float pathWidth = 0.05f;
    public Material myMaterial;
    private float initX;
	private float initZ;
	private MapNav gps;
    private bool gpsFix = true;

	public float updateRate = 0.1f;
	public double totalDistance;
	public double milesDist;
	public double metersDist;
	public double feetDist;
    
	void Awake()
	{
		//Reference to the MapNav.js script and gpsFix variable. gpsFix will be true when a valid location data has been set.
		gps = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
        gpsFix = true;//= gps.gpsFix;

	}

	IEnumerator Start()
	{
        Debug.Log("Trying");
        //Wait until the gps sensor provides a valid location.
        while (!gpsFix)
        {
            gpsFix = gps.gpsFix;
            yield return null;
        }
        //Read initial position (used as a reference system)
        initX = gps.WGS84toWebMercator(22.95574f, 40.61363f, 0f).x; // gps.iniRef.x;

        initZ = gps.WGS84toWebMercator(22.95574f, 40.61363f, 0f).z; //gps.iniRef.z;


        if (renderPath)
        {
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer> ();
            myMaterial = (Material)Resources.Load("Distance", typeof(Material));
            lineRenderer.material = myMaterial;
            lineRenderer.enabled=true;
			lineRenderer.startColor =pathColor;
            lineRenderer.endColor = lineRenderer.startColor;

            lineRenderer.startWidth = pathWidth * 100 / gps.mapScale;
            lineRenderer.endWidth = lineRenderer.startWidth;

            lineRenderer.positionCount= waypoints.Length;
		}

		InvokeRepeating("MyDistance", 1, updateRate);
	}

	void MyDistance (){
		if (gpsFix)
        {
			totalDistance = 0.0f;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                //Distance between waypoint[i] and waypoint[i+1]
                double leg = GeoDistance(Pos2Loc(waypoints[i].position), Pos2Loc(waypoints[i + 1].position));

                Debug.Log("LEG : " + leg);
                //add to totalDistance
                totalDistance += leg;
            }
			if(renderPath)
            {
				LineRenderer lineRenderer = GetComponent<LineRenderer>();
				for (int j=0; j<waypoints.Length; j++)
                {
					Vector3 flatPos = new Vector3(waypoints [j].position.x,0.01f,waypoints [j].position.z);
					lineRenderer.SetPosition(j, flatPos);
				}

                if (gps.mapping)
                {
                    lineRenderer.enabled = false;
                }
                else {
                    lineRenderer.enabled = true;
                }
            }

           // totalDistance = GeoDistance(new locWGS84(40.62401, 22.95532), new locWGS84(40.62531, 22.95378));
            metersDist = totalDistance * 1000;
			milesDist = totalDistance * 0.621371192;
			feetDist = totalDistance * 3280.83989501;
			Debug.Log ("Total Distance: " + Math.Round(totalDistance,3).ToString() + " Km");
		}
    } 

	public double GeoDistance (locWGS84 loc1, locWGS84 loc2)
	{
		double radius = 6371;
		double dLat = this.toRadian(loc2.Latitude - loc1.Latitude);
		double dLon = this.toRadian(loc2.Longitude - loc1.Longitude);
		double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
			Math.Cos(this.toRadian(loc1.Latitude)) * Math.Cos(this.toRadian(loc2.Latitude)) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
		double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
		double d = radius * c;
		return d;
	}

	public locWGS84 Pos2Loc (Vector3 wayPoint)
    {
        double lon = ((wayPoint.x + initX) / 20037508.34) * 180 * gps.mapScale;
        double lat = ((wayPoint.z + initZ) / 20037508.34) * 180 * gps.mapScale;
        lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);

        locWGS84 result = new locWGS84 (lat,lon);
		return result;
	
	}

	// Convert to Radians.
	private double toRadian(double val)
	{
		return (Math.PI / 180) * val;
	}
}	
