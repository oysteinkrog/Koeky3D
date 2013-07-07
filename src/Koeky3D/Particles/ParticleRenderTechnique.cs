using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shaders;
using Koeky3D.Properties;
using Koeky3D.BufferHandling;
using Koeky3D.Utilities;

namespace Koeky3D.Particles
{
    class ParticleRenderTechnique : Technique
    {
        private int textureLocation;
        private int textureCountLocation;
        private int maxLifeTimeLocation;

        public override bool Initialise()
        {
            if (!base.CreateShaderFromSource(Resources.ParticleVertexShader, Resources.ParticleFragmentShader, Resources.ParticleGeometryShader))
                return false;

            // Set buffer attributes
            base.SetShaderAttribute((int)BufferAttribute.Vertex, "in_Vertex");
            base.SetShaderAttribute((int)BufferAttribute.ParticleTextureAlpha, "in_TextureAlpha");
            base.SetShaderAttribute((int)BufferAttribute.ParticleScale, "in_Scale");
            base.SetShaderAttribute((int)BufferAttribute.ParticleLifeTime, "in_LifeTime");

            if (!base.Finalize())
                return false;

            // retrieve uniforms
            this.textureLocation = base.GetUniformLocation("particleTexture");
            this.textureCountLocation = base.GetUniformLocation("textureCount");
            this.maxLifeTimeLocation = base.GetUniformLocation("maxLifeTime");

            return true;
        }

        public override void Enable()
        {
            base.shader.SetVariable(this.textureLocation, 0);
        }

        public int TextureCount
        {
            set
            {
                base.shader.SetVariable(this.textureCountLocation, value);
            }
        }
        public float MaxLifeTime
        {
            set
            {
                base.shader.SetVariable(this.maxLifeTimeLocation, value);
            }
        }
    }
}
