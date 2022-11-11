using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public enum VertexUsage
    {
        POSITION = 0,
        BLENDWEIGHT,   // 1
        BLENDINDICES,  // 2
        NORMAL,        // 3
        PSIZE,         // 4
        TEXCOORD1,      // 5
        TANGENT,       // 6
        TEXCOORD2 = 261,
        TEXCOORD3 = 517,
        TEXCOORD4 = 773,
        TEXCOORD5 = 1029,
        UNKNOWNTYPE
    }

    public enum VertexType
    {
        FLOAT1 = 0,
        FLOAT2,
        FLOAT3,
        FLOAT4,
        D3DCOLOR,
        UBYTE4,
        SHORT2,
        SHORT4,
        NUMVALS,
        UNKNOWNTYPE
    }
}
