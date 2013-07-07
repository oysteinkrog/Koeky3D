using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Koeky3D.Utilities
{
    /// <summary>
    /// Provides some default OpenGL functionality
    /// </summary>
    public static class GLMethods
    {
        /// <summary>
        /// Prints all OpenGL errors in the console
        /// </summary>
        public static void PrintErrors()
        {
            PrintErrors("");
        }
        /// <summary>
        /// Prints all OpenGL errors in the console
        /// </summary>
        /// <param name="prefix">The prefix for every error</param>
        public static void PrintErrors(String prefix)
        {
            ErrorCode error = GL.GetError();
            if (error == ErrorCode.NoError)
                return;

            Console.WriteLine("----OPENGL ERRORS " + prefix + "----");
            while (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                error = GL.GetError();
            }
            Console.WriteLine("--------");
        }
        /// <summary>
        /// Checks if there are OpenGL errors and throws an exception if an error was found.
        /// </summary>
        /// <param name="message">The message to give to the exception</param>
        public static void CheckErrors(String message)
        {
            ErrorCode error = GL.GetError();
            while (error != ErrorCode.NoError)
                throw new Exception("OpenGL Error: " + error.ToString() + " - " + message);
        }
        /// <summary>
        /// Clears all current opengl errors
        /// </summary>
        /// <returns>True if errors where found</returns>
        public static bool ClearErrors()
        {
            ErrorCode error = GL.GetError();
            bool errorFound = false;
            while (error != ErrorCode.NoError)
                errorFound = true;

            return errorFound;
        }
    }
}
