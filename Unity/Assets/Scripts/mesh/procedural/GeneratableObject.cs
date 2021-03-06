﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrcode.mesh.procedural
{
    public interface GeneratableObject {
        void AlignAxis(Vector3 axis);
        void Resize(float dim);
        void Generate();
        void Reposition(Vector3 center);
    }
}