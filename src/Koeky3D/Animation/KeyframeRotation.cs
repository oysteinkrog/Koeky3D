using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.Utilities;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Specifies a rotation keyframe
    /// </summary>
    public class KeyframeRotation
    {
        /// <summary>
        /// The time at which this keyframe starts
        /// </summary>
        public float time;
        /// <summary>
        /// The rotation of this keyframe
        /// </summary>
        public Quaternion quaternion;

        /// <summary>
        /// Constructs a new KeyframeRotation object
        /// </summary>
        /// <param name="time"></param>
        /// <param name="xAngle">The angle in radians</param>
        /// <param name="yAngle">The angle in radians</param>
        /// <param name="zAngle">The angle in radians</param>
        public KeyframeRotation(float time, float xAngle, float yAngle, float zAngle)
        {
            this.time = time;
            this.quaternion = GLConversion.CreateQuaternion(new Vector3(xAngle, yAngle, zAngle));
        }
        /// <summary>
        /// Constructs a new KeyframeRotation object
        /// </summary>
        /// <param name="time"></param>
        /// <param name="quaternion"></param>
        public KeyframeRotation(float time, Quaternion quaternion)
        {
            this.time = time;
            this.quaternion = quaternion;
        }
    }
}
