using UnityEngine;
using UnityEditor;

public class ClearPrefsEditor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}