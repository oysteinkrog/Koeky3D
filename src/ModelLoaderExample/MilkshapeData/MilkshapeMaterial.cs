using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;

namespace MilkModelLoader.MilkshapeData
{
    public class MilkshapeMaterial
    {
        public String name;
        public Color4 ambient, diffuse, specular, emissive;
        public float shininess, transparancy;

        public String texturePath, alphaMapPath;
    }
}
