using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.Vision;
using System.Linq;
using System.Threading.Tasks;
using FrostweepGames.Plugins.GoogleCloud.Vision.Helpers;

namespace EVISION.Camera.plugin
{

    /// <summary>
    /// Make calls to the Cloud vision API for getting the OCR words.
    /// For optimization check :
    /// a) GenericUtils.ScaleTexture in Perform annotation, faster with slight loss in accuracy.
    /// b) Max results in AnnotateImage, set to 50 as default.
    /// </summary>


    public class PublicServiceGCAnnotation : MonoBehaviour, IAnnotate
    {
        private GCVision _gcVision;
        private string textAnnotation;
        private bool annotationCompleted = false;
        private PublicServiceManager publicServiceManager;

        [Header("Rescale Image")]
        public bool RescaleInput = false;
        [Tooltip("The resolution to be scaled")]
        public Vector2 scaleResolution;
        public bool displayBoundingBox;

        // Private fields
        private float _width;
        private float _height;
        private List<Enumerators.FeatureType> faceFeatureTypes = new List<Enumerators.FeatureType>();
        private List<Enumerators.FeatureType> docFeatureTypes = new List<Enumerators.FeatureType>();
        private List<Enumerators.FeatureType> objectFeatureTypes = new List<Enumerators.FeatureType>();
        private List<Enumerators.FeatureType> fullFeatureTypes = new List<Enumerators.FeatureType>();

        #region IAnnotate callbacks

        public async Task<string> PerformAnnotation(Texture2D snap)
        {
            textAnnotation = string.Empty;

            if (RescaleInput)
            {
                snap = GenericUtils.Resize(snap, (int)scaleResolution.x, (int)scaleResolution.y);
            }

            _width = snap.width;
            _height = snap.height;

            // Convert to base64 encoding.
            string _selectedImageData = ImageConvert.Convert(snap);
            AnnotateImage(_selectedImageData, fullFeatureTypes);

            while (annotationCompleted != true)
            {
                await new WaitForEndOfFrame();
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

            var img = new FrostweepGames.Plugins.GoogleCloud.Vision.Image();
            if (string.IsNullOrEmpty(imageData))
            {
                img.source = new ImageSource()
                {
                    imageUri = string.Empty,
                    gcsImageUri = string.Empty,
                };

                img.content = string.Empty;
            }
            else
            {
                img.source = new ImageSource()
                {
                    imageUri = string.Empty,
                    gcsImageUri = string.Empty,
                };

                img.content = imageData;
            }

            _gcVision.Annotate(new List<AnnotateRequest>()
            {
                new AnnotateRequest()
                {
                    image = img,
                    context = new ImageContext()
                    {
                        cropHintsParams = new CropHintsParams()
                        {
                            aspectRatios = new double[] { 1, 2 }
                        },
                        languageHints = new string[]
                        {
                            "english"
                        },
                        latLongRect = new LatLongRect()
                        {
                            maxLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            },
                            minLatLng = new LatLng()
                            {
                                latitude = 0,
                                longitude = 0
                            }
                        }
                    },
                    features = features
                }
            });
        }


