using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Koeky3D.Utilities;

namespace Koeky3D.BufferHandling
{
    /// <summary>
    /// Provides a wrapper for the vertex array object in OpenGL.
    /// </summary>
    public class VertexArray
    {
        #region variables
        public int vaoId;
        private VertexBuffer[] vertexBuffers;
        private IndexBuffer indexBuffer;
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new VertexArray.
        /// </summary>
        /// <param name="vertexBuffers">An array of VertexBuffer objects which contain the vertex data</param>
        public VertexArray(params VertexBuffer[] vertexBuffers)
            : this(null, vertexBuffers)
        {
        }
        /// <summary>
        /// Creates a new VertexArray.
        /// </summary>
        /// <param name="indexBuffer">The index buffers to use with this vertex array</param>
        /// <param name="vertexBuffers">An array of VertexBuffer objects which contain the vertex data</param>
        public VertexArray(IndexBuffer indexBuffer, params VertexBuffer[] vertexBuffers)
        {
            GL.GenVertexArrays(1, out this.vaoId);

            this.indexBuffer = indexBuffer;

            // Copy the data from the given array to a class specific array
            this.vertexBuffers = new VertexBuffer[vertexBuffers.Length];
            Array.Copy(vertexBuffers, this.vertexBuffers, vertexBuffers.Length);

            GL.BindVertexArray(this.vaoId);

            foreach (VertexBuffer buffer in this.vertexBuffers)
            {
                buffer.BindBuffer();
                buffer.BindAttributes();
                buffer.InitialiseAttributes();
            }

            if (this.indexBuffer != null)
            {
                this.indexBuffer.BindBuffer();
            }

            GL.BindVertexArray(0);
        }
        #endregion

        #region VertexArray methods
        /// <summary>
        /// Binds this VertexArray
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(this.vaoId);
        }
        /// <summary>
        /// Unbinds this VertexArray
        /// </summary>
        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Clears the resources of this VertexArray from memory.
        /// Do not use this VertexArray after this call.
        /// </summary>
        /// <param name="clearBuffers">True if the vertex buffers part of this vertex array object also need to be cleared</param>
        public void ClearResources(bool clearBuffers)
        {
            if (clearBuffers)
            {
                foreach (VertexBuffer buffer in this.vertexBuffers)
                    buffer.ClearResources();
                if (this.indexBuffer != null)
                    this.indexBuffer.ClearResources();
            }

            GL.DeleteVertexArrays(1, ref this.vaoId);
        }

        /// <summary>
        /// Adds the given VertexBuffer to this VertexArray.
        /// </summary>
        /// <param name="vertexBuffer"></param>
        public void AddBuffer(VertexBuffer vertexBuffer)
        {
            this.Bind();

            vertexBuffer.BindBuffer();
            vertexBuffer.InitialiseAttributes();
            vertexBuffer.BindAttributes();

            this.Unbind();
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The id of this vertex array object.
        /// </summary>
        public int BufferId
        {
            get
            {
                return this.vaoId;
            }
        }

        /// <summary>
        /// True if this vertex array object binds an index buffer
        /// </summary>
        public bool HasIndexBuffer
        {
            get
            {
                return this.indexBuffer != null;
            }
        }
        /// <summary>
        /// Gets the attached index buffer or null if no index buffer is attached
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get
            {
                return this.indexBuffer;
            }
        }
        #endregion
    }
}
