using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OpenCVForUnity.CoreModule;
using UnityEngine;


namespace FaceRecognition
{
    [Serializable]
    public class FaceEmbeddings
    {
        [SerializeField] public double[] Embeddings { get; }

        public FaceEmbeddings(Mat mat, int size)
        {
            this.Embeddings = new double[size];
            SetEmbeddingsFromMatVGG2(mat);
        }

        public void SetEmbeddingsFromMat(Mat mat)
        {
            for (int i = 0; i < mat.cols(); i++)
            {
                Embeddings[i] = mat.get(0, i)[0];
            }
        }

        public void SetEmbeddingsFromMatVGG2(Mat mat)
        {
            var reshaped = mat.reshape(1, 1);
            for (int i = 0; i < reshaped.rows(); i++)
            {
                for (int j = 0; j < reshaped.cols(); j++)
                {
                    Embeddings[j] = reshaped.get(i, j)[0];
                }
            }
        }

        public Mat CreateMatFromEmbeddings(double[] input)
        {
            var mat = new Mat(1, input.Length, CvType.CV_32FC1);
            if (input == null)
            {
                return null;
            }
            mat.put(0, 0, input);

            return mat;
        }

        public void SerializeEmbeddings(string fileName, double[] embeddings)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream($"{fileName}.emb", FileMode.Create);

            formatter.Serialize(stream, embeddings);
            stream.Close();
        }

        public double[] DeSerializeEmbeddings(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            if (File.Exists(path))
            {
                var embeddings = formatter.Deserialize(stream) as double[];
                stream.Close();
                return embeddings;
            }

            Debug.LogError("File does not exists, deserialization failed.");
            return null;
        }
    }
}
