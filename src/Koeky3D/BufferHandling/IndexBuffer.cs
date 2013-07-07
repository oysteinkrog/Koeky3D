using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Koeky3D.BufferHandling
{
    public class IndexBuffer
    {
        #region variables
        public int bufferId = -1;
        private BufferUsageHint usageHint;
        private DrawElementsType elementType;

        private int count;
        #endregion

        #region constructors
        public IndexBuffer(BufferUsageHint usageHint, short[] indices)
        {
            this.usageHint = usageHint;
            this.SetData(indices);
        }
        public IndexBuffer(BufferUsageHint usageHint, ushort[] indices)
        {
            this.usageHint = usageHint;
            this.SetData(indices);
        }
        public IndexBuffer(BufferUsageHint usageHint, int[] indices)
        {
            this.usageHint = usageHint;
            this.SetData(indices);
        }
        public IndexBuffer(BufferUsageHint usageHint, uint[] indices)
        {
            this.usageHint = usageHint;
            this.SetData(indices);
        }
        #endregion

        #region IndexBuffer methods
        public void SetData(short[] indices)
        {
            if (bufferId == -1)
            {
                GL.GenBuffers(1, out bufferId);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.bufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indices.Length), indices, usageHint);

            this.count = indices.Length;
            this.elementType = DrawElementsType.UnsignedShort;
        }
        public void SetData(ushort[] indices)
        {
            if (bufferId == -1)
            {
                GL.GenBuffers(1, out bufferId);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.bufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indices.Length), indices, usageHint);

            this.count = indices.Length;
            this.elementType = DrawElementsType.UnsignedShort;
        }
        public void SetData(int[] indices)
        {
            if (bufferId == -1)
            {
                GL.GenBuffers(1, out bufferId);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.bufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * indices.Length), indices, usageHint);

            this.count = indices.Length;
            this.elementType = DrawElementsType.UnsignedInt;
        }
        public void SetData(uint[] indices)
        {
            if (bufferId == -1)
            {
                GL.GenBuffers(1, out bufferId);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.bufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * indices.Length), indices, usageHint);

            this.count = indices.Length;
            this.elementType = DrawElementsType.UnsignedInt;
        }

        public void BindBuffer()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.bufferId);
        }
        public void UnbindBuffer()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void ClearResources()
        {
            GL.DeleteBuffers(1, ref this.bufferId);
        }
        #endregion

        #region getters & setters
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public DrawElementsType ElementType
        {
            get
            {
                return this.elementType;
            }
        }
        #endregion
    }
}
