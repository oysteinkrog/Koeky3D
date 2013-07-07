using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Shapes
{
    /// <summary>
    /// Provides an implementation of an axis aligned bounding box.
    /// </summary>
    public class AABB
    {
        #region variables
        /// <summary>
        /// The min bound (or extent) of this AABB
        /// </summary>
        public Vector3 min;
        /// <summary>
        /// The max bound (or extent) of this AABB
        /// </summary>
        public Vector3 max;
        private Vector3 position;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs an AABB from the given AABB
        /// </summary>
        /// <param name="toCopy">The AABB to copy</param>
        public AABB(AABB toCopy)
            : this(toCopy.min, toCopy.max)
        {
        }
        /// <summary>
        /// Constructs an AABB
        /// </summary>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        public AABB(Vector3 min, Vector3 max)
        {
            this.position = new Vector3(this.min.X + (this.max.X - this.min.X) / 2, this.min.Y + (this.max.Y - this.min.Y) / 2, this.min.Y + (this.max.Y - this.min.Y) / 2);

            this.min = min;
            this.max = max;
        }
        /// <summary>
        /// Constructs an AABB
        /// </summary>
        /// <param name="position">The center position</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="length">The length</param>
        public AABB(Vector3 position, float width, float height, float length)
        {
            this.position = position;

            float hW = width / 2;
            float hH = height / 2;
            float hL = length / 2;

            this.min = new Vector3(position.X - hW, position.Y - hH, position.Z - hL);
            this.max = new Vector3(position.X + hW, position.Y + hH, position.Z + hL);
        }
        #endregion

        #region AABB methods
        /// <summary>
        /// Updates the min and max bound of this AABB based on the given position.
        /// </summary>
        /// <param name="pos"></param>
        public void UpdateWithPosition(Vector3 pos)
        {
            this.min = (this.min - this.position) + pos;
            this.max = (this.max - this.position) + pos;
        }
        /// <summary>
        /// Updates the position of this AABB based on a transform matrix
        /// </summary>
        /// <param name="transform"></param>
        public void UpdateWithTransform(ref Matrix4 transform)
        {
            // extract position and call UpdateWithPosition
            UpdateWithPosition(new Vector3(transform.M41, transform.M42, transform.M43));
        }
        #endregion

        #region object methods
        /// <summary>
        /// Returns a string representation of the AABB for debug purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.min.ToString() + " - " + this.max.ToString();
        }
        #endregion
    }
}
