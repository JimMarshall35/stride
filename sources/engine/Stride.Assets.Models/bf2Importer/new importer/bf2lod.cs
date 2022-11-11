using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public class bf2lod
    {
        /// <summary>
        /// bounds min
        /// </summary>
        public bf2Vec3 min;

        /// <summary>
        /// bounds max
        /// </summary>
        public bf2Vec3 max;

        /// <summary>
        /// not sure this is really a pivot (only on version<=6)
        /// </summary>
        public bf2Vec3 pivot;

        // skinning matrices (skinnedmesh only)

        /// <summary>
        /// this corresponds to matnum
        /// </summary>
        public uint rignum;
        public bf2rig[] rig;

        // nodes (staticmesh and bundledmesh only)
        public uint nodenum;
        public bf2Mat4x4[] node;

        // material groups
        public uint matnum;
        public bf2mat[] mat;

        #region internal

        /// <summary>
        /// number of triangles
        /// </summary>
        public uint polycount;

        /// <summary>
        /// internal first vertex index
        /// </summary>
        public uint vertStart;

        /// <summary>
        /// internal last vertex index
        /// </summary>
        public uint vertEnd;

        /// <summary>
        /// internal first face index
        /// </summary>
        public uint faceStart;

        /// <summary>
        /// internal last face index
        /// </summary>
        public uint faceEnd;

        #endregion
    }
}
