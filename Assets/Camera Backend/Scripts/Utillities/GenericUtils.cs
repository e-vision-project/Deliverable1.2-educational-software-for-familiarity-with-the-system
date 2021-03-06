﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using OpenCVForUnity.CoreModule;
using Rect = UnityEngine.Rect;

namespace EVISION.Camera.plugin
{
    public enum CameraType { Webcam, arCam }

    public static class GenericUtils
    {

        public static List<string> SplitStringToList(string text)
        {
            if(text != null)
            {
                List<string> l = text.Split(new Char[] { ' ', '\n', ','}).ToList();
                return l;
            }
            Debug.LogError("Null text passed to SplitStringToList function");
            return null;
            
        }

        public static List<string> SplitStringToList(string text, string stringBreak)
        {
            if (text != null)
            {
                List<string> l = text.Split(new string[] {stringBreak}, StringSplitOptions.None).ToList();
                return l;
            }
            Debug.LogError("Null text passed to SplitStringToList function");
            return null;

        }

        public static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        public static void ScaleTextureBilinear(Texture2D originalTexture, int width, int height)
        {
            TextureScale.Bilinear(originalTexture, width, height);
        }

        public static void ScaleTexturePoint(Texture2D originalTexture, int width, int height)
        {
            TextureScale.Point(originalTexture, width, height);
        }

        public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Trilinear;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            Texture2D nTex = new Texture2D(newWidth, newHeight);
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            nTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        public static Texture2D CopyTexture(Texture2D source)
        {
            Texture2D returnedTex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
            Color[] sourcePixels = source.GetPixels();
            returnedTex.SetPixels(sourcePixels);
            returnedTex.Apply();
            return returnedTex;
        }

        public static Texture2D CombineTextures(Texture2D aBaseTexture, Texture2D aToCopyTexture)
        {
            int aWidth = aBaseTexture.width;
            int aHeight = aBaseTexture.height;
            Texture2D aReturnTexture = new Texture2D(aWidth, aHeight, TextureFormat.RGBA32, false);

            Color[] aBaseTexturePixels = aBaseTexture.GetPixels();
            Color[] aCopyTexturePixels = aToCopyTexture.GetPixels();
            Color[] aColorList = new Color[aBaseTexturePixels.Length];
            int aPixelLength = aBaseTexturePixels.Length;

            for (int p = 0; p < aPixelLength; p++)
            {
                aColorList[p] = Color.Lerp(aBaseTexturePixels[p], aCopyTexturePixels[p], aCopyTexturePixels[p].a);
            }

            aReturnTexture.SetPixels(aColorList);
            aReturnTexture.Apply(false);

            return aReturnTexture;
        }

        public static double[] ConvertToDouble(float[] inputArray)
        {
            if (inputArray == null)
                return null;

            double[] output = new double[inputArray.Length];
            for (int i = 0; i < inputArray.Length; i++)
                output[i] = inputArray[i];

            return output;
        }

        public static string ListToString(List<string> _validWords)
        {
            string OCR_string = "";

            foreach (var word in _validWords)
            {
                OCR_string += word + " ";
            }

            return OCR_string;
        }

        public static Texture2D RenderTexToTex2D(Texture texture)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture2D;
        }

        public static float CalculateTimeDifference(float start, float end)
        {
            float timeToCompleteSec = 0;

            timeToCompleteSec = (float)System.Math.Round(end - start, 2);

            return timeToCompleteSec;
        }

        public static void SaveTXT(string text)
        {
            string path = "";
            string imagePath = "";

#if UNITY_EDITOR_WIN
            path = Application.dataPath + "/evision_result_logs.txt";
#endif

#if UNITY_ANDROID
            path = Application.persistentDataPath + "/evision_result_logs.txt";
#endif

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "Result logs \n");
            }

            File.AppendAllText(path, text);
            Debug.Log("saved as :" + path);
        }

        public static void SaveImageFile(Texture2D tex)
        {
            string imagePath = "";


            if (Application.isEditor)
            {
                imagePath = Application.dataPath + "/captured_images";
            }
            else
            {
                imagePath = Application.persistentDataPath + "/captured_images";
            }

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            var bytes = tex.EncodeToJPG();
            int capture_count = 0;
            string capture_name = string.Format("/{0}_Capture{1}.png", Application.productName, capture_count.ToString());
            System.IO.File.WriteAllBytes(imagePath + capture_name, bytes);
            Debug.Log(imagePath + capture_name);
        }

        public static void AdjustImageScale(Mat img, GameObject gameObj)
        {
            //Adust Quad.transform.localScale.
            gameObj.transform.localScale = new Vector3(img.width(), img.height(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float imageWidth = img.width();
            float imageHeight = img.height();

            float widthScale = (float)Screen.width / imageWidth;
            float heightScale = (float)Screen.height / imageHeight;
            if (widthScale < heightScale)
            {
                UnityEngine.Camera.main.orthographicSize = (imageWidth * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                UnityEngine.Camera.main.orthographicSize = imageHeight / 2;
            }
        }

        public static void AdjustImageScale(Texture2D img, GameObject gameObj)
        {
            //Adust Quad.transform.localScale.
            gameObj.transform.localScale = new Vector3(img.width, img.height, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " +
                      Screen.orientation);

            float width = gameObj.transform.localScale.x;
            float height = gameObj.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                UnityEngine.Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                UnityEngine.Camera.main.orthographicSize = height / 2;
            }
        }

        public static string RemoveGreekAccent(string word)
        {
            string edited = new StringBuilder(word)
                .Replace('ά', 'α')
                .Replace('ί', 'ι')
                .Replace('ή', 'η')
                .Replace('ύ', 'υ')
                .Replace('ό', 'ο')
                .Replace('ώ', 'ω')
                .Replace('έ', 'ε')
                .ToString().ToUpper();
            return edited;
        }
        
    }
}
