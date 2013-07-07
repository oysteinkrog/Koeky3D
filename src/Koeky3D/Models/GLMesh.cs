using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shaders;
using Koeky3D.BufferHandling;
using OpenTK;
using Koeky3D.Animation;
using OpenTK.Graphics;

namespace Koeky3D.Models
{
    /// <summary>
    /// Specifies a part of a GLModel. A GLMesh is a triangle mesh with a material.
    /// </summary>
    public class GLMesh : IComparable<GLMesh>
    {
        #region variables
        /// <summary>
        /// The vertex array object for this triangle mesh
        /// </summary>
        public VertexArray vertexArray;
        /// <summary>
        /// The index buffer for this triangle mesh
        /// </summary>
        public IndexBuffer indexBuffer;
        /// <summary>
        /// The material
        /// </summary>
        public Material material;
        private GLModel model;
        private int vertexCount;
        private int vertexOffset;

        /// <summary>
        /// The transform of this triangle mesh
        /// </summary>
        public Matrix4 transform;
        #endregion

        #region contructors
        /// <summary>
        /// Constructs a new GLMesh object
        /// </summary>
        /// <param name="vertexArray"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="material"></param>
        /// <param name="vertexOffset"></param>
        /// <param name="vertexCount"></param>
        public GLMesh(VertexArray vertexArray, IndexBuffer indexBuffer, Material material,
                      int vertexOffset, int vertexCount)
        {
            this.vertexArray = vertexArray;
            this.indexBuffer = indexBuffer;
            this.material = material;
            this.vertexOffset = vertexOffset;
            this.vertexCount = vertexCount;
        }
        #endregion

        #region IComparable methods
        /// <summary>
        /// Compares this GLMesh to the given GLMesh
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(GLMesh other)
        {
            // check if transparancy is available
            if (this.material.transparancy != 1.0f && other.material.transparancy != 1.0f)
            {
                // both meshes contain transparancy, so we need to sort this on distance first
                if (this.model.computedDistance < other.model.computedDistance)
                    return -1;
                else if (this.model.computedDistance > other.model.computedDistance)
                    return 1;
            }

            // sort on textures
            if (other.material.hasDiffuseMap && this.material.hasDiffuseMap)
            {
                if (other.material.diffuseMap.textureId < this.material.diffuseMap.textureId)
                    return -1;
                else if (other.material.diffuseMap.textureId > this.material.diffuseMap.textureId)
                    return 1;
            }

            if (other.material.hasSpecularMap && this.material.hasSpecularMap)
            {
                if (other.material.specularMap.textureId < this.material.specularMap.textureId)
                    return -1;
                else if (other.material.specularMap.textureId > this.material.specularMap.textureId)
                    return 1;
            }

            if (other.material.hasNormalMap && this.material.hasNormalMap)
            {
                if (other.material.normalMap.textureId < this.material.normalMap.textureId)
                    return -1;
                else if (other.material.normalMap.textureId > this.material.normalMap.textureId)
                    return 1;
            }

            // sort on buffers
            if (other.vertexArray.vaoId < this.vertexArray.vaoId)
                return -1;
            else if (other.vertexArray.vaoId > this.vertexArray.vaoId)
                return 1;

            if (other.indexBuffer.bufferId < this.indexBuffer.bufferId)
                return -1;
            else if (other.indexBuffer.bufferId > this.indexBuffer.bufferId)
                return 1;

            return 0;
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Gets the current skeleton
        /// </summary>
        public Skeleton Skeleton
        {
            get { return this.model.skeleton; }
        }
        /// <summary>
        /// Gets the GLModel this GLMesh is part of
        /// </summary>
        public GLModel Model
        {
            get
            {
                return this.model;
            }
            set
            {
                this.model = value;
            }
        }
        /// <summary>
        /// The amount of indices to use when rendering this model
        /// </summary>
        public int IndexCount
        {
            get
            { 
                return this.vertexCount;
            }
        }
        /// <summary>
        /// The offset or start of the first index in the index buffer
        /// </summary>
        public int IndexOffset
        {
            get
            {
                return this.vertexOffset;
            }
        }
        #endregion
    }
}
