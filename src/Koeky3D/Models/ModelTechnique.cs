using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shaders;
using Koeky3D.BufferHandling;
using Koeky3D.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Koeky3D.Utilities;

namespace Koeky3D.Models
{
    /// <summary>
    /// Provides a default technique for rendering models.
    /// The technique does not provide a shader implementation, this has to be provided by the user.
    /// </summary>
    public class ModelTechnique : Technique
    {
        #region variables
        private int hasDiffuseMapLocation;
        private int hasNormalMapLocation;
        private int hasSpecularMapLocation;

        private int diffuseMapLocation;
        private int normalMapLocation;
        private int specularMapLocation;

        private int diffuseColorLocation;
        private int specularColorLocation;

        private int hasSkeletonLocation;
        private int jointsLocation;
        private int invJointsLocation;

        private String vertexShader, fragmentShader, geometryShader;
        private bool shaderAreAsPaths;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new model technique
        /// </summary>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        /// <param name="geometryShader"></param>
        /// <param name="shaderAreAsPaths">True if the previous variables are paths. Otherwise it is interpreted as code.</param>
        public ModelTechnique(String vertexShader, String fragmentShader, String geometryShader, bool shaderAreAsPaths)
        {
            this.vertexShader = vertexShader;
            this.fragmentShader = fragmentShader;
            this.geometryShader = geometryShader;
            this.shaderAreAsPaths = shaderAreAsPaths;
        }
        #endregion

        #region Technique methods
        /// <summary>
        /// Initialise this technique
        /// </summary>
        /// <returns></returns>
        public override bool Initialise()
        {
            if (this.shaderAreAsPaths)
            {
                if (!base.CreateShaderFromFile(this.vertexShader, this.fragmentShader, this.geometryShader))
                    return false;
            }
            else
            {
                if (!base.CreateShaderFromSource(this.vertexShader, this.fragmentShader, this.geometryShader))
                    return false;
            }

            // Set buffer attributes
            base.SetShaderAttribute((int)BufferAttribute.Vertex, "in_Vertex");
            base.SetShaderAttribute((int)BufferAttribute.TexCoord, "in_TexCoord");
            base.SetShaderAttribute((int)BufferAttribute.Normal, "in_Normal");
            base.SetShaderAttribute((int)BufferAttribute.BoneIndex, "in_BoneIndex");
            base.SetShaderAttribute((int)BufferAttribute.BoneWeight, "in_BoneWeight");

            if (!base.Finalize())
                return false;

            // Retrieve uniform location
            this.hasDiffuseMapLocation = base.GetUniformLocation(GLManager.SHADER_HASDIFFUSEMAP);
            this.hasSpecularMapLocation = base.GetUniformLocation(GLManager.SHADER_HASSPECULARMAP);
            this.hasNormalMapLocation = base.GetUniformLocation(GLManager.SHADER_HASNORMALMAP);

            this.diffuseMapLocation = base.GetUniformLocation(GLManager.SHADER_DIFFUSEMAP);
            this.specularMapLocation = base.GetUniformLocation(GLManager.SHADER_SPECULARMAP);
            this.normalMapLocation = base.GetUniformLocation(GLManager.SHADER_NORMALMAP);

            this.diffuseColorLocation = base.GetUniformLocation(GLManager.SHADER_DIFFUSECOLOR);
            this.specularColorLocation = base.GetUniformLocation(GLManager.SHADER_SPECULARCOLOR);

            this.hasSkeletonLocation = base.GetUniformLocation(GLManager.SHADER_HASSKELETON);
            this.jointsLocation = base.GetUniformLocation(GLManager.SHADER_JOINTS);
            this.invJointsLocation = base.GetUniformLocation(GLManager.SHADER_INVJOINTS);

            if (!base.Validate())
                return false;

            return true;
        }

        /// <summary>
        /// Enable this technique
        /// </summary>
        public override void Enable()
        {
            base.shader.SetVariable(this.diffuseMapLocation, 0);
            base.shader.SetVariable(this.normalMapLocation, 1);
            base.shader.SetVariable(this.specularMapLocation, 2);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Sets the hasDiffuseMap uniform
        /// </summary>
        public bool HasDiffuseMap
        {
            set
            {
                base.shader.SetVariable(this.hasDiffuseMapLocation, value);
            }
        }
        /// <summary>
        /// Sets the hasSpecularMap uniform
        /// </summary>
        public bool HasSpecularMap
        {
            set
            {
                base.shader.SetVariable(this.hasSpecularMapLocation, value);
            }
        }
        /// <summary>
        /// Sets the hasNormalMap uniform
        /// </summary>
        public bool HasNormalMap
        {
            set
            {
                base.shader.SetVariable(this.hasNormalMapLocation, value);
            }
        }
        /// <summary>
        /// Sets the hasSkeleton uniform
        /// </summary>
        public bool HasSkeleton
        {
            set
            {
                base.shader.SetVariable(this.hasSkeletonLocation, value);
            }
        }

        /// <summary>
        /// Sets the diffuseColor uniform
        /// </summary>
        public Vector4 DiffuseColor
        {
            set
            {
                base.shader.SetVariable(this.diffuseColorLocation, value);
            }
        }
        /// <summary>
        /// Sets the specularColor uniform
        /// </summary>
        public Vector4 SpecularColor
        {
            set
            {
                base.shader.SetVariable(this.specularColorLocation, value);
            }
        }

        /// <summary>
        /// Sets the joints uniform
        /// </summary>
        public Matrix4[] Joints
        {
            set
            {
                base.shader.SetVariable(this.jointsLocation, value.Length, value);
            }
        }
        /// <summary>
        /// Sets the invJoints uniform
        /// </summary>
        public Matrix4[] InvJoints
        {
            set
            {
                base.shader.SetVariable(this.invJointsLocation, value.Length, value);
            }
        }
        #endregion
    }
}
