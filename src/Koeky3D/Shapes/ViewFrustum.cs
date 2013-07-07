using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Shapes
{
    /// <summary>
    /// An enumeration specifying the 3 possible outcomes for an intersection test.
    /// </summary>
    public enum InBoundsType
    {
        /// <summary>
        /// One body contains the other completely
        /// </summary>
        FullyContained = 1,
        /// <summary>
        /// One body contains the other partly
        /// </summary>
        PartlyContained = 2,
        /// <summary>
        /// The two bodies do not intersect
        /// </summary>
        NotContained = 3
    };

    class Plane
    {
        public Vector3 vertex1, vertex2, vertex3;

        public Plane(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertex3 = vertex3;
        }

        public Vector3 GetNormal()
        {
            return Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1);
        }
    }

    /// <summary>
    /// Provides easy construction of a view frustum.
    /// The view frustum can be used for intersection checks with axis aligned bounding boxes.
    /// </summary>
    public class ViewFrustum
    {
        #region variables
        /// <summary>
        /// The current transform of this view frustum.
        /// Use SetTransform(...) to edit the transform.
        /// </summary>
        public Matrix4 transform = Matrix4.Identity;

        private Vector3[] identityVertices = new Vector3[8];
        private Vector3[] identityNormals = new Vector3[6];

        private Plane[] planes = new Plane[6];

        /// <summary>
        /// The eight vertices which define this view frustum.
        /// These will be updated when SetTransform(...) is called.
        /// </summary>
        public Vector3[] transformedVertices = new Vector3[8];
        /// <summary>
        /// The inward pointing normals. These are updated when SetTransform(...) is called.
        /// </summary>
        public Vector3[] transformedNormals = new Vector3[6];

        private float fov, aspectRatio, zNear, zFar;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a ViewFrustum.
        /// </summary>
        /// <param name="fov">The field of view in degrees</param>
        /// <param name="aspectRatio">The aspect ratio. Calculated by taking the width and height of the screen and doing: width / height.</param>
        /// <param name="zNear">The zNear value.</param>
        /// <param name="zFar">The zFar value.</param>
        public ViewFrustum(float fov, float aspectRatio, float zNear, float zFar)
        {
            this.fov = fov;
            this.aspectRatio = aspectRatio;
            this.zNear = zNear;
            this.zFar = zFar;

            CalculateIdentity();
            SetTransform(Matrix4.Identity);
        }
        #endregion

        #region ViewFrustum methods
        /// <summary>
        /// Sets the transform of this ViewFrustum. This function resets the ViewFrustum and then multiplies it with the given Matrix4.
        /// </summary>
        /// <param name="transform"></param>
        public void SetTransform(Matrix4 transform)
        {
            this.transform = transform;
            //this.LoadIdentity();

            // Update vertices
            for (int i = 0; i < 8; i++)
            {
                this.transformedVertices[i] = new Vector3(Vector4.Transform(new Vector4(this.identityVertices[i], 1.0f), transform));
                if (i < 6)
                {
                    this.transformedNormals[i] = new Vector3(Vector4.Transform(new Vector4(this.identityNormals[i], 0.0f), transform));
                    this.transformedNormals[i].Normalize();
                }
                //Matrix4 result = Matrix4.CreateTranslation(this.identityVertices[i]) * transform;
                //this.transformedVertices[i] = new Vector3(result.M41, result.M42, result.M43);
            }
            ConstructPlanes();
        }

        /// <summary>
        /// Checks if the given AABB is contained by this ViewFrustum.
        /// </summary>
        /// <param name="volumeBox"></param>
        /// <returns></returns>
        public InBoundsType IsInBounds(AABB volumeBox)
        {
            // Iterate all transformed normals
            bool allContained = true;
            for (int i = 0; i < 6; i++)
            {
                Vector3 normal = this.planes[i].GetNormal();

                Vector3 maxPoint = Vector3.Zero, minPoint = Vector3.Zero;

                // Compute x point
                if (normal.X > 0.0f)
                {
                    maxPoint.X = volumeBox.max.X;
                    minPoint.X = volumeBox.min.X;
                }
                else
                {
                    maxPoint.X = volumeBox.min.X;
                    minPoint.X = volumeBox.max.X;
                }
                // Compute y point
                if (normal.Y > 0.0f)
                {
                    maxPoint.Y = volumeBox.max.Y;
                    minPoint.Y = volumeBox.min.Y;
                }
                else
                {
                    maxPoint.Y = volumeBox.min.Y;
                    minPoint.Y = volumeBox.max.Y;
                }
                // Compute z point
                if (normal.Z > 0.0f)
                {
                    maxPoint.Z = volumeBox.max.Z;
                    minPoint.Z = volumeBox.min.Z;
                }
                else
                {
                    maxPoint.Z = volumeBox.min.Z;
                    minPoint.Z = volumeBox.max.Z;
                }

                maxPoint -= this.planes[i].vertex1;
                minPoint -= this.planes[i].vertex1;

                if (Vector3.Dot(normal, maxPoint) < 0.0f)
                    return InBoundsType.NotContained;
                if (allContained && Vector3.Dot(normal, minPoint) < 0.0f)
                    allContained = false;
            }

            return allContained ? InBoundsType.FullyContained : InBoundsType.PartlyContained;
        }

        private void ConstructPlanes()
        {
            // Left and right
            this.planes[0] = new Plane(this.transformedVertices[0], this.transformedVertices[4], this.transformedVertices[3]);
            this.planes[1] = new Plane(this.transformedVertices[1], this.transformedVertices[2], this.transformedVertices[5]);
            // Up and down
            this.planes[2] = new Plane(this.transformedVertices[4], this.transformedVertices[5], this.transformedVertices[7]);
            this.planes[3] = new Plane(this.transformedVertices[0], this.transformedVertices[3], this.transformedVertices[1]);
            // Front and back
            this.planes[4] = new Plane(this.transformedVertices[0], this.transformedVertices[1], this.transformedVertices[4]);
            this.planes[5] = new Plane(this.transformedVertices[3], this.transformedVertices[7], this.transformedVertices[2]);
        }

        private void LoadIdentity()
        {
            // Load identity vertices
            for (int i = 0; i < 8; i++)
            {
                this.transformedVertices[i] = this.identityVertices[i];
            }
            // Load identity normals
            for (int i = 0; i < 6; i++)
            {
                this.transformedNormals[i] = this.identityNormals[i];
            }
        }
        private void CalculateIdentity()
        {
            // calculate the variables we need
            float radFov = this.fov / 180.0f * (float)Math.PI;

            float hNear = zNear * (float)Math.Tan(radFov);
            float hFar = zFar * (float)Math.Tan(radFov);
            float wNear = aspectRatio * hNear;
            float wFar = aspectRatio * hFar;

            // we can now determine every point in the view frustum
            this.identityVertices[0] = new Vector3(-wNear, -hNear, zNear);
            this.identityVertices[1] = new Vector3(wNear, -hNear, zNear);

            this.identityVertices[2] = new Vector3(wFar, -hFar, zFar);
            this.identityVertices[3] = new Vector3(-wFar, -hFar, zFar);

            this.identityVertices[4] = new Vector3(-wNear, hNear, zNear);
            this.identityVertices[5] = new Vector3(wNear, hNear, zNear);

            this.identityVertices[6] = new Vector3(wFar, hFar, zFar);
            this.identityVertices[7] = new Vector3(-wFar, hFar, zFar);

            //this.identityVertices[0] = new Vector3(-wNear, -hNear, zNear);
            //this.identityVertices[1] = new Vector3(wNear, -hNear, zNear);

            //this.identityVertices[2] = new Vector3(wFar, -hFar, zFar);
            //this.identityVertices[3] = new Vector3(-wFar, -hFar, zFar);

            //this.identityVertices[4] = new Vector3(-wFar, hFar, zFar);
            //this.identityVertices[5] = new Vector3(-wNear, hNear, zNear);

            //this.identityVertices[6] = new Vector3(wNear, hNear, zNear);
            //this.identityVertices[7] = new Vector3(wFar, hFar, zFar);




            // Calculate the identity normals
            Vector3 leftNormal = Vector3.Cross(this.identityVertices[5] - this.identityVertices[0], this.identityVertices[3] - this.identityVertices[0]);
            Vector3 rightNormal = Vector3.Cross(this.identityVertices[6] - this.identityVertices[1], this.identityVertices[2] - this.identityVertices[1]);

            Vector3 upNormal = Vector3.Cross(this.identityVertices[4] - this.identityVertices[5], this.identityVertices[6] - this.identityVertices[5]);
            Vector3 downNormal = Vector3.Cross(this.identityVertices[3] - this.identityVertices[0], this.identityVertices[1] - this.identityVertices[0]);

            Vector3 frontNormal = Vector3.Cross(this.identityVertices[5] - this.identityVertices[0], this.identityVertices[1] - this.identityVertices[0]);
            Vector3 backNormal = Vector3.Cross(this.identityVertices[4] - this.identityVertices[3], this.identityVertices[2] - this.identityVertices[3]);

            // Normalize normals
            leftNormal.Normalize();
            rightNormal.Normalize();
            upNormal.Normalize();
            downNormal.Normalize();
            frontNormal.Normalize();
            backNormal.Normalize();

            this.identityNormals[0] = leftNormal;
            this.identityNormals[1] = rightNormal;
            this.identityNormals[2] = upNormal;
            this.identityNormals[3] = downNormal;
            this.identityNormals[4] = frontNormal;
            this.identityNormals[5] = backNormal;
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Gets or sets the field of view
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
                CalculateIdentity();
                SetTransform(this.transform);
            }
        }
        /// <summary>
        /// Gets or sets the aspect ratio
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return this.aspectRatio;
            }
            set
            {
                this.aspectRatio = value;
                CalculateIdentity();
                SetTransform(this.transform);
            }
        }
        /// <summary>
        /// Gets or sets the distance to the near plane
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
                CalculateIdentity();
                SetTransform(this.transform);
            }
        }
        /// <summary>
        /// Gets or sets the distance to the far plane
        /// </summary>
        public float ZFar
        {
            get
            {
                return this.zFar;
            }
            set
            {
                this.zNear = value;
                CalculateIdentity();
                SetTransform(this.transform);
            }
        }
        #endregion
    }
}
