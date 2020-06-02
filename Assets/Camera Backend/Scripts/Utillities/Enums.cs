﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EVISION.Camera.plugin
{
    public class Enums
    {
        public enum MasoutisCategories
        {
            trail = 0,
            shelf = 1,
            product = 2,
            other = 3
        }

        public enum PServiceCategories
        {
            document = 0,
            sign = 1,
            face = 2,
            obj = 3,
            face_doc = 4
        }

        public enum ScenarioCases
        {
            masoutis = 0,
            publicService = 1,
            tour = 2,
            facerec
        }
    }
}