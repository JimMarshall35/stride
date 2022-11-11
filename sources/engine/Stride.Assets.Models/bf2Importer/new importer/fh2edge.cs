using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public class fh2edge
    {
        /// <summary>
        /// geom index
        /// </summary>
        public byte geom;

        /// <summary>
        /// 'LOD index
        /// </summary>
        public byte lod;

        /// <summary>
        /// 'LOD material index
        /// </summary>
        public byte mat;

        /// <summary>
        /// 'selection
        /// </summary>
        public byte sel;

        /// <summary>
        /// selecton mask
        /// </summary>
        public byte flag;

        /// <summary>
        /// vertex index 1
        /// </summary>
        public int v1;

        /// <summary>
        /// vertex index 2
        /// </summary>
        public int v2;

        /// <summary>
        /// face index 1
        /// </summary>
        public int f1;

        /// <summary>
        /// face index 2
        /// </summary>
        public int f2;

        /// <summary>
        /// element ID
        /// </summary>
        public int elem;
    }
}