        private void _gcVision_AnnotateFailedEvent(string arg1, long arg2)
        {
            if (CameraClient.scenarioCase != Enums.ScenarioCases.publicService)
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
            if (CameraClient.scenarioCase != Enums.ScenarioCases.publicService)
            {
                return;
            }

            foreach (var response in arg1.responses)
            {
                if (response.faceAnnotations != null && response.textAnnotations != null)
                {
                    PublicServiceManager.category = 4;
                }
                else if (response.faceAnnotations != null)
                {
                    PublicServiceManager.category = 2;
                }
                else if (response.textAnnotations != null)
                {
                    PublicServiceManager.category = 0;
                }
                else if (response.localizedObjectAnnotations != null)
                {
                    PublicServiceManager.category = 3;
                    break;
                }
            }
            try
            {
                switch (PublicServiceManager.category)
                {
                    case (int)Enums.PServiceCategories.document:
                        GetDocumentAnnotation(arg1);
                        break;
                    case (int)Enums.PServiceCategories.sign:
                        GetTextAnnotation(arg1);
                        break;
                    case (int)Enums.PServiceCategories.face:
                        GetFaceAnnotation(arg1);
                        break;
                    case (int)Enums.PServiceCategories.obj:
                        GetProductDescription(arg1);
                        break;
                    case (int)Enums.PServiceCategories.face_doc:
                        GetFaceDocDescription(arg1);
                        break;
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Annotation was successfull but no responses were found");
                textAnnotation = string.Empty;
                annotationCompleted = true;
            }
            annotationCompleted = true;
            PublicServiceManager.annotationDone = true;
        }

        private void RenderAnnotationResults(List<Vertex> Coords)
        {
            //InternalTools.ProcessImage(Coords.ToArray(), ref temp_image, UnityEngine.Color.green);
            //var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
        }

        #endregion


        // Start is called before the first frame update
        void Start()
        {
            _gcVision = GCVision.Instance;
            _gcVision.AnnotateSuccessEvent += _gcVision_AnnotateSuccessEvent;
            _gcVision.AnnotateFailedEvent += _gcVision_AnnotateFailedEvent;
            publicServiceManager = gameObject.GetComponent<PublicServiceManager>();

            // set features types
            faceFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.FACE_DETECTION
            };
            docFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.DOCUMENT_TEXT_DETECTION
            };
            objectFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.OBJECT_LOCALIZATION
            };
            fullFeatureTypes = new List<Enumerators.FeatureType>()
            {
                Enumerators.FeatureType.FACE_DETECTION,
                Enumerators.FeatureType.OBJECT_LOCALIZATION,
                Enumerators.FeatureType.DOCUMENT_TEXT_DETECTION
            };
        }

        /// <summary>
        /// This method returns the OCR text located inside the biggest bounding box 
        /// received from the object_localization FeatureType.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <param name="biggestBoxCoords"></param>
        /// <returns>string type</returns>
        private void GetTextAnnotation(VisionResponse arg1, List<Vertex> biggestBoxCoords)
        {
            var _desc = GetEntityInMaxBox(arg1, biggestBoxCoords);
            textAnnotation = _desc;
        }

        /// <summary>
        /// This method returns the full OCR text from the text_detection FeautureType.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private void GetTextAnnotation(VisionResponse arg1)
        {
            textAnnotation = arg1.responses[0].fullTextAnnotation.text;
        }

        private void GetProductDescription(VisionResponse arg1)
        {
            textAnnotation = arg1.responses[0].localizedObjectAnnotations[0].name;
        }

        /// <summary>
        /// This method returns the full OCR text from the text_detection FeautureType
        /// of every ocr entity separetly.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private void GetDocumentAnnotation(VisionResponse arg1)
        {
            var blocks = arg1.responses[0].fullTextAnnotation.pages[0].blocks;
            foreach (var block in blocks)
            {
                var words = block.paragraphs[0].words;
                foreach (var word in words)
                {
                    foreach (var symbol in word.symbols)
                    {
                        textAnnotation = $"{textAnnotation}{symbol.text}";
                    }

                    textAnnotation = $"{textAnnotation} ";
                }

                textAnnotation = $"{textAnnotation}<end_block>";
            }
        }

        /// <summary>
        /// This method returns the face emotions from the face_detection FeautureType
        /// of every facial detected entity separetly.
        /// </summary>
        /// <param name="arg1">The response from google cloud vision</param>
        /// <returns>string type</returns>
        private void GetFaceAnnotation(VisionResponse arg1)
        {
            foreach(var face in arg1.responses[0].faceAnnotations)
            {
                if (face.joyLikelihood == Enumerators.Likelihood.LIKELY || face.joyLikelihood == Enumerators.Likelihood.VERY_LIKELY
                    || face.joyLikelihood == Enumerators.Likelihood.POSSIBLE)
                {
                    textAnnotation = $"{textAnnotation} χαράς";
                    textAnnotation = $"{textAnnotation} ,";
                }
                else if(face.sorrowLikelihood == Enumerators.Likelihood.LIKELY || face.sorrowLikelihood == Enumerators.Likelihood.VERY_LIKELY
                    || face.sorrowLikelihood == Enumerators.Likelihood.POSSIBLE)
                {
                    textAnnotation = $"{textAnnotation} λύπης";
                    textAnnotation = $"{textAnnotation} ,";
                }
                else if (face.angerLikelihood == Enumerators.Likelihood.LIKELY || face.angerLikelihood == Enumerators.Likelihood.VERY_LIKELY
                    || face.angerLikelihood == Enumerators.Likelihood.POSSIBLE)
                {
                    textAnnotation = $"{textAnnotation} θυμού";
                    textAnnotation = $"{textAnnotation} ,";
                }
                else if (face.surpriseLikelihood == Enumerators.Likelihood.LIKELY || face.surpriseLikelihood == Enumerators.Likelihood.VERY_LIKELY
                    || face.surpriseLikelihood == Enumerators.Likelihood.POSSIBLE)
                {
                    textAnnotation = $"{textAnnotation} έκπληξης";
                    textAnnotation = $"{textAnnotation} ,";
                }
                else
                {
                    textAnnotation = $"{textAnnotation} ουδέτερα";
                    textAnnotation = $"{textAnnotation},";
                }
            }
        }

        private void GetFaceDocDescription(VisionResponse arg1)
        {
            GetFaceAnnotation(arg1);
            textAnnotation = $"{textAnnotation}<end_block>";
            GetDocumentAnnotation(arg1);
        }

        /// <summary>
        /// Επιστρέφει τις λέξεις / προτάσεις με τα μεγαλύτερα N bounding boxes στην εικόνα. 
        /// Σε αντίθεση με το FindBiggestBoundingBox όπου βρίσκουμε το μεγαλύτερο bounding box του αντικειμένου, 
        /// εδώ εντοπίζουμε τα μεγαλύτερα bounding boxes των αναγνωρισμένων λέξεων στην εικόνα.
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public List<EntityAnnotation> FindNBiggestBoundingBoxes(VisionResponse arg1)
        {
            List<double> areas = new List<double>();
            List<EntityAnnotation> entities = new List<EntityAnnotation>();
            foreach (var response in arg1.responses)
            {
                foreach (var entity in response.textAnnotations)
                {
                    var b = entity.boundingPoly.vertices[2].x - entity.boundingPoly.vertices[0].x;
                    var h = entity.boundingPoly.vertices[2].y - entity.boundingPoly.vertices[0].y;
                    var area = b * h;
                    areas.Add(area);
                    entities.Add(entity);
                }
            }

            var textEntities = new List<EntityAnnotation>();
            var max_Areas = areas.OrderByDescending(x => x).Take(10).ToList();
            for (int i = 0; i < max_Areas.Count; i++)
            {
                var entity = entities[areas.IndexOf(max_Areas[i])];
                i++;
                textEntities.Add(entity);
            }
            return textEntities;
        }


        /*
        Returns the text of every OCR entity separated with space, inside the area of the box. 
        */
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
                        if (displayBoundingBox)
                        {
                            //    InternalTools.ProcessImage(entity.boundingPoly.vertices, ref temp_image, UnityEngine.Color.red);
                            //    var display_img = GameObject.FindGameObjectWithTag("DISPLAY_IMAGE").GetComponent<RawImage>();
                        }
                    }
                }
            }

            foreach (var item in entities)
            {
                _description += item.description + ' ';
            }

            return _description;
        }

        private void RescaleTexture(Texture2D snap)
        {
            GenericUtils.ScaleTextureBilinear(snap, (int)scaleResolution.x, (int)scaleResolution.y);
        }
    }
}
