using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Properties;
using Koeky3D.BufferHandling;
using OpenTK.Graphics;

namespace Koeky3D.Shaders
{
    /// <summary>
    /// Provides a simple default technique for rendering objects.
    /// This technique allows to render objects with vertex position, vertex normals and vertex texture coordinates.
    /// It has two render modes: using a texture to color the object or using a constant color to color the object.
    /// </summary>
    public class DefaultTechnique : Technique
    {
        private int useTextureLocation, drawColorLocation;

        /// <summary>
        /// Initialises this technique
        /// </summary>
        /// <returns>True if the technique succesfully initialised</returns>
        public override bool Initialise()
        {
            if (!base.CreateShaderFromSource(Resources.DefaultVertexShader, Resources.DefaultFragmentShader, ""))
                return false;

            // Set shader attributes
            base.SetShaderAttribute((int)BufferAttribute.Vertex, "in_Vertex");
            base.SetShaderAttribute((int)BufferAttribute.TexCoord, "in_TexCoord");
            base.SetShaderAttribute((int)BufferAttribute.Normal, "in_Normal");

            if (!base.Finalize())
                return false;

            this.useTextureLocation = base.GetUniformLocation("useTexture");
            this.drawColorLocation = base.GetUniformLocation("drawColor");

            return true;
        }
        /// <summary>
        /// Enables this technique
        /// </summary>
        public override void Enable()
        {
            
        }

        /// <summary>
        /// Sets the useTexture uniform of this technique. True means a texture has to be used in the fragment shader.
        /// </summary>
        public bool UseTexture
        {
            set
            {
                base.shader.SetVariable(this.useTextureLocation, value);
            }
        }
        /// <summary>
        /// Sets the drawColor uniform of this technique. The drawColor is used to color the object when useTexture = false
        /// </summary>
        public Color4 DrawColor
        {
            set
            {
                base.shader.SetVariable(this.drawColorLocation, value);
            }
        }
    }
}
