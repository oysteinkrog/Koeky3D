using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MilkModelLoader.MilkshapeData
{
    class MilkshapeTriangle
    {
        public MilkshapeVertex vertex1, vertex2, vertex3;
        public Vector2 texCoord1, texCoord2, texCoord3;
        public Vector3 normal1, normal2, normal3;
    }
}
