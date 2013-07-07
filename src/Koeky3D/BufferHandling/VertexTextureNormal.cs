using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.BufferHandling
{
    public struct VertexTextureNormal
    {
        public Vector3 vertex;
        public Vector2 texture;
        public Vector3 normal;

        public VertexTextureNormal(Vector3 vertex, Vector2 texture, Vector3 normal)
        {
            this.vertex = vertex;
            this.texture = texture;
            this.normal = normal;
        }
    }
}
