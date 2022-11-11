using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// bf2 BundledMesh vertex weight (helper structure, memcopy float to this)
    /// </summary>
    public class bf2vw
    {
        /// <summary>
        /// bone 1 index
        /// </summary>
        public byte b1;

        /// <summary>
        /// bone 2 index
        /// </summary>
        public byte b2;

        /// <summary>
        /// weight for bone 1
        /// </summary>
        public byte w1;

        /// <summary>
        /// weight for bone 2
        /// </summary>
        public byte w2;
    }
}
