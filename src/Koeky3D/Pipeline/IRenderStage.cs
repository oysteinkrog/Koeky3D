using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shapes;

namespace Koeky3D.Pipeline
{
    /// <summary>
    /// Provides the implementation for a render stage.
    /// A render stage is a specific high level part of the rendering pipeline.
    /// Examples of a render stage are:
    ///     rendering a terrain,
    ///     rendering a skybox or
    ///     rendering a collection of 3d models
    /// </summary>
    public interface IRenderStage
    {
        void DoStage(GLRenderPipeline pipeLine, ViewFrustum frustum, GLManager glManager, RenderOptions renderOptions);
    }
}
