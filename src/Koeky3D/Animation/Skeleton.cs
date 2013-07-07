using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Specifies a playing animation.
    /// </summary>
    public class RunningAnimation
    {
        /// <summary>
        /// The animation
        /// </summary>
        public Animation animation;
        /// <summary>
        /// The animation time
        /// </summary>
        public float time = 0.0f;
        /// <summary>
        /// The animation speed
        /// </summary>
        public float speed;
        /// <summary>
        /// The animation type
        /// </summary>
        public AnimationType type;
        /// <summary>
        /// The animation weight
        /// </summary>
        public float animWeight = 0.0f;
        /// <summary>
        /// True if the animation is finished
        /// </summary>
        public bool finished = false;
        /// <summary>
        /// True if the animation is starting
        /// </summary>
        public bool starting = true;
        /// <summary>
        /// The time multiplier. Used to make the animation go forward or backwards.
        /// </summary>
        public float timeMult;

        /// <summary>
        /// Constructs a new RunningAnimation object.
        /// </summary>
        /// <param name="animation">The animation</param>
        /// <param name="type">The animation type</param>
        /// <param name="speed">The speed of the animation</param>
        public RunningAnimation(Animation animation, AnimationType type, float speed)
        {
            this.animation = animation;
            this.type = type;
            this.speed = speed;

            switch (type)
            {
                case (AnimationType.ForwardStop):
                    {
                        timeMult = 1.0f;
                        break;
                    }
                case (AnimationType.ForwardLoop):
                    {
                        timeMult = 1.0f;
                        break;
                    }
                case (AnimationType.ForwardBackwardStop):
                    {
                        timeMult = 1.0f;
                        break;
                    }
                case (AnimationType.ForwardBackwardLoop):
                    {
                        timeMult = 1.0f;
                        break;
                    }
                case (AnimationType.BackwardStop):
                    {
                        timeMult = -1.0f;
                        break;
                    }
                case (AnimationType.BackwardLoop):
                    {
                        timeMult = -1.0f;
                        break;
                    }
                case (AnimationType.BackwardForwardStop):
                    {
                        timeMult = -1.0f;
                        break;
                    }
                case (AnimationType.BackwardForwardLoop):
                    {
                        timeMult = -1.0f;
                        break;
                    }
                case (AnimationType.ForwardHalt):
                    {
                        timeMult = 1.0f;
                        break;
                    }
                case (AnimationType.BackwardHalt):
                    {
                        timeMult = -1.0f;
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// The Skeleton class is used for skeletal animations. 
    /// The Skeleton class contains a array of Bones which are animated. The output of every animated bone is stored in this class.
    /// </summary>
    public class Skeleton
    {
        #region variables
        /// <summary>
        /// The bones of this skeleton
        /// </summary>
        public readonly Bone[] bones;
        /// <summary>
        /// The absolute transforms of every bone
        /// </summary>
        public readonly Matrix4[] absoluteJointMatrices;
        /// <summary>
        /// The animated transform of every bone
        /// </summary>
        public readonly Matrix4[] finalJointMatrices;
        /// <summary>
        /// The inverse transform of every bone (equals inverse(absoluteTransform))
        /// </summary>
        public readonly Matrix4[] invJointMatrices;

        private List<RunningAnimation> runningAnimations = new List<RunningAnimation>();
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new Skeleton with the given bones
        /// </summary>
        /// <param name="bones">The bones of this skeleton. These bones can be used by more than one Skeleton object.</param>
        public Skeleton(Bone[] bones)
        {
            this.bones = bones;

            this.absoluteJointMatrices = new Matrix4[bones.Length];
            this.finalJointMatrices = new Matrix4[bones.Length];
            this.invJointMatrices = new Matrix4[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                Bone bone = bones[i];

                // set default transforms
                if (bone.parentIndex != -1)
                    this.absoluteJointMatrices[i] = bone.localTransform * this.absoluteJointMatrices[bone.parentIndex];
                else
                    this.absoluteJointMatrices[i] = bone.localTransform;

                this.finalJointMatrices[i] = this.absoluteJointMatrices[i];
                Matrix4 invAbsoluteTransform = this.absoluteJointMatrices[i];
                invAbsoluteTransform.Invert();
                this.invJointMatrices[i] = invAbsoluteTransform;
            }
        }
        #endregion

        #region Skeleton methods
        /// <summary>
        /// Starts the given animation
        /// </summary>
        /// <param name="animation">The animation to start</param>
        /// <param name="type">The animation type</param>
        /// <param name="speed">The speed of the animation. The timestep is divided by this value, so an animation will go twice as slow if this value is 2.</param>
        public void StartAnimation(Animation animation, AnimationType type, float speed)
        {
            StartAnimation(animation, type, speed, false);
        }
        /// <summary>
        /// Starts the given animation
        /// </summary>
        /// <param name="animation">The animation to start</param>
        /// <param name="type">The animation type</param>
        /// <param name="speed">The speed of the animation. The timestep is divided by this value, so an animation will go twice as slow if this value is 2.</param>
        /// <param name="skipStartup">Skip the start of the animation, meaning the weight of this animation is 1 from start.</param>
        public void StartAnimation(Animation animation, AnimationType type, float speed, bool skipStartup)
        {
            if (animation == null)
                return;

            RunningAnimation running = new RunningAnimation(animation, type, speed);
            if (skipStartup)
            {
                running.starting = false;
                running.animWeight = 1.0f;
            }

            this.runningAnimations.Add(running);
        }

        /// <summary>
        /// Stops the given animation. If multiple animation of the same type are started only one is stopped.
        /// </summary>
        /// <param name="animation">The animation to stop</param>
        public void StopAnimation(Animation animation)
        {
            if (animation == null)
                return;

            RunningAnimation linkedAnim = null;
            foreach (RunningAnimation anim in this.runningAnimations)
            {
                if (anim.animation == animation && !anim.finished)
                {
                    linkedAnim = anim;
                    break;
                }
            }
            if (linkedAnim != null)
            {
                linkedAnim.finished = true;
            }
        }

        /// <summary>
        /// Stops all running animations
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (RunningAnimation running in this.runningAnimations)
                running.finished = true;
        }

        /// <summary>
        /// Updates any animations currently active by the given timestep.
        /// </summary>
        /// <param name="timeStep"></param>
        public void Update(float timeStep)
        {
            // update running animations time. Also remove or reset animations if needed
            for (int i = 0; i < this.runningAnimations.Count; i++)
            {
                RunningAnimation running = this.runningAnimations[i];

                if (running.finished)
                {
                    running.animWeight -= timeStep * 10;
                    if (running.animWeight <= 0.0f)
                    {
                        this.runningAnimations.Remove(running);
                        i--;
                    }
                }
                else
                {
                    if (running.starting)
                    {
                        running.animWeight += timeStep * 10;
                        if (running.animWeight >= 1.0f)
                        {
                            running.animWeight = 1.0f;
                            running.starting = false;
                        }
                    }

                    running.time += (timeStep / running.speed) * running.timeMult;
                    CheckAnimationTime(running);
                }
            }

            // update bone transforms
            foreach (Bone bone in this.bones)
                bone.ProcessAnimations(this.runningAnimations, this);
        }

        private void CheckAnimationTime(RunningAnimation running)
        {
            switch (running.type)
            {
                case (AnimationType.ForwardStop):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                            running.finished = true;
                        }
                        break;
                    }
                case (AnimationType.ForwardLoop):
                    {
                        if (running.time > running.animation.totalTime)
                            running.time = running.time % running.animation.totalTime;
                        break;
                    }
                case (AnimationType.ForwardHalt):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                        }
                        break;
                    }
                case (AnimationType.ForwardBackwardStop):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                            running.timeMult = -1.0f;
                        }
                        else if(running.time < 0.0f)
                        {
                            running.time = 0.0f;
                            running.finished = true;
                        }
                        break;
                    }
                case (AnimationType.ForwardBackwardLoop):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                            running.timeMult = -1.0f;
                        }
                        else if (running.time < 0.0f)
                        {
                            running.time = 0.0f;
                            running.timeMult = 1.0f;
                        }
                        break;
                    }
                case (AnimationType.BackwardStop):
                    {
                        if (running.time < 0.0f)
                            running.finished = true;
                        break;
                    }
                case (AnimationType.BackwardLoop):
                    {
                        if (running.time < 0.0f)
                        {
                            running.time = running.animation.totalTime - running.time;
                        }
                        break;
                    }
                case (AnimationType.BackwardHalt):
                    {
                        if (running.time < 0.0f)
                        {
                            running.time = 0.0f;
                        }
                        break;
                    }
                case (AnimationType.BackwardForwardStop):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                            running.finished = true;
                        }
                        else if (running.time < 0.0f)
                        {
                            running.time = 0.0f;
                            running.timeMult = 1.0f;
                        }
                        break;
                    }
                case (AnimationType.BackwardForwardLoop):
                    {
                        if (running.time > running.animation.totalTime)
                        {
                            running.time = running.animation.totalTime;
                            running.timeMult = -1.0f;
                        }
                        else if (running.time < 0.0f)
                        {
                            running.time = 0.0f;
                            running.timeMult = 1.0f;
                        }
                        break;
                    }
            }
        }
        #endregion
    }
}
