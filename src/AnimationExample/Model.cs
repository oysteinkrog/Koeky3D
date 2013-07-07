using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.BufferHandling;
using Koeky3D.Textures;
using Koeky3D;
using OpenTK.Graphics.OpenGL;

namespace SimpleModelExample
{
    class Model
    {
        /// <summary>
        /// The world transform of this model
        /// </summary>
        public Matrix4 world;
        /// <summary>
        /// The vertex array
        /// </summary>
        public VertexArray vertexArray;
        /// <summary>
        /// The index buffer per triangle mesh
        /// </summary>
        public IndexBuffer[] indexBuffers;
        /// <summary>
        /// The texture per triangle mesh
        /// </summary>
        public Texture2D[] textures;

        public Model(Matrix4 world, VertexArray vertexArray, IndexBuffer[] indexBuffers, Texture2D[] textures)
        {
            this.world = world;
            this.vertexArray = vertexArray;
            this.indexBuffers = indexBuffers;
            this.textures = textures;
        }

        /// <summary>
        /// Renders the model
        /// </summary>
        /// <param name="glManager"></param>
        public void Draw(GLManager glManager)
        {
            glManager.World = this.world;
            glManager.BindVertexArray(this.vertexArray);
            // render every mesh
            for(int i = 0; i < this.indexBuffers.Length; i++)
            {
                glManager.BindIndexBuffer(this.indexBuffers[i]);
                glManager.BindTexture(this.textures[i], TextureUnit.Texture0);

                glManager.DrawElementsIndexed(BeginMode.Triangles, this.indexBuffers[i].Count, 0);
            }
        }
    }
}
