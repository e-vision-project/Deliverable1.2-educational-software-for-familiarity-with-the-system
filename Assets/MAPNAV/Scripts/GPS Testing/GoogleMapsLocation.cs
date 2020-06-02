using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GoogleMapsLocation 
{

    public List<Result> results ;
    public string status ;

    [Serializable]
    public class AddressComponent
    {
        public string long_name ;
        public string short_name ;
        public List<string> types ;
    }

    [Serializable]
    public class Location
    {
        public double lat ;
        public double lng ;
    }

    [Serializable]
    public class Northeast
    {
        public double lat ;
        public double lng ;
    }

    [Serializable]
    public class Southwest
    {
        public double lat ;
        public double lng ;
    }

    [Serializable]
    public class Viewport
    {
        public Northeast northeast ;
        public Southwest southwest ;
    }

    [Serializable]
    public class Geometry
    {
        public Location location ;
        public string location_type ;
        public Viewport viewport ;
    }

    [Serializable]
    public class Result
    {
        public List<AddressComponent> address_components ;
        public string formatted_address ;
        public Geometry geometry ;
        public string place_id ;
        public List<string> types ;
    }
   
}
