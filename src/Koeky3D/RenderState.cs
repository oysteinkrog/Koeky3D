using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics;

namespace Koeky3D
{
    /// <summary>
    /// A container class for the OpenGL render state variables.
    /// This class is used internally in the GLManager class. This class does not call any OpenGL functions to set render options.
    /// </summary>
    public class RenderState
    {
        /// <summary>
        /// True if depth testing is enabled
        /// </summary>
        public bool depthEnabled;
        /// <summary>
        /// True if writing of depth value is enabled
        /// </summary>
        public bool writeDepth;
        /// <summary>
        /// The depth function to use when depth testing
        /// </summary>
        public DepthFunction depthFunction;

        /// <summary>
        /// True if face culling is enabled
        /// </summary>
        public bool cullingEnabled;
        /// <summary>
        /// The face culling mode
        /// </summary>
        public CullFaceMode cullingMode;

        /// <summary>
        /// True if blending is enabled
        /// </summary>
        public bool blendingEnabled;
        /// <summary>
        /// The blending equation at the source
        /// </summary>
        public BlendingFactorSrc blendSrc;
        /// <summary>
        /// The blending equation at the destination
        /// </summary>
        public BlendingFactorDest blendDest;
        /// <summary>
        /// The constant blending color
        /// </summary>
        public Color4 blendColor;

        /// <summary>
        /// The polygon mode to use when rendering
        /// </summary>
        public PolygonMode polygonMode;

        /// <summary>
        /// The clear color to use when clearing the screen
        /// </summary>
        public Color4 clearColor;

        /// <summary>
        /// True if rasterizing is disabled (pixels are not processed)
        /// </summary>
        public bool discardRasterizer;

        /// <summary>
        /// Constructs a new render state with default values
        /// </summary>
        public RenderState()
        {
            this.depthEnabled = true;
            this.writeDepth = true;
            this.depthFunction = DepthFunction.Lequal;

            this.cullingEnabled = true;
            this.cullingMode = CullFaceMode.Back;

            this.blendingEnabled = false;
            this.blendSrc = BlendingFactorSrc.One;
            this.blendDest = BlendingFactorDest.One;
            this.blendColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

            this.polygonMode = PolygonMode.Fill;

            this.clearColor = Color4.Black;

            this.discardRasterizer = false;
        }
        /// <summary>
        /// Constructs a new render state
        /// </summary>
        /// <param name="toCopy">The renderstate to copy</param>
        public RenderState(RenderState toCopy)
        {
            this.depthEnabled = toCopy.depthEnabled;
            this.writeDepth = toCopy.writeDepth;
            this.depthFunction = toCopy.depthFunction;

            this.cullingEnabled = toCopy.cullingEnabled;
            this.cullingMode = toCopy.cullingMode;

            this.blendingEnabled = toCopy.blendingEnabled;
            this.blendSrc = toCopy.blendSrc;
            this.blendDest = toCopy.blendDest;
            this.blendColor = toCopy.blendColor;

            this.polygonMode = toCopy.polygonMode;

            this.clearColor = toCopy.clearColor;

            this.discardRasterizer = toCopy.discardRasterizer;
        }
    }
}
