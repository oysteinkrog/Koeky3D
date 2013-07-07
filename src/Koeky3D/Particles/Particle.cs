using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Particles
{
    class Particle
    {
        private static Random rand = new Random();

        public Vector3 moveVelocityStart;
        public float angleStart;
        public float scaleStart;
        public Vector4 transparancyStart;

        public Vector3 moveVelocity;
        public Vector3 gravity;
        public float angularVelocity = 0.0f;
        public float scaleIncrease = 0.0f;
        public Vector4 transparancyIncrease;
        public float maxLifeTime;
        public float lifeTime;

        public Particle(Vector3 minMoveVelocity, Vector3 maxMoveVelocity,
                        float minAngularVelocity, float maxAngularVelocity, float startAngle,
                        float minLifeTime, float maxLifeTime,
                        float minScaleIncrease, float maxScaleIncrease, float startScale,
                        Vector4 transparancyIncrease, Vector4 startTransparancy,
                        Vector3 gravity, float lifeOffset)
        {
            this.moveVelocityStart = minMoveVelocity + ((maxMoveVelocity - minMoveVelocity) * (float)rand.NextDouble());

            this.angularVelocity = minAngularVelocity + ((maxAngularVelocity - minAngularVelocity) * (float)rand.NextDouble());
            this.angleStart = startAngle;

            this.maxLifeTime = minLifeTime + ((maxLifeTime - minLifeTime) * (float)rand.NextDouble());

            this.scaleIncrease = minScaleIncrease + ((maxScaleIncrease - minScaleIncrease) * (float)rand.NextDouble());
            this.scaleStart = startScale;

            this.transparancyIncrease = transparancyIncrease;
            this.transparancyStart = startTransparancy;

            this.gravity = gravity;
            this.lifeTime = lifeOffset;
        }
    }
}
