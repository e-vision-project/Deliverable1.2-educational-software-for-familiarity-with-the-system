using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnityExample;
using UnityEngine;

public class ObjectDetectionInferenceTF : MonoBehaviour
{
    /// <summary>
    /// The texture.
    /// </summary>
    Texture2D texture;

    /// <summary>
    /// The bgr mat.
    /// </summary>
    Mat bgrMat;

    /// <summary>
    /// The net.
    /// </summary>
    Net net;

    /// <summary>
    /// The model filepath.
    /// </summary>
    public string model_filepath;

    /// <summary>
    /// The config filepath.
    /// </summary>
    public string config_filepath;

    string input_filepath;

    /// <summary>
    /// The classes filepath.
    /// </summary>
    public string classes_filepath;

    public int width;
    public int height;
    public float scaleFactor;
    List<string> classNames;
    List<string> outBlobNames;
    List<string> outBlobTypes;

    public float confThreshold;
    public float nmsThreshold;
    public string input;


    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            getFilePath_Coroutine = GetFilePath();
            StartCoroutine(getFilePath_Coroutine);
#else
        if (!string.IsNullOrEmpty(classes_filepath)) classes_filepath = Utils.getFilePath("dnn/" + classes_filepath);
        if (!string.IsNullOrEmpty(config_filepath)) config_filepath = Utils.getFilePath("dnn/" + config_filepath);
        if (!string.IsNullOrEmpty(model_filepath)) model_filepath = Utils.getFilePath("dnn/" + model_filepath);
        if (!string.IsNullOrEmpty(input)) input_filepath = Utils.getFilePath("dnn/" + input);

#endif
        Run();
    }

    public void Run()
    {
        //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
        Utils.setDebugMode(true);

        classNames = readClassNames(classes_filepath);
        if (classNames == null)
        {
            Debug.LogError(classes_filepath +
                           " is not loaded. Please see \"StreamingAssets/dnn/setup_dnn_module.pdf\". ");
        }


        //Mat img = new Mat(imageTex.height, imageTex.width, CvType.);
        //Utils.texture2DToMat(imageTex, img);

        Mat img = Imgcodecs.imread(input_filepath);
        if (img.empty())
        {
            Debug.LogError(input_filepath + " is not loaded. Please see \"StreamingAssets/dnn/setup_dnn_module.pdf\". ");
            img = new Mat(424, 640, CvType.CV_8UC3, new Scalar(0, 0, 0));
        }

        //Adust Quad.transform.localScale.
        gameObject.transform.localScale = new Vector3(img.width(), img.height(), 1);
        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        float imageWidth = img.width();
        float imageHeight = img.height();

        float widthScale = (float)Screen.width / imageWidth;
        float heightScale = (float)Screen.height / imageHeight;
        if (widthScale < heightScale)
        {
            Camera.main.orthographicSize = (imageWidth * (float)Screen.height / (float)Screen.width) / 2;
        }
        else
        {
            Camera.main.orthographicSize = imageHeight / 2;
        }

        Net net = null;

        if (string.IsNullOrEmpty(model_filepath) || string.IsNullOrEmpty(config_filepath))
        {
            Debug.LogError(model_filepath + " or " + config_filepath +
                           " is not loaded. Please see \"StreamingAssets/dnn/setup_dnn_module.pdf\". ");
        }
        else
        {
            net = Dnn.readNetFromTensorflow(model_filepath, config_filepath);
        }

        if (net == null)
        {
            Imgproc.putText(img, "model file is not loaded.", new Point(5, img.rows() - 30),
                Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255), 2, Imgproc.LINE_AA, false);
            Imgproc.putText(img, "Please read console message.", new Point(5, img.rows() - 10),
                Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255), 2, Imgproc.LINE_AA, false);
        }
        else
        {
            outBlobNames = getOutputsNames(net);
            outBlobTypes = getOutputsTypes(net);

            Mat blob = Dnn.blobFromImage(img, 0.007843, new Size(300,300), new Scalar(127.5, 127.5, 127.5));

            net.setInput(blob);

            TickMeter tm = new TickMeter();
            tm.start();


            List<Mat> outs = new List<Mat>();
            net.forward(outs, outBlobNames);


            tm.stop();
            Debug.Log("Inference time, ms: " + tm.getTimeMilli());


            postprocess(img, outs, net);

            for (int i = 0; i < outs.Count; i++)
            {
                outs[i].Dispose();
            }
            blob.Dispose();
            net.Dispose();


            Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);

            Texture2D texture = new Texture2D(img.cols(), img.rows(), TextureFormat.RGBA32, false);

            Utils.matToTexture2D(img, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;


            Utils.setDebugMode(false);
        }
    }

        /// <summary>
        /// Postprocess the specified frame, outs and net.
        /// </summary>
        /// <param name="frame">Frame.</param>
        /// <param name="outs">Outs.</param>
        /// <param name="net">Net.</param>
        private void postprocess(Mat frame, List<Mat> outs, Net net)
        {
            string outLayerType = outBlobTypes[0];


            List<int> classIdsList = new List<int>();
            List<float> confidencesList = new List<float>();
            List<OpenCVForUnity.CoreModule.Rect> boxesList = new List<OpenCVForUnity.CoreModule.Rect>();
            if (net.getLayer(new DictValue(0)).outputNameToIndex("im_info") != -1)
            {
                // Faster-RCNN or R-FCN
                // Network produces output blob with a shape 1x1xNx7 where N is a number of
                // detections and an every detection is a vector of values
                // [batchId, classId, confidence, left, top, right, bottom]

                if (outs.Count == 1)
                {

                    outs[0] = outs[0].reshape(1, (int)outs[0].total() / 7);

                    //Debug.Log ("outs[i].ToString() " + outs [0].ToString ());

                    float[] data = new float[7];

                    for (int i = 0; i < outs[0].rows(); i++)
                    {

                        outs[0].get(i, 0, data);

                        float confidence = data[2];

                        if (confidence > confThreshold)
                        {
                            int class_id = (int)(data[1]);

                            int left = (int)(data[3] * frame.cols());
                            int top = (int)(data[4] * frame.rows());
                            int right = (int)(data[5] * frame.cols());
                            int bottom = (int)(data[6] * frame.rows());
                            int width = right - left + 1;
                            int height = bottom - top + 1;

                            classIdsList.Add((int)(class_id) - 0);
                            confidencesList.Add((float)confidence);
                            boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));
                        }
                    }
                }
            }
            else if (outLayerType == "DetectionOutput")
            {
                // Network produces output blob with a shape 1x1xNx7 where N is a number of
                // detections and an every detection is a vector of values
                // [batchId, classId, confidence, left, top, right, bottom]

                if (outs.Count == 1)
                {
                    outs[0] = outs[0].reshape(1, (int)outs[0].total() / 7);

                    //Debug.Log ("outs[i].ToString() " + outs [0].ToString ());

                    float[] data = new float[7];

                    for (int i = 0; i < outs[0].rows(); i++)
                    {

                        outs[0].get(i, 0, data);

                        float confidence = data[2];

                        if (confidence > confThreshold)
                        {
                            int class_id = (int)(data[1]);

                            int left = (int)(data[3] * frame.cols());
                            int top = (int)(data[4] * frame.rows());
                            int right = (int)(data[5] * frame.cols());
                            int bottom = (int)(data[6] * frame.rows());
                            int width = right - left + 1;
                            int height = bottom - top + 1;

                            classIdsList.Add((int)(class_id) - 0);
                            confidencesList.Add((float)confidence);
                            boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));
                        }
                    }
                }
            }
            else if (outLayerType == "Region")
            {
                for (int i = 0; i < outs.Count; ++i)
                {
                    // Network produces output blob with a shape NxC where N is a number of
                    // detected objects and C is a number of classes + 4 where the first 4
                    // numbers are [center_x, center_y, width, height]

                    //Debug.Log ("outs[i].ToString() "+outs[i].ToString());

                    float[] positionData = new float[5];
                    float[] confidenceData = new float[outs[i].cols() - 5];

                    for (int p = 0; p < outs[i].rows(); p++)
                    {

                        outs[i].get(p, 0, positionData);

                        outs[i].get(p, 5, confidenceData);

                        int maxIdx = confidenceData.Select((val, idx) => new { V = val, I = idx }).Aggregate((max, working) => (max.V > working.V) ? max : working).I;
                        float confidence = confidenceData[maxIdx];

                        if (confidence > confThreshold)
                        {

                            int centerX = (int)(positionData[0] * frame.cols());
                            int centerY = (int)(positionData[1] * frame.rows());
                            int width = (int)(positionData[2] * frame.cols());
                            int height = (int)(positionData[3] * frame.rows());
                            int left = centerX - width / 2;
                            int top = centerY - height / 2;

                            classIdsList.Add(maxIdx);
                            confidencesList.Add((float)confidence);
                            boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));

                        }
                    }
                }
            }
            else
            {
                Debug.Log("Unknown output layer type: " + outLayerType);
            }


            MatOfRect boxes = new MatOfRect();
            boxes.fromList(boxesList);

            MatOfFloat confidences = new MatOfFloat();
            confidences.fromList(confidencesList);


            MatOfInt indices = new MatOfInt();
            Dnn.NMSBoxes(boxes, confidences, confThreshold, nmsThreshold, indices);

            //Debug.Log ("indices.dump () "+indices.dump ());
            //Debug.Log ("indices.ToString () "+indices.ToString());

            for (int i = 0; i < indices.total(); ++i)
            {
                int idx = (int)indices.get(i, 0)[0];
                Debug.Log(classNames[classIdsList[idx]] + confidencesList[idx].ToString());
                //modelOutput.Add(new KeyValuePair<string, float>(classNames[classIdsList[idx]], confidencesList[idx]));
                OpenCVForUnity.CoreModule.Rect box = boxesList[idx];
                drawPred(classIdsList[idx], confidencesList[idx], box.x, box.y, box.x + box.width, box.y + box.height, frame);
            }


            indices.Dispose();
            boxes.Dispose();
            confidences.Dispose();

        }


        private void drawPred(int classId, float conf, int left, int top, int right, int bottom, Mat frame)
        {
            Imgproc.rectangle(frame, new Point(left, top), new Point(right, bottom), new Scalar(0, 255, 0, 255), 2);

            string label = conf.ToString();
            if (classNames != null && classNames.Count != 0)
            {
                if (classId < (int)classNames.Count)
                {
                    label = classNames[classId] + ": " + label;
                }
            }

            int[] baseLine = new int[1];
            Size labelSize = Imgproc.getTextSize(label, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, 1, baseLine);

            top = Mathf.Max(top, (int)labelSize.height);
            Imgproc.rectangle(frame, new Point(left, top - labelSize.height),
                new Point(left + labelSize.width, top + baseLine[0]), Scalar.all(255), Core.FILLED);
            Imgproc.putText(frame, label, new Point(left, top), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(0, 0, 0, 255));
        }

    /// <summary>
    /// Reads the class names.
    /// </summary>
    /// <returns>The class names.</returns>
    /// <param name="filename">Filename.</param>
    private List<string> readClassNames(string filename)
        {
            List<string> classNames = new List<string>();

            System.IO.StreamReader cReader = null;
            try
            {
                cReader = new System.IO.StreamReader(filename, System.Text.Encoding.Default);

                while (cReader.Peek() >= 0)
                {
                    string name = cReader.ReadLine();
                    classNames.Add(name);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
            finally
            {
                if (cReader != null)
                    cReader.Close();
            }

            return classNames;
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

        /// <summary>
        /// Gets the outputs types.
        /// </summary>
        /// <returns>The outputs types.</returns>
        /// <param name="net">Net.</param>
        private List<string> getOutputsTypes(Net net)
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
    }
