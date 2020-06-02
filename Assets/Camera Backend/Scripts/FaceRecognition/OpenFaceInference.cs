using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using EVISION.Camera.plugin;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace FaceRecognition
{
    public class OpenFaceInference : MonoBehaviour
    {
        public string input_1;
        public string input_2;

        [TooltipAttribute(
            "Path to a binary file of model contains trained weights. It could be a file with extensions .caffemodel (Caffe), .pb (TensorFlow), .t7 or .net (Torch), .weights (Darknet).")]
        public string model;

        [TooltipAttribute("Path to input image.")]
        public string input;

        [TooltipAttribute("Preprocess input image by resizing to a specific width.")]
        public int inpWidth;

        [TooltipAttribute("Preprocess input image by resizing to a specific height.")]
        public int inpHeight;

        private string model_filepath;
        private string input_filepath;
        public ObjectDetectionInference _faceDetector;
        public DlibFaceDetection _DlibDetector;
        public float scalefactor;
        public bool swapRB;
        [SerializeField] private bool displayBB;

        // Start is called before the first frame update
        void Start()
        {
            _faceDetector = gameObject.GetComponent<ObjectDetectionInference>();
            _DlibDetector = gameObject.GetComponent<DlibFaceDetection>();
            if (!string.IsNullOrEmpty(model)) model_filepath = Utils.getFilePath("dnn/" + model);
            if (!string.IsNullOrEmpty(input)) input_filepath = Utils.getFilePath("dnn/" + input);

            //var rep1 = GetRep(input_1);
            //var rep2 = GetRep(input_2);

            //double result = Core.norm(rep1, rep2, 5);
            //Debug.Log(result);

            //// squared L2 distance.
            //var diff = mat_1 - mat_2;
            //var result = diff.dot(diff);
        }

        public Mat GetRep(string path)
        {
            var tex = CVUtils.LoadImgToTexture(path, TextureFormat.RGBA32);
            var rep = ExtractFaceEmbeddings(tex, GetBBs(tex));
            Mat mat = rep.CreateMatFromEmbeddings(rep.Embeddings);
            return mat;
        }

        public List<Rect> GetDetectionBoxes(Texture2D img_Texture)
        {
            _faceDetector.RunInference(img_Texture);
            return _faceDetector.GetDetectionBoxes();
        }

        public List<Rect> GetDetectionBoxes(Mat img)
        {
            _faceDetector.RunInference(img);
            return _faceDetector.GetDetectionBoxes();
        }

        public UnityEngine.Rect GetBBs(Texture2D img)
        {
            var rects = _DlibDetector.DetectFaces(img);
            return _DlibDetector.GetBiggestBoundingBox(rects);
        }

        public UnityEngine.Rect GetBB(Mat img)
        {
            var img_Texture = new Texture2D(img.width(), img.height(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(img, img_Texture);
            var rects = _DlibDetector.DetectFaces(img_Texture);
            return _DlibDetector.GetBiggestBoundingBox(rects);
        }

        public FaceEmbeddings ExtractFaceEmbeddings(Texture2D imageTex, UnityEngine.Rect ROI)
        {
            //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
            Utils.setDebugMode(true);

            var embedder = Dnn.readNetFromTorch(model_filepath);

            Mat img = new Mat(imageTex.height, imageTex.width, CvType.CV_8UC3);
            Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);
            Utils.texture2DToMat(imageTex, img);
            
            if (img.empty())
            {
                Debug.LogError(input_filepath + " is not loaded. Please see \"StreamingAssets/dnn/setup_dnn_module.pdf\". ");
                img = new Mat(424, 640, CvType.CV_8UC3, new Scalar(0, 0, 0));
            }

            Mat cropped_face = img.submat((int)ROI.y, (int)ROI.y + (int)ROI.height, (int)ROI.x, (int)ROI.width + (int)ROI.x);
            Imgproc.cvtColor(cropped_face, cropped_face, Imgproc.COLOR_BGR2RGB);
            var faceBlob = Dnn.blobFromImage(cropped_face, scalefactor, new Size(inpWidth, inpHeight), new Scalar(0, 0, 0), true, false);
            embedder.setInput(faceBlob);

            var netOut = embedder.forward();

            var embeddings = new FaceEmbeddings(netOut, 128);

            if (gameObject.GetComponent<Renderer>() != null && displayBB)
            {
                GenericUtils.AdjustImageScale(cropped_face, this.gameObject);
                Texture2D texture = new Texture2D(cropped_face.cols(), cropped_face.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(cropped_face, texture);
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }

            embedder.Dispose();
            cropped_face.Dispose();
            img.Dispose();
            netOut.Dispose();

            return embeddings;
        }

        public Mat Extract(string path)
        {
            //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
            Utils.setDebugMode(true);

            var embedder = Dnn.readNetFromTorch(model_filepath);

            Mat img = Imgcodecs.imread(Utils.getFilePath("faces/" + path));
            if (img.empty())
            {
                Debug.LogError("image is not loaded");
                return img;
            }

            Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);
            var roi = GetBB(img);
            Mat cropped_face = img.submat((int)roi.y, (int)roi.y + (int)roi.height, 
                (int)roi.x,(int)roi.width + (int)roi.x);
            var faceBlob = Dnn.blobFromImage(cropped_face, scalefactor, new Size(inpWidth, inpHeight), new Scalar(0, 0, 0), true, false);
            embedder.setInput(faceBlob);
            var netOut = embedder.forward();

            if (gameObject.GetComponent<Renderer>() != null && displayBB)
            {
                GenericUtils.AdjustImageScale(cropped_face, this.gameObject);
                Texture2D texture = new Texture2D(cropped_face.cols(), cropped_face.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(cropped_face, texture);
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }

            //_embedder.Dispose();
            //cropped_face.Dispose();
            img.Dispose();

            return netOut;
        }

        /// <summary>
        /// Gets the outputs names.
        /// </summary>
        /// <returns>The outputs names.</returns>
        /// <param name="net">Net.</param>
        private List<string> getOutputsNames(Net net)
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
    }
}
