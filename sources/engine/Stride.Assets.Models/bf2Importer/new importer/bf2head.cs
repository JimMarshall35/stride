using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// bf2 mesh file header
    /// 20 bytes
    /// </summary>
    public class bf2head
    {
        /// <summary>
        /// 0
        /// </summary>
        public uint u1 { get; private set; }

        /// <summary>
        /// 10 for most bundledmesh, 6 for some bundledmesh, 11 for staticmesh
        /// </summary>
        public uint version { get; private set; }

        /// <summary>
        /// 0
        /// </summary>
        public uint u3 { get; private set; }

        /// <summary>
        /// 0
        /// </summary>
        public uint u4 { get; private set; }

        /// <summary>
        /// 0
        /// </summary>
        public uint u5 { get; private set; }

        public bf2head(Stream s)
        {
            u1 = StreamHelpers.ReadU32(s);
            version = StreamHelpers.ReadU32(s);
            u3 = StreamHelpers.ReadU32(s);
            u4 = StreamHelpers.ReadU32(s);
            u5 = StreamHelpers.ReadU32(s);
        }
    }
}
