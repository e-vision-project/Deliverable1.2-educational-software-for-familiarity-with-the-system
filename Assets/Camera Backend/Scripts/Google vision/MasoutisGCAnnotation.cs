using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.Vision;
using System.Linq;
using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace EVISION.Camera.plugin
{

    /// <summary>
    /// Make calls to the Cloud vision API for getting the OCR words.
    /// For optimization check :
    /// a) GenericUtils.ScaleTexture in Perform annotation, faster with slight loss in accuracy.
    /// b) Max results in AnnotateImage, set to 50 as default.
    /// </summary>


    public class MasoutisGCAnnotation : MonoBehaviour, IAnnotate
    {
        private GCVision _gcVision;
        private string textAnnotation = null;
        private bool annotationCompleted = false;
        private MasoutisManager masoutisClient;

        [Header("Rescale Image")]
        public bool RescaleInput = false;
        [Tooltip("The resolution to be scaled")]
        public Vector2 scaleResolution;
        public bool displayBoundingBox;

        // Private fields
        private float _width;
        private float _height;
        private List<Enumerators.FeatureType> productFeatureTypes = new List<Enumerators.FeatureType>();
        private List<Enumerators.FeatureType> shelfFeatureTypes = new List<Enumerators.FeatureType>();

        #region IAnnotate callbacks

        public async Task<string> PerformAnnotation(Texture2D snap)
        {
            var frameBreak = new WaitForEndOfFrame();

            if (RescaleInput)
            {
                snap = GenericUtils.Resize(snap, (int)scaleResolution.x, (int)scaleResolution.y);
            }

            _width = snap.width;
            _height = snap.height;

            // Convert to base64 encoding.
            string _selectedImageData = ImageConvert.Convert(snap);

            if (MasoutisManager.category == (int)Enums.MasoutisCategories.product)
            {
                AnnotateImage(_selectedImageData, productFeatureTypes);
            }
            else
            {
                AnnotateImage(_selectedImageData, shelfFeatureTypes);
            }
            while (annotationCompleted != true)
            {
                await frameBreak;
            }
            return textAnnotation;
        }

        public T GetAnnotationResults<T>() where T : class
        {
            return textAnnotation as T;
        }

        #endregion


        #region Cloud vision callbacks

        public void AnnotateImage(string imageData, List<Enumerators.FeatureType> featureTypes)
        {
            //flag showing if annotation process has ended.
            annotationCompleted = false;

            var features = new List<Feature>();

            foreach (var feat in featureTypes)
            {
                features.Add(new Feature() { maxResults = 50, type = feat.ToString() });
            }

            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = new FrostweepGames.Plugins.GoogleCloud.Vision.Image()
                    {
                        source = new ImageSource()
                        {
                            imageUri = string.Empty,
                            gcsImageUri = string.Empty
                        },
                        content = imageData
                    },
                    context = new ImageContext()
                    {
                        languageHints = new string[]
                        {
                            "english"
                        },
                    },
                    features = features
                }
            });
        }


        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            if (CameraClient.scenarioCase != Enums.ScenarioCases.masoutis)
            {
                return;
            }
            Debug.Log("e-vision platform logs: " + "Annotation failed. Check internet connection");
            annotationCompleted = true;
            textAnnotation = "GCFAILED";
            //Invoke onAnnotationFailed
            EventCamManager.onAnnotationFailed?.Invoke();
        }

        private void _gcVision_AnnotateSuccessEvent(VisionResponse arg1, long arg2)
        {
            if (CameraClient.scenarioCase != Enums.ScenarioCases.masoutis)
            {
                return;
            }
            try
            {
                if (MasoutisManager.category == (int)Enums.MasoutisCategories.product)
                {
                    // vertices from object localization max poly box.
                    var biggestBoxCoords = GetMaxBoxCoords(arg1);

                    //Display Bounding box
                    if(displayBoundingBox) { RenderAnnotationResults(biggestBoxCoords); }
                    
                    // OCR words that are contained inside the bounding box.
                    textAnnotation = GetTextAnnotation(arg1, biggestBoxCoords);
                }
                else
                {
                    textAnnotation = GetTextAnnotation(arg1);
                }

                annotationCompleted = true;
            }
            catch (System.Exception)
            {
                Debug.LogError("Annotation was successfull but no responses were found");
                textAnnotation = string.Empty;
                annotationCompleted = true;
            }
        }

        private void RenderAnnotationResults(List<Vertex> Coords)
        {
            //InternalTools.ProcessImage(Coords.ToArray(), ref temp_snap, UnityEngine.Color.green);
            
            //CameraClient.camTexture = temp_snap;
        }

        #endregion


        // Start is called before the first frame update
        void Start()
        {
            _gcVision = GCVision.Instance;
            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;
            masoutisClient = gameObject.GetComponent<MasoutisManager>();

            // set features types
            productFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.TEXT_DETECTION,
                Enumerators.FeatureType.OBJECT_LOCALIZATION
            };
            shelfFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.TEXT_DETECTION,
            };
        }

        /// <summary>
            /// This method returns the OCR text located inside the biggest bounding box 
            /// received from the object_localization FeatureType.
            /// </summary>
            /// <param name="arg1">The response from google cloud vision</param>
            /// <param name="biggestBoxCoords"></param>
            /// <returns>string type</returns>
            private string GetTextAnnotation(VisionResponse arg1, List<Vertex> biggestBoxCoords)
        {
            var _desc = GetEntityInMaxBox(arg1, biggestBoxCoords);
            textAnnotation = _desc;
            return textAnnotation;
        }

        /// <summary>
        /// This method returns the full OCR text from the text_detection FeautureType.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private string GetTextAnnotation(VisionResponse arg1)
        {
            textAnnotation = arg1.responses[0].fullTextAnnotation.text;
            return textAnnotation;
        }

        /// <summary>
        /// Returns the coordinates(4) of the biggest bounding box from object_localization FeatureType.
        /// </summary>
        /// <param name="arg1">The google cloud vision response</param>
        /// <returns>List of Vertex objects</returns>
        private List<Vertex> GetMaxBoxCoords(VisionResponse arg1)
        {
            List<Vertex> biggestBoxCoord = new List<Vertex>();
            foreach (var vert in FindBiggestBoundingBox(arg1).normalizedVertices)
            {
                Vertex _v = new Vertex
                {
                    x = vert.x * _width,
                    y = vert.y * _height
                };
                biggestBoxCoord.Add(_v);
            }

            return biggestBoxCoord;
        }

        /// <summary>
        /// This method return the biggest bounding box coordinates by calculating and returning the
        /// bounding box with the biggest area.
        /// </summary>
        /// <param name="arg1">The google cloud vision response</param>
        /// <returns>BoundingPoly object</returns>
        public BoundingPoly FindBiggestBoundingBox(VisionResponse arg1)
        {
            List<double> areas = new List<double>();
            List<LocalizedObjectAnnotation> entities = new List<LocalizedObjectAnnotation>();
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.localizedObjectAnnotations)
                {
                    var b = entity.boundingPoly.normalizedVertices[2].x - entity.boundingPoly.normalizedVertices[0].x;
                    var h = entity.boundingPoly.normalizedVertices[2].y - entity.boundingPoly.normalizedVertices[0].y;
                    var area = b * h;
                    areas.Add(area);
                    entities.Add(entity);
                }
            }

            var max_Area = areas.Max();
            var maxEntity = entities[areas.IndexOf(max_Area)];
            return maxEntity.boundingPoly;
        }

        /// <summary>
        /// This method returns the text of every OCR entity inside the area of the given vertices. 
        /// </summary>
        /// <param name="arg1">The google cloud vision response</param>
        /// <param name="verticesObj">List of vertices</param>
        /// <returns>type string</returns>
        public string GetEntityInMaxBox(VisionResponse arg1, List<Vertex> verticesObj)
        {
            List<EntityAnnotation> entities = new List<EntityAnnotation>();
            string _description = "";
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.textAnnotations)
                {
                    //compare with bottom left (x,y) and top right
                    if (entity.boundingPoly.vertices[0].x > verticesObj[0].x && entity.boundingPoly.vertices[2].x < verticesObj[2].x
                        && entity.boundingPoly.vertices[0].y > verticesObj[0].y && entity.boundingPoly.vertices[2].y < verticesObj[2].y)
                    {
                        entities.Add(entity);

                        //Display Bounding box
                        //if (displayBoundingBox)
                        //{
                        //    InternalTools.ProcessImage(entity.boundingPoly.vertices, ref temp_image, UnityEngine.Color.red);
                        //    var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
                        //    //display_img.texture = temp_image;
                        //}
                    }
                }
            }

            foreach (var item in entities)
            {
                _description += item.description + ' ';
            }
            //_description = String.Concat(entities.Select(o => o.description));
            entities = null;
            return _description;
        }

        private void RescaleTexture(Texture2D snap)
        {
            GenericUtils.ScaleTextureBilinear(snap, (int)scaleResolution.x, (int)scaleResolution.y);
            Debug.Log(snap.width + " ," + snap.height);
        }
    }
}
