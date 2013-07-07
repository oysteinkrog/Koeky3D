using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shaders;
using Koeky3D.BufferHandling;

namespace FramebufferExample
{
    /// <summary>
    /// Provides a simple render technique to display a model with a texture
    /// </summary>
    class RenderTechnique : Technique
    {
        private int textureLocation;
        private int enableLightLocation;

        public override bool Initialise()
        {
            // Load the shaders from a file
            if (!base.CreateShaderFromFile("Data/Shaders/vertexShader.txt", "Data/Shaders/fragmentShader.txt", ""))
                return false;

            // Set shader attributes, this is where the data will go in the shader
            base.SetShaderAttribute((int)BufferAttribute.Vertex, "in_Vertex");
            base.SetShaderAttribute((int)BufferAttribute.TexCoord, "in_TexCoord");
            base.SetShaderAttribute((int)BufferAttribute.Normal, "in_Normal");

            // Finalize creation of the shader program
            if (!base.Finalize())
                return false;

            // Retrieve the location of the uniforms
            this.textureLocation = base.GetUniformLocation("texture");
            this.enableLightLocation = base.GetUniformLocation("lightOn");

            // Initialisation was succesful
            return true;
        }

        public override void Enable()
        {
            // Set the texture variable to 0
            // By default it should be zero, but just to be sure :P
            base.shader.SetVariable(this.textureLocation, 0);
        }

        public bool EnableLighting
        {
            set
            {
                base.shader.SetVariable(this.enableLightLocation, value);
            }
        }
    }
}
