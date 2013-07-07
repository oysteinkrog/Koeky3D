using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shapes;
using Koeky3D.Models;

namespace Koeky3D.Pipeline
{
    /// <summary>
    /// Provides a basic rendering pipeline for graphics.
    /// You could, for example, split up the rendering of the terrain and models in seperate stages.
    /// This class helps in that regard.
    /// </summary>
    public class GLRenderPipeline
    {
        private List<IRenderStage> stages = new List<IRenderStage>();
        private Dictionary<String, object> renderCache = new Dictionary<String, object>();

        /// <summary>
        /// Constructs an empty GLRenderPipeline
        /// </summary>
        public GLRenderPipeline()
        {

        }
        /// <summary>
        /// Constucts a GLRenderPipeline
        /// </summary>
        /// <param name="stages">The stages to add</param>
        public GLRenderPipeline(List<IRenderStage> stages)
        {
            this.stages = stages;
        }

        /// <summary>
        /// Executes all stages in this rendering pipeline.
        /// </summary>
        /// <param name="frustum">The view frustum</param>
        /// <param name="glManager">The opengl manager to use</param>
        /// <param name="renderOptions">The render options</param>
        public void DoStages(ViewFrustum frustum, GLManager glManager, RenderOptions renderOptions)
        {
            // TODO: destroy the current render cache


            foreach (IRenderStage stage in this.stages)
                stage.DoStage(this, frustum, glManager, renderOptions);
        }

        /// <summary>
        /// Stores a render result, such as a texture, in the render cache
        /// </summary>
        /// <param name="key">The key or id of the result</param>
        /// <param name="value">The render result</param>
        public void RegisterRenderResult(String key, object value)
        {
            this.renderCache.Add(key, value);
        }
        /// <summary>
        /// Retrieves a render result
        /// </summary>
        /// <typeparam name="T">The type of the render result</typeparam>
        /// <param name="key">The name of the render result</param>
        /// <returns>The render result</returns>
        public T GetRenderResult<T>(String key)
        {
            return (T)this.renderCache[key];
        }

        /// <summary>
        /// Adds a render stage
        /// </summary>
        /// <param name="renderStage">The render stage to add</param>
        public void AddRenderStage(IRenderStage renderStage)
        {
            this.stages.Add(renderStage);
        }
        /// <summary>
        /// Adds a render stage at the given index.
        /// If the index does not yet exists (for example: there are 2 render stages and you try to add one at index 4) 
        /// an IndexOutOfRangeException is thrown.
        /// </summary>
        /// <param name="renderStage"></param>
        /// <param name="index"></param>
        public void AddRenderStage(IRenderStage renderStage, int index)
        {
            this.stages.Insert(index, renderStage);
        }
        /// <summary>
        /// Removes the given render stage
        /// </summary>
        /// <param name="renderStage"></param>
        public void RemoveRenderStage(IRenderStage renderStage)
        {
            this.stages.Remove(renderStage);
        }
    }
}
