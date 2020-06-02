using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognition;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.UI;

public class FaceRecognitionTest : MonoBehaviour
{
    public Button addFolderBtn;
    public Button recFolderBtn;

    public Button addBtn;
    public Button removeBtn;
    public Button RecognizeBtn;

    public InputField addPath;
    public InputField addFolderPath;
    public InputField recFolderPath;

    public InputField addName;
    public InputField removeName;
    public InputField recPath;
    public Text text;

    public FaceRecMng faceRecMng;
    public List<string> imageFiles;
    public List<string> recoFiles;

    // Start is called before the first frame update
    void Start()
    {
        addBtn.onClick.AddListener((() => AddBtnListener()));
        addFolderBtn.onClick.AddListener((() => AddFolderListener()));
        recFolderBtn.onClick.AddListener((() => RecFolderListener()));
        removeBtn.onClick.AddListener(() => RemoveBtnListener());
        RecognizeBtn.onClick.AddListener(() => RecognizeBtnListener());

        imageFiles = Directory.GetFiles(addFolderPath.text, "*.*", SearchOption.AllDirectories)
            .Where(file => new string[] { ".JPG", ".jpg", ".png" }
                .Contains(Path.GetExtension(file)))
            .ToList();

        recoFiles = Directory.GetFiles(recFolderPath.text, "*.*", SearchOption.AllDirectories)
            .Where(file => new string[] { ".JPG", ".jpg", ".png" }
                .Contains(Path.GetExtension(file)))
            .ToList();
    }

    private void AddBtnListener()
    {
        var n = addName.text;
        var tex = LoadTexture(addPath.text);
        faceRecMng.AddFace(n, tex);
    }

    private void AddFolderListener()
    {
        StartCoroutine(AddFolderToDict());
    }

    private void RecFolderListener()
    {
        RecognizeFolder();
    }

    private IEnumerator AddFolderToDict()
    {
        foreach (var file in imageFiles)
        {
            var n = file.Replace(addFolderPath.text, "");
            var n_2 = n.Remove(0, 1);
            var tex = CVUtils.LoadImgToTexture("FR/LFW_Train/" + n, TextureFormat.RGBA32);
            faceRecMng.AddFace(n_2, tex);
            yield return null;
        }
    }

    private void RecognizeFolder()
    {
        foreach (var file in recoFiles)
        {
            var n = file.Replace(recFolderPath.text, "");
            var n_2 = n.Remove(0, 1);
            var tex = CVUtils.LoadImgToTexture($"FR/{recPath.text}/" + n, TextureFormat.RGBA32);
            var input_emb = faceRecMng.ExtractEmbeddings(tex);
            var dict = faceRecMng.StoredFaces.serializedDict;
            var matchResults = new Dictionary<string, double>();
            foreach (var key in dict.Keys)
            {
                var dist = faceRecMng.CompareSimilarity(input_emb, dict[key]);
                matchResults.Add(key, dist);
            }

            var matched = (from match in matchResults where match.Value < 0.5 orderby match.Value select match).ToList();
            if (matched.Count == 0)
            {
                Debug.Log("no match");
            }
            else
            {
                Debug.Log(matched[0].Key);
            }
        }
    }

    private void RemoveBtnListener()
    {
        var n = removeName.text;
        faceRecMng.DeleteFace(n);
    }

    private void RecognizeBtnListener()
    {
        var tex = CVUtils.LoadImgToTexture(Utils.getFilePath(recPath.text), TextureFormat.RGB24);
        var input_emb = faceRecMng.ExtractEmbeddings(tex);
        var dict = faceRecMng.StoredFaces.serializedDict;
        var matchResults = new Dictionary<string, double>();
        foreach (var key in dict.Keys)
        {
            var dist = faceRecMng.CompareEmbeddings(input_emb, dict[key]);
            matchResults.Add(key,dist);
        }

        //var matched = matchResults.Select(m => m).Where(k => k.Value < 0.99).ToList();
        var matched = (from match in matchResults where match.Value < 0.5 orderby match.Value select match ).ToList();
        if (matched.Count == 0)
        {
            text.text = "no match";
        }
        else
        {
            text.text = matched[0].Key;
        }
    }


    private Texture2D LoadTexture(string path)
    {
        Mat img = Imgcodecs.imread(Utils.getFilePath(path));
        //Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);
        if (img.empty())
        {
            Debug.LogError("image is not loaded");
            var tex = new Texture2D(img.width(), img.height(), TextureFormat.RGB24, false);
            return tex;
        }
        var imageTex = new Texture2D(img.width(), img.height(), TextureFormat.RGB24, false);
        Utils.matToTexture2D(img, imageTex);
        return imageTex;
    }
}
