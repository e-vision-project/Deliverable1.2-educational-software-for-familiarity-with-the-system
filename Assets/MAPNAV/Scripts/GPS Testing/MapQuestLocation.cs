using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapQuestLocation
{
   


    public Info info;
    public Options options;
    public List<Result> results;


    [Serializable]
    public class Copyright
    {
        public string text ;
        public string imageUrl ;
        public string imageAltText ;
    }

    [Serializable]
    public class Info
    {
        public int statuscode;
        public Copyright copyright;
        public List<object> messages;
    }

    [Serializable]
    public class Options
    {
        public int maxResults;
        public bool thumbMaps;
        public bool ignoreLatLngInput;
    }

    [Serializable]
    public class LatLng
    {
        public double lat;
        public double lng;
    }

    [Serializable]
    public class ProvidedLocation
    {
        public LatLng latLng;
    }

    [Serializable]
    public class LatLng2
    {
        public double lat;
        public double lng;
    }

    [Serializable]
    public class DisplayLatLng
    {
        public double lat;
        public double lng;
    }

    [Serializable]
    public class LatLng3
    {
        public double longitude;
        public double latitude;
    }

    [Serializable]
    public class NearestIntersection
    {
        public string streetDisplayName;
        public string distanceMeters;
        public LatLng3 latLng;
        public string label;
    }

    [Serializable]
    public class RoadMetadata
    {
        public string speedLimitUnits;
        public object tollRoad;
        public int speedLimit;
    }

    [Serializable]
    public class Location
    {
        public string street ;
        public string adminArea6 ;
        public string adminArea6Type ;
        public string adminArea5 ;
        public string adminArea5Type ;
        public string adminArea4 ;
        public string adminArea4Type ;
        public string adminArea3 ;
        public string adminArea3Type ;
        public string adminArea1 ;
        public string adminArea1Type ;
        public string postalCode ;
        public string geocodeQualityCode ;
        public string geocodeQuality ;
        public bool dragPoint ;
        public string sideOfStreet ;
        public string linkId ;
        public string unknownInput ;
        public string type ;
        public LatLng2 latLng ;
        public DisplayLatLng displayLatLng ;
        public string mapUrl ;
        public NearestIntersection nearestIntersection ;
        public RoadMetadata roadMetadata ;
    }

    [Serializable]
    public class Result
    {
        public ProvidedLocation providedLocation;
        public List<Location> locations;
    }


}
