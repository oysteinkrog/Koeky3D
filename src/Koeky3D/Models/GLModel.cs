using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.Shapes;
using Koeky3D.Animation;
using OpenTK.Graphics;

namespace Koeky3D.Models
{
    /// <summary>
    /// Specifies a GLModel. A GLModel contains a collection of GLMesh objects.
    /// </summary>
    public class GLModel
    {
        #region variables
        private GLMesh[] meshes;
        private Matrix4 transform;
        /// <summary>
        /// The bounding box for this model
        /// </summary>
        public AABB boundingBox;
        /// <summary>
        /// The skeleton, if any, of this model
        /// </summary>
        public Skeleton skeleton;
        /// <summary>
        /// The computed distance to the camera for the current frame.
        /// </summary>
        public float computedDistance;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new GLModel object
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="transform"></param>
        /// <param name="boundingBox"></param>
        /// <param name="skeleton"></param>
        public GLModel(GLMesh[] meshes, Matrix4 transform, AABB boundingBox, Skeleton skeleton)
        {
            this.meshes = meshes;
            this.transform = transform;
            this.boundingBox = boundingBox;
            this.skeleton = skeleton;

            // link meshes to this IModel
            foreach (GLMesh mesh in this.meshes)
            {
                mesh.Model = this;
                mesh.transform = transform;
            }
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Gets a collection of GLMesh objects
        /// </summary>
        public GLMesh[] Meshes
        {
            get { return this.meshes; }
        }
        /// <summary>
        /// Gets or sets the transform of this model
        /// </summary>
        public Matrix4 Transform
        {
            get
            {
                return this.transform;
            }
            set
            {
                this.transform = value;

                foreach (GLMesh mesh in this.meshes)
                    mesh.transform = value;

                this.boundingBox.UpdateWithTransform(ref value);
            }
        }
        #endregion
    }
}
