using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MilkModelLoader
{
    /// <summary>
    /// A container class for a complete vertex. This class is used to filter redundant vertices in a milkshape model
    /// </summary>
    class TotalVertex
    {
        public short index = -1;

        public Vector3 vertex;
        public Vector2 texCoord;
        public Vector3 normal;

        public Vector4 boneIndices;
        public Vector4 boneWeights;

        public override bool Equals(object obj)
        {
            TotalVertex other = obj as TotalVertex;
            if (other == null)
                return false;

            if (other.vertex != this.vertex)
                return false;
            if (other.texCoord != this.texCoord)
                return false;
            if (other.normal != this.normal)
                return false;

            if (other.boneIndices != this.boneIndices)
                return false;
            if (other.boneWeights != this.boneWeights)
                return false;

            return true;
        }
    }
}
