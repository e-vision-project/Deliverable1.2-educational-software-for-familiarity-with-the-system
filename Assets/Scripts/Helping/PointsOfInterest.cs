using System;
using UnityEngine;

[Serializable]
public class PointsOfInterest {
    public string Name;
    public string UiItemText;
    public double TriggerLatitude, TriggerLongitude;
    public int DistanceFromTrigger;
    public double DistanceFromPoint;
    public int WalkTimeToPoint;
    public GameObject PointCanvas;
    public Sprite MapSnapShot;
    public bool HaveOpenedOnce = false;
    public float TimeOpened;
}
