using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Specifies the various animation types
    /// </summary>
    public enum AnimationType : byte
    {
        /// <summary>
        /// The animation starts at the beginning and ends at the end
        /// </summary>
        ForwardStop = 0,
        /// <summary>
        /// The animation starts at the end and ends at the beginning
        /// </summary>
        BackwardStop = 1,
        /// <summary>
        /// The animation start at the beginning and moves back at the end. Once back at the beginning again it ends.
        /// </summary>
        ForwardBackwardStop = 2,
        /// <summary>
        /// The animation start at the end and moves back at the end. Once back at the end again it ends.
        /// </summary>
        BackwardForwardStop = 3,
        /// <summary>
        /// The animation starts at the beginning and ends at the end. This is then looped.
        /// </summary>
        ForwardLoop = 4,
        /// <summary>
        /// The animation start at the end and ends at the beginning. This is then looped.
        /// </summary>
        BackwardLoop = 5,
        /// <summary>
        /// The animation start at the beginning and moves back at the end. Once back at the beginning again it ends. This is then looped.
        /// </summary>
        ForwardBackwardLoop = 6,
        /// <summary>
        /// The animation start at the end and moves back at the end. Once back at the end again it ends. This is then looped.
        /// </summary>
        BackwardForwardLoop = 7,
        /// <summary>
        /// The animation starts at the beginning and freezes at the end.
        /// </summary>
        ForwardHalt = 8,
        /// <summary>
        /// The animation starts at the end and freezes at the beginning.
        /// </summary>
        BackwardHalt = 9
    }
}
