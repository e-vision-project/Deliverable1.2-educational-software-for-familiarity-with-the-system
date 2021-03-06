﻿using EVISION.Camera.plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpImageLoading : MonoBehaviour
{
    public string imageUrl;
    public bool videoMode;
    public Enums.ScenarioCases ScenarioCase;
    private readonly string photosFolder = "http://192.168.1.254/DCIM/PHOTO/";
    public static bool snapTaken;
    public bool photoRemoved;
    public static bool textureLoaded;
    public bool cameraConnected = false;
    private static bool firstConnection = true;
    private WaitForEndOfFrame _frameBreak;
    [SerializeField] private CameraClient client;

    public delegate void OnCameraConnected();
    public static event OnCameraConnected onCameraConnected;

    public delegate void OnCameraNotConnected();
    public static event OnCameraNotConnected onCameraNotConnected;
    private Texture2D tempTex;

    // Start is called before the first frame update
    public void Start()
    {
        client = gameObject.GetComponent<CameraClient>();
        _frameBreak = new WaitForEndOfFrame();
    }

    public void OnEnable()
    {
        //EventCamManager.onCheckCameraConnection += CheckConnectionStatus;
        //onCameraNotConnected += OnCameraNotConnectedListener;
        //onCameraConnected += OnCameraConnectedListener;
    }

    public void CheckConnectionStatus()
    {
        StartCoroutine(IsCameraConnected());
    }

    public void OnCameraNotConnectedListener()
    {
        client.text_result = "Κάμερα αποσυνδέθηκε, επιστρέψτε στο κεντρικό μενού";
        UAP_AccessibilityManager.Say("Κάμερα αποσυνδέθηκε, επιστρέψτε στο κεντρικό μενού");
        cameraConnected = false;
    }

    public void OnCameraConnectedListener()
    {
        UAP_AccessibilityManager.Say("Κάμερα συνδέθηκε");
        //set to photo mode
        if (cameraConnected)
        {
            Debug.Log("Camera already initialized");
        }
        else
        {
            Debug.Log("Initialize camera");
            cameraConnected = true;
            firstConnection = true;
            StartCoroutine(InitCamera());
        }
    }

    public IEnumerator InitCamera()
    {
        if (videoMode)
        {
            firstConnection = false;
            RemoveAllPhotos();
            yield return null;
        }
        else
        {
            RemoveAllPhotos();
            firstConnection = false;
            yield return StartCoroutine(SetRecordingRequest(1));
        }
    }

    /// <summary>
    /// This method aims to communicate with an http server and load the newest added image in the server
    /// to a texture 2D.
    /// </summary>
    /// <returns>IEnumarator gameobject</returns>
    public async Task<Texture2D> LoadTextureFromImage()
    {
        StartCoroutine(TakePhotoRequest());
        //Debug.Log("step 1: take photo");
        while (!snapTaken && !firstConnection)
        {
            await _frameBreak;
        }
        //Debug.Log("step 2: get name");
        var photoNames = GetPhotoNames(photosFolder); // get all photos contained in the server.
        var photoName = GetCapturedPhotoName(photoNames); // get the newest one.
        imageUrl = $"{photosFolder}{photoName}";
        //Debug.Log("step 3: download tex");
        StartCoroutine(GetURLTexture(imageUrl));
        while (!textureLoaded)
        {
            await _frameBreak;
        }
        //Debug.Log("step 4: delete");
        StartCoroutine(SendRemovePhotoRequest(imageUrl));
        while (!photoRemoved)
        {
            await _frameBreak;
        }

        if (client != null)
        {
            client.textureReceived = true;
        }
        textureLoaded = false;
        photoRemoved = false;
        snapTaken = false;
        return tempTex;
    }

    /// <summary>
    /// This method finds the addresses of the files contained in the specified http server and 
    /// removes them.
    /// </summary>
    private void RemoveAllPhotos()
    {
        var savedPhotos = GetPhotoNames(photosFolder);
        if (savedPhotos.Count != 0)
        {
            savedPhotos.RemoveAll(s => s.Contains("Remove"));
            for (int i = 0; i < savedPhotos.Count; i++)
            {
                var x = savedPhotos[i].Replace("<b>", string.Empty);
                x = x.Replace("</b>", string.Empty);
                StartCoroutine(SendRemovePhotoRequest(photosFolder + x));
            }
            firstConnection = false;
        }
    }

    /// <summary>
    /// This method load an image file from a specified address of an http server
    /// to a texture 2D object.
    /// </summary>
    /// <param name="url">The url address of the file in the server</param>
    /// <param name="camTex"></param>
    /// <returns>Ienumarator gameobject</returns>
    public IEnumerator GetURLTexture(string url)
    {
        var x = UnityWebRequestTexture.GetTexture(url);
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError)
        {
            Debug.Log("Get url texture error" + x.error);
        }
        else if (x.isDone)
        {
            tempTex = null;
            tempTex = ((DownloadHandlerTexture)x.downloadHandler).texture;
            textureLoaded = true;
        }
        x.Dispose();
        Resources.UnloadUnusedAssets();
    }

    public IEnumerator LoadImageToTex(Texture2D tex, string url)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            textureLoaded = true;
        }
    }

    public string GetCapturedPhotoName(List<string> names)
    {
        // every photo has also a remove text on the client, therefore the last list item is a remove text.
        names.RemoveAll(s => s.Contains("Remove"));
        int index = names.Count - 1;
        var x = names[0].Replace("<b>", string.Empty);
        x = x.Replace("</b>", string.Empty);
        Debug.Log("Photo name acquired");
        return x;
    }

    /// <summary>
    /// This methods find and returns a list with all the file names contained in the specified url address.
    /// </summary>
    /// <param name="url">The url address to the http server</param>
    /// <returns>List of strings</returns>
    private List<string> GetPhotoNames(string url)
    {
        WebRequest request = WebRequest.Create(url);
        WebResponse response = request.GetResponse();
        Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            string result = reader.ReadToEnd();
            MatchCollection matches = regex.Matches(result);
            if (matches.Count == 0)
            {
                Debug.Log("No files inside the folder found");
            }
            List<string> names = new List<string>();
            foreach (Match match in matches)
            {
                if (!match.Success) { continue; }
                //Debug.Log("folder Match: " + match.Groups["name"]);
                var name = match.Groups["name"].Value;
                names.Add(name);
            }
            response.Dispose();
            return names;
        }
    }

    /// <summary>
    /// Sends a Delete request to the specified address of the http server.
    /// </summary>
    /// <param name="url">The url address to the http server</param>
    /// <returns>IEnumarator gameobject</returns>
    public IEnumerator SendRemovePhotoRequest(string url)
    {
        var request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log("error in deleting photo");
        }
        else
        {
            photoRemoved = true;
            Debug.Log("deleted photo");
        }
        request.Dispose();
    }

    /// <summary>
    /// This method sends a predifined Get request to the http server. The address sent address
    /// can work only with the Gogloo E7-E9 wifi glasses.
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakePhotoRequest()
    {
        var request = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=1001");
        float startTime = Time.realtimeSinceStartup;
        request.timeout = 3;

        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError || request.timeout > 3)
        {
            StartCoroutine((TakePhotoRequest()));
        }
        else
        {
            if (!firstConnection)
            {
                //Debug.Log("snap taken");
                snapTaken = true;
            }
            //Debug.Log("Request for photo sent");
        }
        request.Dispose();
    }

    public IEnumerator IsCameraConnected()
    {
        var request = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=3027");
        float startTime = Time.realtimeSinceStartup;
        request.timeout = 5;

        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError || request.timeout > 5)
        {
            OnCameraNotConnectedListener();
        }
        else
        {
            OnCameraConnectedListener();
        }
    }

    private void DisableHttpCamera()
    {
        EventCamManager.onExternalCamError?.Invoke();

        cameraConnected = false;
        if (GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_HTTP") != null)
        {
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_HTTP").SetActive(false);
        }
        if (GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL") != null)
        {
            GameObject.FindGameObjectWithTag("DISPLAY_IMAGE_EXTERNAL").SetActive(false);
        }
    }

    public IEnumerator SetCameraModeRequest(int mode)
    {
        if(mode > 1 && mode < 0) { Debug.Log("valid mode input is 0 or 1."); yield return null; }
        Debug.Log("camera mode");
        var x = UnityWebRequest.Get(String.Format("http://192.168.1.254/?custom=1&cmd=3001&par={0}", mode));
        x.timeout = 5;
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError || x.timeout > 5)
        {
            Debug.Log(x.error);
        }
        else
        {
            x.Dispose();
            StartCoroutine(TakePhotoRequest());
        }
    }

    public IEnumerator SetRecordingRequest(int mode)
    {
        if (mode > 1 && mode < 0) { Debug.Log("valid mode input is 0 or 1."); yield return null; }
        var x = UnityWebRequest.Get(String.Format("http://192.168.1.254/?custom=1&cmd=2001&par={0}", mode));
        x.timeout = 4;
        yield return x.SendWebRequest();
        if (x.isNetworkError || x.isHttpError || x.timeout > 4)
        {
            Debug.Log(x.error);
        }
        else
        {
            cameraConnected = true;
        }
    }

    public IEnumerator GetConnectionStatus()
    {
        var x = UnityWebRequest.Get("http://192.168.1.254/?custom=1&cmd=3027");
        x.timeout = 3;
        yield return x.SendWebRequest();
        if (x.isHttpError || x.isNetworkError || x.timeout > 3)
        {
            Debug.Log("camera connection error :" + x.error);
            cameraConnected = false;
            EventCamManager.onExternalCamError?.Invoke();
        }
        else if (x.isDone)
        {
            Debug.Log("camera connected");
            cameraConnected = true;
        }
    }

    public Texture2D GetUrlTextureObsolete(string url)
    {
        WWW www = new WWW(url);
        while (www.isDone == false)
        {
            continue;
        }
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        www.LoadImageIntoTexture(texture);
        Debug.Log("Done");
        return texture;
    }

   
}
