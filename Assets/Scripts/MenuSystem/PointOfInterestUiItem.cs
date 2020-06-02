using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointOfInterestUiItem : MonoBehaviour {
    public Text PointNumberText, PointNameText,
        PointDistanceText, PointWalkTimeText;
    public Button PointButton;
    public AccessibleButton PointAccessibleButton;
    public GameObject Seperator;
    [HideInInspector]
    public int index;

    public void SeperatorCheckHighlight()
    {
        AppManager.PoIsMenu.SeperatorCheckHighlight(index);
    }

    public void SeperatorCheckIddle()
    {
        AppManager.PoIsMenu.SeperatorCheckIddle(index);
    }
}
