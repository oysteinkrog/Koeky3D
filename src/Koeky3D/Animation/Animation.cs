using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koeky3D.Animation
{
    /// <summary>
    /// The animation class is used to store an animation.
    /// For every bone in a skeleton it contains a BoneKeyFrames object.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// The name of the animation
        /// </summary>
        public String animationName = "";
        /// <summary>
        /// The total running time of the animation
        /// </summary>
        public readonly float totalTime;
        /// <summary>
        /// The keyframes per bone
        /// </summary>
        public readonly BoneKeyframes[] framesPerBone;

        /// <summary>
        /// Constructs an animation
        /// </summary>
        /// <param name="framesPerBone">The keyframes per bone</param>
        public Animation(BoneKeyframes[] framesPerBone)
        {
            this.framesPerBone = framesPerBone;
            this.totalTime = float.MinValue;
            foreach (BoneKeyframes frames in this.framesPerBone)
            {
                if (frames.TotalTime >= totalTime)
                    this.totalTime = frames.TotalTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.animationName;
        }
    }
}
