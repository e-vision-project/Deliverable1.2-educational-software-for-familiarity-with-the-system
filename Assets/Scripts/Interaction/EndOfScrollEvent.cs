using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;



public class EndOfScrollEvent : MonoBehaviour
{
    public int pageNumber = -1;
    private GameObject scrollSnapObj;
    private ScrollSnapBase scrollSnap;

    private void Start()
    {
        scrollSnapObj = this.gameObject;
        scrollSnap = scrollSnapObj.GetComponent<ScrollSnapBase>();
        scrollSnap.OnSelectionChangeEndEvent.AddListener(OnPageChangeEnd);
    }

    private void OnPageChangeEnd(int pageNo)
    {
        GameObject content = scrollSnapObj.GetComponent<ScrollRect>().content.gameObject;

        //content.transform.GetChild(pageNo).gameObject.GetComponent<PlaySound>().PlayClip();
        Debug.Log(pageNo);
        pageNumber = pageNo;
    }

}
