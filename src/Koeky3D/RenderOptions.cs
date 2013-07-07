using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Koeky3D.Shapes;
using System.Drawing;

namespace Koeky3D
{
    /// <summary>
    /// Provides an easy way to create view frustums, perspective projections and orthographics projections.
    /// It keeps track of the resolution, fov, window state (fullscreen, normal, minimized) and vsync mode.
    /// When one of these values change an event is fired. This class could be used as a general window manager.
    /// </summary>
    public class RenderOptions
    {
        #region variables
        private Size resolution;
        private float fov, zNear, zFar;

        private VSyncMode vSyncMode;
        private WindowState windowState;

        private Matrix4 projection, ortho;
        #endregion

        #region events
        /// <summary>
        /// Raised when any of the variables in this RenderOptions class is changed.
        /// </summary>
        public event EventHandler OnSettingsChange;
        /// <summary>
        /// Raised when the resolution (width and height) is changed.
        /// </summary>
        public event EventHandler OnResolutionChange;
        /// <summary>
        /// Raised when the fov, zNear or zFar variables are changed.
        /// </summary>
        public event EventHandler OnViewFrustumChange;
        /// <summary>
        /// Raised when the window state has changed.
        /// </summary>
        public event EventHandler OnWindowStateChange;
        /// <summary>
        /// Raised when the vsync mode has changed.
        /// </summary>
        public event EventHandler OnVSyncModeChange;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a default RenderOptions object
        /// </summary>
        public RenderOptions()
            : this(800, 600, WindowState.Normal, VSyncMode.Off)
        {
        }
        /// <summary>
        /// Constructs a RenderOptions object
        /// </summary>
        /// <param name="width">The width of the window</param>
        /// <param name="height">The height of the window</param>
        /// <param name="windowState">The state of the window (fullscreen, normal or minimized)</param>
        /// <param name="vSync">The vsync mode</param>
        public RenderOptions(int width, int height, WindowState windowState, VSyncMode vSync)
            : this(width, height, windowState, vSync, 45.0f, 0.1f, 100.0f)
        {
        }
        /// <summary>
        /// Constructs a RenderOptions object
        /// </summary>
        /// <param name="width">The width of the window</param>
        /// <param name="height">The height of the window</param>
        /// <param name="windowState">The state of the window (fullscreen, normal or minimized)</param>
        /// <param name="vSync">The vsync mode</param>
        /// <param name="fov">The field of view in degrees</param>
        /// <param name="zNear">The near plane</param>
        /// <param name="zFar">The far plane</param>
        public RenderOptions(int width, int height, WindowState windowState, VSyncMode vSync, float fov, float zNear, float zFar)
        {
            this.resolution = new Size(width, height);
            this.windowState = windowState;
            this.vSyncMode = vSync;
            this.fov = fov;
            this.zNear = zNear;
            this.zFar = zFar;

            UpdateProjectionOrtho();
            SetViewPort();
        }
        #endregion

        #region RenderOptions methods
        private void UpdateProjectionOrtho()
        {
            this.projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.fov),
                                                                         AspectRatio,
                                                                         this.zNear,
                                                                         this.zFar);

            this.ortho = Matrix4.CreateOrthographicOffCenter(0.0f, this.Width, this.Height, 0.0f, -30.0f, 30.0f);
        }

        /// <summary>
        /// Sets the OpenGL viewport based on the settings in this RenderOptions instance
        /// </summary>
        public void SetViewPort()
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        /// <summary>
        /// Creates a viewing frustum based on the current render options
        /// </summary>
        /// <returns></returns>
        public ViewFrustum CreateViewFrustum()
        {
            return new ViewFrustum(this.fov, AspectRatio, this.zNear, this.zFar);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Sets or gets the current resolution
        /// </summary>
        public Size Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                this.resolution = value;
                if (this.OnResolutionChange != null)
                    this.OnResolutionChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);
                if (this.OnViewFrustumChange != null)
                    this.OnViewFrustumChange(this, EventArgs.Empty);

                this.SetViewPort();
                this.UpdateProjectionOrtho();
            }
        }
        /// <summary>
        /// Sets or gets the width of the resolution
        /// </summary>
        public int Width
        {
            get
            {
                return this.Resolution.Width;
            }
            set
            {
                Size newResolution = this.resolution;
                newResolution.Width = value;
                Resolution = newResolution;
            }
        }
        /// <summary>
        /// Sets or gets the height of the resolution
        /// </summary>
        public int Height
        {
            get
            {
                return this.Resolution.Height;
            }
            set
            {
                Size newResolution = this.resolution;
                newResolution.Height = value;
                Resolution = newResolution;
            }
        }

        /// <summary>
        /// Sets or gets the current field of view in degrees
        /// </summary>
        public float FOV
        {
            get
            {
                return this.fov;
            }
            set
            {
                this.fov = value;
                if (this.fov <= 0.0f)
                    this.fov = 1.0f;
                if (this.fov >= 180.0f)
                    this.fov = 179.0f;

                if (this.OnViewFrustumChange != null)
                    this.OnViewFrustumChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);

                this.UpdateProjectionOrtho();
            }
        }
        /// <summary>
        /// Sets or gets the z near plane
        /// </summary>
        public float ZNear
        {
            get
            {
                return this.zNear;
            }
            set
            {
                this.zNear = value;
                if (this.zNear <= 0.0f)
                    this.zNear = 0.1f;

                if (this.OnViewFrustumChange != null)
                    this.OnViewFrustumChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);

                this.UpdateProjectionOrtho();

            }
        }
        /// <summary>
        /// Sets or gets the z far plane
        /// </summary>
        public float ZFar
        {
            get
            {
                return this.zFar;
            }
            set
            {
                this.zFar = value;

                if (this.OnViewFrustumChange != null)
                    this.OnViewFrustumChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);

                this.UpdateProjectionOrtho();

            }
        }

        /// <summary>
        /// Sets or gets the current vsync mode
        /// ote that this does not actually change the vsync mode, it only notifies others the vsync mode has changed and stores the new vsync mode.
        /// </summary>
        public VSyncMode VSyncMode
        {
            get
            {
                return this.vSyncMode;
            }
            set
            {
                this.vSyncMode = value;

                if (this.OnVSyncModeChange != null)
                    this.OnVSyncModeChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Sets or gets the current window state. 
        /// Note that this does not actually change the window state, it only notifies others the window state has changed and stores the new window state.
        /// </summary>
        public WindowState WindowState
        {
            get
            {
                return this.windowState;
            }
            set
            {
                this.windowState = value;

                if (this.OnWindowStateChange != null)
                    this.OnWindowStateChange(this, EventArgs.Empty);
                if (this.OnSettingsChange != null)
                    this.OnSettingsChange(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The current aspect ratio
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return this.Width / (float)this.Height;
            }
        }

        /// <summary>
        /// The current perspective projection matrix
        /// </summary>
        public Matrix4 Projection
        {
            get
            {
                return this.projection;
            }
        }
        /// <summary>
        /// The current orthographic projection matrix
        /// </summary>
        public Matrix4 Ortho
        {
            get
            {
                return this.ortho;
            }
        }
        #endregion
    }
}
