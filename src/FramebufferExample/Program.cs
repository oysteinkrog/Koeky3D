using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using Koeky3D.BufferHandling;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using Koeky3D.Textures;
using System.Drawing;
using OpenTK.Graphics;

namespace FramebufferExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (ExampleWindow window = new ExampleWindow())
            {
                window.Run(60.0);
            }
        }
    }
}
