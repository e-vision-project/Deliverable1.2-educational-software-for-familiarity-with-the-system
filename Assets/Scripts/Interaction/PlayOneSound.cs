using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOneSound : MonoBehaviour {

    public AudioClip clip;
    public AudioSource source;
    public int ID;


    public void PlayClip()
    {
        Debug.Log("Play");
        source.PlayOneShot(clip);
    }

    
}
