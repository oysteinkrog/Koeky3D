using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shapes;
using Koeky3D.Models;
using Koeky3D.Particles;

namespace Koeky3D.Pipeline
{
    /// <summary>
    /// Provides acces to graphic resources using an interface
    /// </summary>
    public interface IDataPipeline
    {
        void GetMeshes(ViewFrustum frustum, List<GLMesh> outList);
        void GetParticles(ViewFrustum frustum, List<ParticleEmitter> emitters);
    }
}
