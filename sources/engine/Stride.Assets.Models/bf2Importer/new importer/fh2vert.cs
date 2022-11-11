using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// internal vertex info (helper structure generated after load time)
    /// </summary>
    public class fh2vert
    {
        /// <summary>
        /// geom index
        /// </summary>
        public byte geom;
        /// <summary>
        /// LOD index
        /// </summary>
        public byte lod;

        /// <summary>
        /// LOD material index
        /// </summary>
        public byte mat;

        /// <summary>
        /// selection
        /// </summary>
        public byte sel;

        /// <summary>
        /// selection mask flag
        /// </summary>
        public byte flag;

        /// <summary>
        /// set if face selected
        /// </summary>
        public byte facesel;

        //'tf As float3        'transformed position(TODO: use this to replace vmesh.skinpos())
        /// <summary>
        /// screen space projected vertex
        /// </summary>
        public bf2Vec3 sv;

        /// <summary>
        /// index of first vertex of this LOD we share position with
        /// </summary>
        public int sharepos;

        /// <summary>
        /// index of first vertex of this LOD we share position+normal with
        /// </summary>
        public int sharenorm;

        /// <summary>
        /// index of first vertex of this LOD we share position+normal+tangent with
        /// </summary>
        public int sharetang;

        /// <summary>
        /// element ID
        /// </summary>
        public int elem;
    }
}
