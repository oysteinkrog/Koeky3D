using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.Textures;

namespace Koeky3D.Models
{
    /// <summary>
    /// Provides a container class for material properties.
    /// </summary>
    public class Material
    {
        /// <summary>
        /// The diffuse color
        /// </summary>
        public Vector4 diffuseColor;
        /// <summary>
        /// The specular color
        /// </summary>
        public Vector4 specularColor;
        /// <summary>
        /// The shininess
        /// </summary>
        public float shininess;
        /// <summary>
        /// The transparancy
        /// </summary>
        public float transparancy = 1.0f;

        /// <summary>
        /// The normal map or null if no normal map
        /// </summary>
        public Texture2D normalMap;
        /// <summary>
        /// The specular map or null if no normal map
        /// </summary>
        public Texture2D specularMap;
        /// <summary>
        /// The diffuse map or null if no diffuse map
        /// </summary>
        public Texture2D diffuseMap;

        /// <summary>
        /// True if this material has a normal map
        /// </summary>
        public bool hasNormalMap;
        /// <summary>
        /// True if this material has a specular map
        /// </summary>
        public bool hasSpecularMap;
        /// <summary>
        /// True if this material has a diffuse map
        /// </summary>
        public bool hasDiffuseMap;

        /// <summary>
        /// Clones this material
        /// </summary>
        /// <returns></returns>
        public Material Clone()
        {
            Material cloned = new Material();

            cloned.diffuseColor = this.diffuseColor;
            cloned.specularColor = this.specularColor;
            cloned.shininess = this.shininess;
            cloned.transparancy = this.transparancy;

            cloned.normalMap = this.normalMap;
            cloned.specularMap = this.specularMap;
            cloned.diffuseMap = this.diffuseMap;

            cloned.hasDiffuseMap = this.hasDiffuseMap;
            cloned.hasSpecularMap = this.hasSpecularMap;
            cloned.hasNormalMap = this.hasNormalMap;

            return cloned;
        }
    }
}
