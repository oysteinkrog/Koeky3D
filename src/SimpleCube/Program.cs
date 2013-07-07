using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SimpleCube
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (SimpleCubeWindow window = new SimpleCubeWindow())
            {
                window.Run();
            }
        }
    }
}
