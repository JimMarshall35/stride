using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Rendering;

namespace Stride.Assets.Models.bf2Importer
{
    internal class StrideBf2MeshInfo
    {
        public StrideBf2MeshInfo(MeshDraw draw, int materialIndex)
        {
            Draw = draw;
            MaterialIndex = materialIndex;
        }
        public MeshDraw Draw { get; private set; }
        public int MaterialIndex { get; private set; }
    }
}
