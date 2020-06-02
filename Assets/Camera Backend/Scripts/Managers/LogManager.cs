using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public static class LogManager
{
    public static string OCRWordsText;
    public static string MajorityValidText;
    public static string MajorityFinalText;
    public static string majorityFinal;
    public static string TimeText;
    public static bool textUploaded;
    public static string uniqueId;

    public static void SaveResultLogs(string text)
    {
        string path;
        #if UNITY_EDITOR_WIN
        path = Application.dataPath + "/evision_result_logs.txt";
        #endif

        #if UNITY_ANDROID || UNITY_IOS
        path = Application.persistentDataPath + "/evision_result_logs.txt";
        #endif

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "\n");
        }

        File.AppendAllText(path, text);
        Debug.Log("saved as :" + path);
    }

    public static void SaveImageFile(Texture2D tex, string fileName)
    {
        string imagePath = "";
        #if UNITY_EDITOR_WIN
            imagePath = Application.dataPath + "/captured_images";
        #endif

        #if UNITY_ANDROID || UNITY_IOS
            imagePath = Application.persistentDataPath + "/captured_images";
        #endif

        if (!Directory.Exists(imagePath))
        {
            Directory.CreateDirectory(imagePath);
        }

        var bytes = tex.EncodeToJPG();
        File.WriteAllBytes($"{imagePath}/{fileName}.jpg", bytes);
        Debug.Log("saved as" + $"{imagePath}/{fileName}");
    }

    public static string GetResponseTime(string cameraTime, string cloudTime, string classTime, string majTime, string sum)
    {
        string text = "#Response Times: " + "<Receive image>: " + cameraTime + "|| <Cloud vision>: " + cloudTime + "|| <Classification>: " + classTime +
                      "|| <Majority>: " + majTime + "|| <sum>: " + sum;
        return text;
    }

    public static string GetResponseTime(string cameraTime, string cloudTime, string classTime, string sum)
    {
        string text = "#Response Times: " + "<Receive image>: " + cameraTime + "|| <Cloud vision>: " + cloudTime + "|| <Classification>: " + classTime
                       + "|| <sum>: " + sum;
        return text;
    }

    public static string GetResponseTime(string cameraTime, string detectTime, string sum)
    {
        string text = "#Response Times: " + "<Receive image>: " + cameraTime  + "|| <DetectionTime>: " + detectTime
                      + "|| <sum>: " + sum;
        return text;
    }

    public static string GetResultLogs(string title ,string results)
    {
        string text = string.Format("#{0}: ", title) + results;
        return text;
    }

    public static void SetUniqueId()
    {
        StringBuilder builder = new StringBuilder();
        Enumerable
            .Range(65, 26)
            .Select(e => ((char)e).ToString())
            .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
            .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
            .OrderBy(e => Guid.NewGuid())
            .Take(11)
            .ToList().ForEach(e => builder.Append(e));
            uniqueId = builder.ToString();
            PlayerPrefs.SetString("uniqueID", uniqueId);
    }

    public static IEnumerator UploadLogs()
    {
        var imageFiles = Directory.GetFiles(Application.persistentDataPath + "/captured_images",
                "*.*", SearchOption.AllDirectories)
            .Where(file => new string[] { ".jpg" }
                .Contains(Path.GetExtension(file)))
            .ToList();

        var textData = File.ReadAllText(Application.persistentDataPath + "/evision_result_logs.txt");
        var textBytes = System.Text.Encoding.UTF8.GetBytes(textData);

        foreach (var image in imageFiles)
        {
            var imgData = File.ReadAllBytes(image);
            image.Replace(Application.persistentDataPath + "/captured_images", "");

            WWWForm form = new WWWForm();
            form.AddField("key", "m2mAAxfa3!");
            form.AddBinaryData("upfile[]", textBytes, $"{uniqueId}_logs.txt", "text/plain");
            form.AddBinaryData("upfile[]", imgData, $"{uniqueId}_{image}", "image/jpg");
            form.AddField("foldername", $"evision_{uniqueId}");

            var request = UnityWebRequest.Post("http://160.40.51.48/evision/freceiver.php", form);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log("error in sending data to evision server");
                UAP_AccessibilityManager.Say("Η αποστολή δεδομένων στον σέρβερ απέτυχε.");
            }
            else
            {
                textUploaded = true;
                File.Delete(image);
            }
            request.Dispose();
        }

        if (imageFiles.Count == 0)
        {
            Debug.Log("emty images directory");
            UAP_AccessibilityManager.Say("Δεν εντοπίστηκαν στοιχεία για αποστολή. Η αποστολή δεδομένων στον σέρβερ ολοκληρώθηκε.");
        }
        if (textUploaded)
        {
            Debug.Log("data uploaded to server");
            UAP_AccessibilityManager.Say("Η αποστολή δεδομένων στον σέρβερ ολοκληρώθηκε.");
        }
    }

    public static IEnumerator UploadLog(string pathText, string imgName)
    {
        var textData = File.ReadAllText(Application.persistentDataPath + "/evision_result_logs.txt");
        var textBytes = System.Text.Encoding.UTF8.GetBytes(textData);

        var imgData = File.ReadAllBytes(Application.persistentDataPath + "/captured_images/" + imgName);

        WWWForm form = new WWWForm();
        form.AddField("key", "m2mAAxfa3!");
        form.AddBinaryData("upfile[]", textBytes, $"{uniqueId}_logs.txt", "text/plain");
        form.AddBinaryData("upfile[]", imgData, $"{uniqueId}_{imgName}", "image/jpg");
        form.AddField("foldername", $"{uniqueId}");

        var request = UnityWebRequest.Post("http://160.40.51.48/evision/freceiver.php", form);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log("error in sending data to evision server");
        }
        else
        {
            textUploaded = true;
            Debug.Log("data uploaded to server");
        }

        request.Dispose();
    }

    public static IEnumerator UploadImages()
    {
        var imageFiles = Directory.GetFiles(Application.persistentDataPath + "/captured_images",
                "*.*", SearchOption.AllDirectories)
            .Where(file => new string[] { ".jpg" }
                .Contains(Path.GetExtension(file)))
            .ToList();

        WWWForm form = new WWWForm();
        form.AddField("key", "m2mAAxfa3!");
        //form.AddField("foldername", $"folder_{uniqueId}");
        foreach (var image in imageFiles)
        {
            var imgData = File.ReadAllBytes(image);
            image.Replace(Application.persistentDataPath + "/captured_images", "");
            form.AddBinaryData("upfile[]", imgData, $"{uniqueId}_{image}", "image/jpeg");
        }

        var request = UnityWebRequest.Post("http://160.40.51.48/evision/freceiver.php", form);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log("error in sending data to evision server");
        }
        else
        {
            textUploaded = true;
            Debug.Log("image data uploaded to server");
        }
        request.Dispose();
    }
}
