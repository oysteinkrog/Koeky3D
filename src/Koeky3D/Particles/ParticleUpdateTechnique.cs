using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shaders;
using Koeky3D.Properties;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Koeky3D.BufferHandling;
using Koeky3D.Utilities;

namespace Koeky3D.Particles
{
    class ParticleUpdateTechnique : Technique
    {
        private int positionLocation;
        private int gravityLocation;
        private int deltaTimeLocation;
        private int timeLocation;
        private int randomTextureLocation;
        private int windLocation;
        private int scaleStartLocation;
        private int scaleDeltaLocation;
        private int maxLifeTimeLocation;
        private int minVelLocation;
        private int maxVelLocation;

        public override bool Initialise()
        {
            if (!base.CreateShaderFromSource(Resources.ParticleUpdateShader, "", ""))
                return false;

            // Set buffer attributes
            base.SetShaderAttribute((int)BufferAttribute.Vertex, "in_Vertex");
            base.SetShaderAttribute((int)BufferAttribute.ParticleVelocity, "in_Velocity");
            base.SetShaderAttribute((int)BufferAttribute.ParticleTextureAlpha, "in_TextureAlpha");
            base.SetShaderAttribute((int)BufferAttribute.ParticleScale, "in_Scale");
            base.SetShaderAttribute((int)BufferAttribute.ParticleLifeTime, "in_LifeTime");

            // Set transform feedback varyings
            String[] varyings = new String[]
            {
                "out_Vertex",
                "out_Velocity",
                "out_TextureAlpha",
                "out_Scale",
                "out_LifeTime",
            };
            base.SetTransformFeedbackVaryings(varyings, TransformFeedbackMode.InterleavedAttribs);

            if (!base.Finalize())
                return false;

            // Retrieve uniform locations
            this.positionLocation = base.GetUniformLocation("emitterPosition");
            this.gravityLocation = base.GetUniformLocation("gravity");
            this.windLocation = base.GetUniformLocation("wind");
            this.deltaTimeLocation = base.GetUniformLocation("deltaTime");
            this.timeLocation = base.GetUniformLocation("time");
            this.randomTextureLocation = base.GetUniformLocation("randomTexture");
            this.scaleStartLocation = base.GetUniformLocation("startScale");
            this.scaleDeltaLocation = base.GetUniformLocation("deltaScale");
            this.maxLifeTimeLocation = base.GetUniformLocation("maxLifeTime");
            this.minVelLocation = base.GetUniformLocation("minVelocity");
            this.maxVelLocation = base.GetUniformLocation("maxVelocity");

            return true;
        }

        public override void Enable()
        {
            base.shader.SetVariable(this.randomTextureLocation, 0);
        }

        public Vector3 Wind
        {
            set
            {
                base.shader.SetVariable(this.windLocation, value);
            }
        }
        public Vector3 EmitterPosition
        {
            set
            {
                base.shader.SetVariable(this.positionLocation, value);
            }
        }
        public Vector3 Gravity
        {
            set
            {
                base.shader.SetVariable(this.gravityLocation, value);
            }
        }
        public float DeltaTime
        {
            set
            {
                base.shader.SetVariable(this.deltaTimeLocation, value);
            }
        }
        public float Time
        {
            set
            {
                base.shader.SetVariable(this.timeLocation, value);
            }
        }
        public float ScaleStart
        {
            set
            {
                base.shader.SetVariable(this.scaleStartLocation, value);
            }
        }
        public float ScaleDelta
        {
            set
            {
                base.shader.SetVariable(this.scaleDeltaLocation, value);
            }
        }
        public float MaxLifeTime
        {
            set
            {
                base.shader.SetVariable(this.maxLifeTimeLocation, value);
            }
        }
        public Vector3 MinVelocity
        {
            set
            {
                base.shader.SetVariable(this.minVelLocation, value);
            }
        }
        public Vector3 MaxVelocity
        {
            set
            {
                base.shader.SetVariable(this.maxVelLocation, value);
            }
        }
    }
}
