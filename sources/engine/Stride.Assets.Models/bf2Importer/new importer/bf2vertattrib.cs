using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// bf2 vertex attribute table entry
    /// </summary>
    public class bf2vertattrib
    {
        /// <summary>
        /// some sort of boolean flag (if true the below field are to be ignored?)
        /// </summary>
        public ushort flag;

        /// <summary>
        /// some sort of boolean flag (if true the below field are to be ignored?)
        /// </summary>
        public ushort offset;

        /// <summary>
        /// attribute type (vec2, vec3 etc)
        /// </summary>
        public ushort vartype;

        /// <summary>
        /// usage ID (vertex, texcoord etc)
        /// 
        /// Note: "usage" field correspond to the definition in DX SDK "Include\d3d9types.h"
        /// It looks like DICE extended these for additional UV channels, these
        /// constants are much larger so they don't conflict with other DX enums.
        /// </summary>
        public ushort usage;

        public VertexUsage Usage
        {
            get => Enum.GetValues(typeof(VertexUsage))
                .Cast<VertexUsage>()
                .Where(x => x != VertexUsage.UNKNOWNTYPE)
                .First(x => (ushort)x == usage);
        }

        public VertexType VertType
        {
            get => vartype < (ushort)VertexType.NUMVALS ? (VertexType)vartype : VertexType.UNKNOWNTYPE;
        }

        public bf2vertattrib(Stream s)
        {
            flag = StreamHelpers.ReadU16(s);
            offset = StreamHelpers.ReadU16(s);
            vartype = StreamHelpers.ReadU16(s);
            usage = StreamHelpers.ReadU16(s);
        }
    }
}
