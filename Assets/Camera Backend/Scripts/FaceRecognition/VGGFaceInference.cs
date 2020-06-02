using System.Collections;
using System.Collections.Generic;
using EVISION.Camera.plugin;
using FaceRecognition;
using HutongGames.PlayMaker.Actions;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

public class VGGFaceInference : MonoBehaviour
{
    private string input;
    private string input_2;

    public string model;

    public string config;

    private string model_filepath;

    private string config_filepath;

    public DlibFaceDetection _DlibDetector;
    public Net net;

    // Start is called before the first frame update
    void Start()
    {
        _DlibDetector = gameObject.GetComponent<DlibFaceDetection>();
        LoadNet();

        //var img = CVUtils.LoadImgToTexture(Utils.getFilePath(input), TextureFormat.RGB24);
        //var img_2 = CVUtils.LoadImgToTexture(Utils.getFilePath(input_2), TextureFormat.RGB24);
        //var cp_1 = GetCroppedFace(img);
        //var cp_2 = GetCroppedFace(img_2);
        //var rep1 = ExtractEmbeddings(cp_1);
        //var rep2 = ExtractEmbeddings(cp_2);

        //var mat1 = rep1.CreateMatFromEmbeddings(rep1.Embeddings);
        //var mat2 = rep2.CreateMatFromEmbeddings(rep2.Embeddings);
        ////var l2_dist = Core.norm(mat1, mat2, 4);
        ////var l2_norm = Core.norm(mat1, 4) - Core.norm(mat2, 4);
        ////var diff = mat1 - mat2;
        ////var result = diff.dot(diff);
        ////result = Mathf.Sqrt((float)result);
        ////if (result < 105)
        ////{
        ////    Debug.Log(result);
        ////    Debug.Log("similar");
        ////}
        ////if (result > 105)
        ////{
        ////    Debug.Log(result);
        ////    Debug.Log("NOT similar");
        ////}

        //for (int i = 0; i < 128; i++)
        //{
        //    mat1.put(0, i, mat1.get(0, i)[0] / Core.norm(mat1,4));
        //}

        //for (int i = 0; i < 128; i++)
        //{
        //    mat2.put(0, i, mat2.get(0, i)[0] / Core.norm(mat2,4));
        //}

        //var ab = mat1.dot(mat2);
        //var aMagni = Core.norm(mat1);
        //var bMagni = Core.norm(mat2);
        //var cos_sim = ab / (aMagni * bMagni);
        //Debug.Log(1 - cos_sim);
        ////Debug.Log(cos_sim);
    }

    public UnityEngine.Rect GetBB(Mat img)
    {
        var img_Texture = new Texture2D(img.width(), img.height(), TextureFormat.RGB24, false);
        Utils.matToTexture2D(img, img_Texture);
        var rects = _DlibDetector.DetectFaces(img_Texture);
        return _DlibDetector.GetBiggestBoundingBox(rects);
    }

    public UnityEngine.Rect GetBB(Texture2D img)
    {
        var rects = _DlibDetector.DetectFaces(img);
        return _DlibDetector.GetBiggestBoundingBox(rects);
    }

    public Mat GetCroppedFace(Texture2D tex)
    {
        //TickMeter tm = new TickMeter();
        //tm.start();

        var rect = GetBB(tex);
        if (rect.width == 0 && rect.height == 0)
        {
            return null;
        }
        var img = CVUtils.TexToMat(tex);
        var ROI = ExtendBB(img, rect, 0.3f);
        Mat cropped_face = img.submat((int)ROI.y, (int)ROI.y + (int)ROI.height, (int)ROI.x, (int)ROI.width + (int)ROI.x);
        Imgproc.resize(cropped_face, cropped_face, new Size(256, 256));

        //tm.stop();
        //Debug.Log(tm.getTimeMilli());

        return cropped_face;
    }

    public Rect ExtendBB(Mat img, UnityEngine.Rect rect, float scale_factor)
    {
        float x_new = rect.x - rect.width * scale_factor / 2;

        float y_new = rect.y - rect.height * scale_factor / 2;

        float width_new = rect.width * (1 + scale_factor);

        float height_new = rect.height * (1 + scale_factor);

        if (x_new < 0)
        {
            x_new = 1;
        }
        if (y_new < 0)
        {
            y_new = 1;
        }

        var xmax = x_new + width_new;
        var ymax = y_new + height_new;
        var img_width = img.size().width;
        var img_height = img.size().height;
        if (xmax > img_width)
        {
            width_new = rect.width;
            x_new = rect.x;
        }
        if (ymax > img_height)
        {
            height_new = rect.height;
            y_new = rect.y;
        }
        Rect ROI = new Rect((int)x_new, (int)y_new, (int)width_new, (int)height_new);
        return ROI;
    }

    public FaceEmbeddings ExtractEmbeddings(Mat img)
    {
        //TickMeter tm = new TickMeter();
        //tm.start();

        Imgproc.cvtColor(img,img, Imgproc.COLOR_RGB2BGR);

        if (img.empty())
        {
            Debug.LogError(model_filepath + " is not loaded. Please see \"StreamingAssets/dnn/setup_dnn_module.pdf\". ");
            img = new Mat(424, 640, CvType.CV_8UC3, new Scalar(0, 0, 0));
        }

        Imgproc.resize(img, img, new Size(224,224),0,0,2);
        var faceBlob = Dnn.blobFromImage(img, 1, new Size(224, 224), new Scalar(91.4953, 103.8827, 131.0912));
        net.setInput(faceBlob);

        var netOut = net.forward();

        var embeddings = new FaceEmbeddings(netOut, 128);


        //if (gameObject.GetComponent<Renderer>() != null)
        //{
        //    GenericUtils.AdjustImageScale(img, this.gameObject);
        //    Texture2D texture = new Texture2D(img.cols(), img.rows(), TextureFormat.RGB24, false);
        //    Utils.matToTexture2D(img, texture);
        //    gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        //}

        img.Dispose();
        netOut.Dispose();

        //tm.stop();
        //Debug.Log("inference:" + tm.getTimeMilli());

        return embeddings;
    }

    public void DisposeNet()
    {
        net.Dispose();
    }

    public void LoadNet()
    {
        if (net == null)
        {
            net = CVUtils.LoadModel(model, config, model_filepath, config_filepath);
        }
    }
}