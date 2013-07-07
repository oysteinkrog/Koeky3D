using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Koeky3D.BufferHandling
{
    /// <summary>
    /// Defines a buffer element
    /// </summary>
    public struct BufferElement
    {
        public int componentCount;
        public int attribute;
        public VertexAttribPointerType type;
        public int stride;
        public int byteOffset;

        /// <summary>
        /// Constructs a BufferElement
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="type">The vertex type</param>
        /// <param name="componentCount">The amount of components this vertex has</param>
        /// <param name="stride">The stride</param>
        /// <param name="byteOffset">The offset in bytes until this element is found</param>
        public BufferElement(int attribute, VertexAttribPointerType type, int componentCount, int stride, int byteOffset)
        {
            this.attribute = attribute;
            this.type = type;
            this.componentCount = componentCount;
            this.stride = stride;
            this.byteOffset = byteOffset;
        }
    }
}
