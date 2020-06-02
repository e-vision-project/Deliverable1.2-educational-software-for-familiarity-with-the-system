using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

public static class CVUtils
{
    public static Mat LoadImgToMat(string path)
    {
        Mat img = Imgcodecs.imread(Utils.getFilePath(path));
        if (img.empty())
        {
            Debug.LogError("image is not loaded");
            return img;
        }
        return img;
    }

    public static Texture2D LoadImgToTexture(string path, TextureFormat textureFormat)
    {
        Mat img = Imgcodecs.imread(Utils.getFilePath(path));
        if (img.empty())
        {
            Debug.LogError("image is not loaded");
            var tex = new Texture2D(img.width(), img.height(), textureFormat, false);
            return tex;
        }
        var imageTex = new Texture2D(img.cols(), img.rows(), textureFormat, false);
        Utils.matToTexture2D(img, imageTex);
        img.Dispose();
        return imageTex;
    }

    /// <summary>
    /// Gets the outputs types.
    /// </summary>
    /// <returns>The outputs types.</returns>
    /// <param name="net">Net.</param>
    public static List<string> GetOutputsNames(Net net)
    {
        List<string> names = new List<string>();


        MatOfInt outLayers = net.getUnconnectedOutLayers();
        for (int i = 0; i < outLayers.total(); ++i)
        {
            names.Add(net.getLayer(new DictValue((int)outLayers.get(i, 0)[0])).get_name());
        }
        outLayers.Dispose();

        return names;
    }

    /// <summary>
    /// Reads the class names.
    /// </summary>
    /// <returns>The class names.</returns>
    /// <param name="filename">Filename.</param>
    public static List<string> GetOutputsTypes(Net net)
    {
        List<string> types = new List<string>();


        MatOfInt outLayers = net.getUnconnectedOutLayers();
        for (int i = 0; i < outLayers.total(); ++i)
        {
            types.Add(net.getLayer(new DictValue((int)outLayers.get(i, 0)[0])).get_type());
        }
        outLayers.Dispose();

        return types;
    }

    public static Net LoadModel(string model, string config, string modelFilePath, string configFilePath)
    {
        if (!string.IsNullOrEmpty(model)) modelFilePath = Utils.getFilePath("dnn/" + model);
        if (!string.IsNullOrEmpty(config)) configFilePath = Utils.getFilePath("dnn/" + config);
        var net = Dnn.readNetFromCaffe(configFilePath, modelFilePath);
        if (net == null)
        {
            Debug.Log("problem loading the model.");
        }
        return net;
    }

    public static Mat TexToMat(Texture2D tex)
    {
        Mat img = new Mat(tex.height, tex.width, CvType.CV_8UC3);
        Utils.texture2DToMat(tex, img);
        return img;
    }
}
