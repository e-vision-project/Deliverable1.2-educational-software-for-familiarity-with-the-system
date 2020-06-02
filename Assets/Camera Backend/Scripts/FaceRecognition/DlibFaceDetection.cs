using System;
using System.Collections.Generic;
using System.Linq;
using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetector.UnityUtils;
using JetBrains.Annotations;
using UnityEngine;
using Rect = UnityEngine.Rect;

public class DlibFaceDetection : MonoBehaviour
{

    string dlibShapePredictorFileName = "sp_human_face_68.dat";
    string dlibShapePredictorMobileFileName = "sp_human_face_68_for_mobile.dat";
    
    string dlibShapePredictorFilePath;

    private FaceLandmarkDetector faceLandmarkDetector;
    // Start is called before the first frame update
    void Awake()
    {
        dlibShapePredictorFilePath = Utils.getFilePath(dlibShapePredictorMobileFileName);
        faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);
    }

    public List<Rect> DetectFaces(Texture2D imgTexture)
    {

        if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
        {
            Debug.LogError(
                "shape predictor file does not exist. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
        }

        faceLandmarkDetector.SetImage(imgTexture);

        //detect face rects
        List<Rect> faceRects = faceLandmarkDetector.Detect();

        return faceRects;
    }

    public Rect GetBiggestBoundingBox(List<Rect> faceRects)
    {
        if (faceRects.Count == 0)
        {
            return new Rect(0,0,0,0);
        }

        var maxArea = (from rect in faceRects select (rect.width * rect.height)).Max();
        Rect maxRect = (from rect in faceRects where (rect.width * rect.height == maxArea) select rect).FirstOrDefault();

        return maxRect;
    }

    public List<Vector2> GetFaceLandmarks(Rect rect)
    {
        var points = faceLandmarkDetector.DetectLandmark(rect);
        return points;
    }

    public void AllignFace(List<Vector2> points)
    {
        throw new NotImplementedException();
    }

    public void DisposeDetector()
    {
        faceLandmarkDetector.Dispose();
    }
}
