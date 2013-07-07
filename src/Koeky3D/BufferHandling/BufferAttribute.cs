using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koeky3D.BufferHandling
{
    /// <summary>
    /// Contains default enumarations for commonly used buffer attributes
    /// </summary>
    public enum BufferAttribute : int
    {
        Vertex = 0,
        TexCoord = 1,
        Normal = 2,
        BoneIndex = 3,
        BoneWeight = 4,
        ParticleTextureAlpha = 5,
        ParticleAngle = 6,
        ParticleScale = 7,
        ParticleVertex = 8,
        ParticleAlpha = 9,
        ParticleVelocity = 10,
        ParticleLifeTime = 11,
        ParticleMaxLifeTime = 12,
        Tangent = 13,
        TextureAlphas = 14,
        Color = 15,
        LowTexCoord = 16
    }
}
