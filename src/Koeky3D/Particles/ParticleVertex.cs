using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Runtime.InteropServices;

namespace Koeky3D.Particles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)] 
    struct ParticleVertex
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 textureAlphas;
        public float scale;
        public float lifeTime;

        public ParticleVertex(Vector3 position, Vector3 velocity, Vector4 textureAlphas, float scale, float lifeTime)
        {
            this.position = position;
            this.velocity = velocity;
            this.textureAlphas = textureAlphas;
            this.scale = scale;
            this.lifeTime = lifeTime;
        }
    }
}
