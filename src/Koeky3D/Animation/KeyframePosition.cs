using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Specifies a translation key frame
    /// </summary>
    public class KeyframePosition
    {
        /// <summary>
        /// The time at which this keyframe starts
        /// </summary>
        public float time;
        /// <summary>
        /// The translation for this keyframe
        /// </summary>
        public Vector3 translation;

        /// <summary>
        /// Constructs a new KeyframePosition object
        /// </summary>
        /// <param name="time"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public KeyframePosition(float time, float x, float y, float z)
        {
            this.time = time;
            this.translation = new Vector3(x, y, z);
        }
    }
}
