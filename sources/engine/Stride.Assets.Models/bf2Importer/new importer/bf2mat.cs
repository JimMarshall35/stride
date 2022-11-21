using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public enum Bf2AlphaMode
    {
        Opaque,
        Blend,
        AlphaTest,
        Unknown
    }
    /// <summary>
    /// bf2 lod material (drawcall)
    /// </summary>
    public class bf2mat
    {
        private bool IsBumpMap(string path) =>
                   (path.Contains("_b.")
                   || path.Contains("_n.")
                   || path.Contains("_b_")
                   || path.Contains("_n_")
                   || path.Contains("_deb.")
                   || path.Contains("_crb.")
                   || path.Contains("deb_")
                   || path.Contains("_crb_"));
        public void PopulateShaderInfo()
        {
            facenum = inum / 3;
            if (mapnum > 0)
            {
                texmapid = new uint[mapnum];
                mapuvid = new uint[mapnum];
                isBumpMap = new bool[mapnum];
            }
            for (int i = 0; i < mapnum; i++)
            {
                isBumpMap[i] = IsBumpMap(map[i]);
            }
            switch (fxfile.ToLower())
            {
                case "skinnedmesh.fx":
                    // TODO: fill these out accordin to vb code in modVisMeshShader.bas - this lays out in detail how the material system works
                    break;
                case "bundledmesh.fx":
                    break;
                case "staticmesh.fx":
                    break;
                
            }
        }
        /// <summary>
        /// 0=opaque, 1=blend, 2=alphatest
        /// </summary>
        public uint alphamode;

        /// <summary>
        /// shader filename string
        /// </summary>
        public string fxfile;

        /// <summary>
        /// technique name
        /// </summary>
        public string technique;

        public uint mapnum;

        /// <summary>
        /// texture map filenames
        /// </summary>
        public string[] map;

        //geometry info

        /// <summary>
        /// vertex start offset
        /// </summary>
        public uint vstart;

        /// <summary>
        /// index start offset
        /// </summary>
        public uint istart;

        /// <summary>
        /// number of indices
        /// </summary>
        public uint inum;

        /// <summary>
        /// number of vertices
        /// </summary>
        public uint vnum;

        //unknown

        /// <summary>
        /// 0
        /// </summary>
        public uint u4;

        /// <summary>
        /// 0
        /// </summary>
        public uint u5;

        //per-material bounds (staticmesh only)
        public bf2Vec3 mmin;
        public bf2Vec3 mmax;

        #region internal

        /// <summary>
        /// inum / 3
        /// </summary>
        public uint facenum;

        /// <summary>
        /// texmap[] index
        /// </summary>
        public uint[] texmapid;

        /// <summary>
        /// if map is bump map
        /// </summary>
        public bool[] isBumpMap;

        /// <summary>
        /// UV index for each map
        /// </summary>
        public uint[] mapuvid;

        public uint layernum;
        public mat_layer[] layer = new mat_layer[4];
        public uint glslprog;
        public bool hasBump;
        public bool hasWreck;
        public bool hasAnimatedUV;
        public bool hasBumpAlpha;
        public bool hasDetail;
        public bool hasDirt;
        public bool hasCrack;
        public bool hasCrackN;
        public bool hasDetaiN;
        public bool hasEnvMap;
        public float alphaTest;
        public bool twosided;

        /// <summary>
        /// internal first vertex
        /// </summary>
        public uint vertStart;

        /// <summary>
        /// internal last vertex
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

        /// <summary>
        /// unique hash
        /// </summary>
        public uint hash;

        /// <summary>
        /// UV editor material name
        /// </summary>
        string shortname;

        #endregion

        public Bf2AlphaMode AlphaModeDescription
        {
            get
            {
                switch (alphamode)
                {
                    case 0u:
                        return Bf2AlphaMode.Opaque;
                    case 1u:
                        return Bf2AlphaMode.Blend;
                    case 2u:
                        return Bf2AlphaMode.AlphaTest;
                    default:
                        return Bf2AlphaMode.Unknown;
                }
            }
        }
    }
}
