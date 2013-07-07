using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MilkModelLoader.MilkshapeData
{
    class MilkshapeGroup
    {
        public String name;
        public MilkshapeTriangle[] triangles;

        // This byte is used because milkshape first defines
        // the groups and then the materials. So we use this to reference the material later
        public byte materialIndex;
        public MilkshapeMaterial material;
    }
}
