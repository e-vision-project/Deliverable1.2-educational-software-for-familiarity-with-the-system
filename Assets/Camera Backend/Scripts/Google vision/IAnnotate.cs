using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IAnnotate
{
    Task<string> PerformAnnotation(Texture2D snap);
    T GetAnnotationResults<T>() where T : class;
}
