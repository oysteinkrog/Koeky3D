using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLFramework.Models;
using GLFramework.Lighting;
using GLFramework.Shapes;

namespace GLFramework
{
    public interface IGraphicsWorld
    {
        void AddStaticTransform(IStaticTransform staticTransform);
        void RemoveStaticTransform(IStaticTransform staticTransform);
        List<IStaticTransform> GetStaticTransforms(ViewFrustum frustum);

        void AddSkinnedTransform(ISkinnedTransform skinnedTransform);
        void RemoveSkinnedTransform(ISkinnedTransform skinnedTransform);
        List<ISkinnedTransform> GetSkinnedTransform(ViewFrustum frustum);

        void AddPointLight(PointLight pointLight);
        void RemovePointLight(PointLight pointLight);
        List<PointLight> GetPointLights(ViewFrustum frustum);

        void AddSpotLight(SpotLight spotLight);
        void RemoveSpotLight(SpotLight spotLight);
        List<SpotLight> GetSpotLights(ViewFrustum frustum);
    }
}
