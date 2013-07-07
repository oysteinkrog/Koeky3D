using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;
using Koeky3D.Utilities;
using OpenTK.Graphics;

namespace Koeky3D.BufferHandling
{
    /// <summary>
    /// Provides a wrapper for the vertex buffer object in opengl.
    /// </summary>
    public class VertexBuffer
    {
        #region variables
        private int bufferId;
        private BufferUsageHint usageHint;
        private int vertexCount;

        private BufferElement[] attributes = null;
        private int vertexSize;
        private int bufferSize;
        #endregion

        #region constructors
        public VertexBuffer(BufferUsageHint usageHint)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;
        }
        public VertexBuffer(BufferUsageHint usageHint, params BufferElement[] bufferLayout)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            // Copy buffer attributes array
            this.attributes = new BufferElement[bufferLayout.Length];
            Array.Copy(bufferLayout, this.attributes, bufferLayout.Length);
        }

        public VertexBuffer(BufferUsageHint usageHint, int attribute, Vector2[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, Vector3[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, Vector4[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, Color4[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, VertexTextureNormal[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, float[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, int[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, uint[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, short[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, ushort[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        public VertexBuffer(BufferUsageHint usageHint, int attribute, byte[] data)
        {
            GL.GenBuffers(1, out this.bufferId);

            this.usageHint = usageHint;

            this.SetData(data, attribute);
        }
        #endregion

        #region VertexBuffer methods
        /// <summary>
        /// Unmaps this buffer from the system memory.
        /// </summary>
        public void UnmapBuffer()
        {
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);

            UnbindBuffer();
        }
        /// <summary>
        /// Maps this buffer to the system memory and returns a pointer to the data.
        /// </summary>
        /// <param name="acces"></param>
        /// <returns>a void* to the data. Cast this pointer to a more convenient data type (like float* or int*).</returns>
        public unsafe void* MapBuffer(BufferAccess acces)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.bufferId);

            return (void*)GL.MapBuffer(BufferTarget.ArrayBuffer, acces);
        }

        /// <summary>
        /// Binds the opengl vertex buffer object.
        /// </summary>
        public void BindBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.bufferId);
        }
        /// <summary>
        /// Unbinds the opengl vertex buffer object.
        /// </summary>
        public void UnbindBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// Binds this buffer's attributes, allowing it to be used.
        /// </summary>
        public void BindAttributes()
        {
            foreach (BufferElement element in this.attributes)
                GL.EnableVertexAttribArray((int)element.attribute);
        }
        /// <summary>
        /// Unbinds this buffer's attributes, disallowing it to be used.
        /// </summary>
        public void UnbindAttributes()
        {
            foreach (BufferElement element in this.attributes)
                GL.DisableVertexAttribArray((int)element.attribute);
        }

        /// <summary>
        /// Initialises the buffer attributes
        /// </summary>
        public void InitialiseAttributes()
        {
            foreach (BufferElement element in this.attributes)
            {
                GL.VertexAttribPointer((int)element.attribute, element.componentCount, element.type, false, element.stride, element.byteOffset);
            }
        }

        /// <summary>
        /// Defines the layout for this buffer.
        /// </summary>
        /// <param name="bufferLayout"></param>
        public void SetBufferLayout(params BufferElement[] bufferLayout)
        {
            if (this.attributes != null)
                throw new Exception("Buffer attributes are already defined");

            // Copy buffer attributes array
            this.attributes = new BufferElement[bufferLayout.Length];
            Array.Copy(bufferLayout, this.attributes, bufferLayout.Length);
        }

        /// <summary>
        /// Empties this buffer. It can still be used to store new data after this call.
        /// </summary>
        public void EmptyBuffer()
        {
            BindBuffer();
            GL.BufferData(BufferTarget.ArrayBuffer, IntPtr.Zero, IntPtr.Zero, this.usageHint);
            UnbindBuffer();
            this.vertexCount = 0;
            this.vertexSize = 0;
            this.bufferSize = 0;
        }

        /// <summary>
        /// Clears the resources of this VertexBuffer from memory.
        /// Do not use this VertexBuffer after this call.
        /// </summary>
        public void ClearResources()
        {
            GL.DeleteBuffers(1, ref this.bufferId);
            this.vertexCount = 0;
        }
        #endregion

        #region Data set methods
        /// <summary>
        /// Uploads the specified data to the VertexBuffer
        /// </summary>
        /// <typeparam name="T">The type of the data. Must be a struct</typeparam>
        /// <param name="data">The data to upload. Null values are not allowed.</param>
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(data, data.Length);
        }
        /// <summary>
        /// Uploads the specified data to the VertexBuffer
        /// </summary>
        /// <typeparam name="T">The type of the data. Must be a struct</typeparam>
        /// <param name="data">The data to upload. Null values are allowed.</param>
        /// <param name="length">The amount of elements to upload. Can be more than the array size. The total size of the buffer equals elementSize * length</param>
        public void SetData<T>(T[] data, int length) where T : struct
        {
            if (this.attributes == null)
                throw new Exception("No buffer attributes defined");

            this.vertexSize = Marshal.SizeOf(typeof(T));
            this.bufferSize = this.vertexSize * length;
            this.vertexCount = length;

            // upload data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.bufferId);
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)this.bufferSize, data, this.usageHint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        /// <summary>
        /// Uploads the specified data to the VertexBuffer
        /// </summary>
        /// <typeparam name="T">The type of the data. Must be a struct</typeparam>
        /// <param name="data">The data to upload in a 2d array.</param>
        public void SetData<T>(T[,] data) where T : struct
        {
            if (this.attributes == null)
                throw new Exception("No buffer attributes defined");

            this.vertexSize = Marshal.SizeOf(typeof(T));
            this.bufferSize = this.vertexSize * data.GetLength(0) * data.GetLength(1);
            this.vertexCount = data.GetLength(0) * data.GetLength(1);

            // upload data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.bufferId);
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)this.bufferSize, data, this.usageHint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void SetData(VertexTextureNormal[] data)
        {
            int structSize = Marshal.SizeOf(typeof(VertexTextureNormal));

            if (this.attributes == null)
            {
                BufferElement[] elements = new BufferElement[3]
                {
                    new BufferElement((int)BufferAttribute.Vertex, VertexAttribPointerType.Float, 3, structSize, 0),
                    new BufferElement((int)BufferAttribute.TexCoord, VertexAttribPointerType.Float, 2, structSize, sizeof(float) * 3),
                    new BufferElement((int)BufferAttribute.Normal, VertexAttribPointerType.Float, 3, structSize, sizeof(float) * 5)
                };
                this.SetBufferLayout(elements);
            }
            SetData<VertexTextureNormal>(data);
        }
        public void SetData(Vector2[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.Float, 2, sizeof(float) * 2, 0));

            SetData<Vector2>(data);
        }
        public void SetData(Vector3[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.Float, 3, sizeof(float) * 3, 0));

            SetData<Vector3>(data);
        }
        public void SetData(Vector4[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.Float, 4, sizeof(float) * 4, 0));

            SetData<Vector4>(data);
        }
        public void SetData(Color4[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.Float, 4, sizeof(float) * 4, 0));

            SetData<Color4>(data);
        }
        public void SetData(float[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.Float, 1, sizeof(float), 0));

            SetData<float>(data);
        }
        public void SetData(int[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.UnsignedInt, 1, sizeof(int), 0));

            SetData<int>(data);
        }
        public void SetData(uint[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.UnsignedInt, 1, sizeof(uint), 0));

            SetData<uint>(data);
        }
        public void SetData(short[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.UnsignedShort, 1, sizeof(short), 0));

            SetData<short>(data);
        }
        public void SetData(ushort[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.UnsignedShort, 1, sizeof(ushort), 0));

            SetData<ushort>(data);
        }
        public void SetData(byte[] data, int attribute)
        {
            if (this.attributes == null)
                this.SetBufferLayout(new BufferElement(attribute, VertexAttribPointerType.UnsignedByte, 1, sizeof(byte), 0));

            SetData<byte>(data);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The opengl id of this buffer.
        /// </summary>
        public int BufferId
        {
            get
            {
                return this.bufferId;
            }
        }

        /// <summary>
        /// The amount of vertices in this buffer.
        /// </summary>
        public int VertexCount
        {
            get
            {
                return this.vertexCount;
            }
        }
        #endregion
    }
}
