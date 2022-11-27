using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// internal struct, we use this for our approximate shading
    /// </summary>
    public class mat_layer
    {
        /// <summary>
        /// index of texmap
        /// </summary>
        public string texmapFilename;

        /// <summary>
        /// ...
        /// </summary>
        public int texcoff;

        /// <summary>
        /// blending
        /// </summary>
        public bool blend;

        /// <summary>
        /// blend source factor
        /// </summary>
        public uint blendsrc;

        /// <summary>
        /// blend destination factor
        /// </summary>
        public uint blenddst;

        /// <summary>
        /// alpha testing
        /// </summary>
        public bool alphaTest;

        /// <summary>
        /// alpha cutoff value [0-1]
        /// </summary>
        public float alpharef;

        /// <summary>
        /// depth testing function
        /// </summary>
        public DepthFunc depthfunc;

        /// <summary>
        /// write to z-buffer
        /// </summary>
        public bool depthWrite;

        /// <summary>
        /// two-sided
        /// </summary>
        public bool twosided;

        /// <summary>
        /// FFP lighting
        /// </summary>
        public bool lighting;
    }
}
