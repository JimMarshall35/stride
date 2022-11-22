using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
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
    public enum BlendDFactor : uint// https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBlendFunc.xhtml
    {
        GL_ZERO = 0,
        GL_ONE = 1,
        GL_SRC_COLOR = 0x0300,
        GL_ONE_MINUS_SRC_COLOR = 0x0301,
        GL_SRC_ALPHA = 0x0302,
        GL_ONE_MINUS_SRC_ALPHA = 0x0303,
        GL_DST_ALPHA = 0x0304,
        GL_ONE_MINUS_DST_ALPHA = 0x0305,
        GL_DST_COLOR = 0x0306,
        GL_ONE_MINUS_DST_COLOR = 0x0307,
        GL_SRC_ALPHA_SATURATE = 0x0308 // exact values don't really matter for our purposes
    }

    /// <summary>
    /// bf2 lod material (drawcall)
    /// </summary>
    public class bf2mat
    {
        private readonly string _assetFileName;
        public bf2mat(string assetFileName)
        {
            _assetFileName = assetFileName;
        }

        private void SetBase(int i)
        {
            layer[i].texcoff = 0;
            layer[i].texmapid = (int)texmapid[0]; //where set?
            layer[i].depthfunc = DepthFunc.GL_LESS;
            layer[i].depthWrite = true;
            layer[i].lighting = false;
            layer[i].blend = false;
            layer[i].alphaTest = false;
            switch (alphamode)
            {
                case 1:
                    layer[i].blend = false;
                    layer[i].blendsrc = (uint) BlendDFactor.GL_SRC_ALPHA;
                    layer[i].blenddst = (uint)BlendDFactor.GL_ONE_MINUS_SRC_ALPHA;
                    layer[i].depthWrite = false;
                    break;
                case 2:
                    layer[i].alphaTest = true;
                    layer[i].alpharef = 0.5f;
                    break;
            }
        }

        private void SetAlphaTest(int i)
        {
            layer[i].texcoff = 0;
            layer[i].texmapid = (int)texmapid[0];
            layer[i].depthfunc = DepthFunc.GL_LESS;
            layer[i].depthWrite = true;
            layer[i].alphaTest = true;
            layer[i].alpharef = 0.5f;
            layer[i].lighting = false;
        }

        private void SetAlpha(int i)
        {
            layer[i].texcoff = 0;
            layer[i].texmapid = (int)texmapid[0];
            layer[i].depthfunc = DepthFunc.GL_LESS;
            layer[i].depthWrite = true;
            layer[i].blend = true;
            layer[i].blendsrc = (uint)BlendDFactor.GL_SRC_ALPHA;
            layer[i].blenddst = (uint)BlendDFactor.GL_ONE_MINUS_SRC_ALPHA;
            layer[i].lighting = false;

        }

        private void SetDetail(int i)
        {
            layer[i].texcoff = 1;
            layer[i].texmapid = (int)texmapid[1];
            layer[i].depthfunc = DepthFunc.GL_EQUAL;
            layer[i].depthWrite = false;
            layer[i].blend = true;
            layer[i].blendsrc = (uint)BlendDFactor.GL_ZERO;
            layer[i].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;
            layer[i].lighting = false;
        }

        private void SetDirt(int i)
        {
            layer[i].texcoff = 2;
            layer[i].texmapid = (int)texmapid[2];
            layer[i].depthfunc = DepthFunc.GL_EQUAL;
            layer[i].depthWrite = false;
            layer[i].blend = true;
            layer[i].blendsrc = (uint)BlendDFactor.GL_ZERO;
            layer[i].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;
            layer[i].lighting = false;
        }

        private void SetCrack(int i)
        {
            layer[i].texcoff = 3;
            layer[i].texmapid = (int)texmapid[3];
            layer[i].depthfunc = DepthFunc.GL_EQUAL;
            layer[i].depthWrite = false;
            layer[i].blend = true;
            layer[i].blendsrc = (uint)BlendDFactor.GL_SRC_ALPHA;
            layer[i].blenddst = (uint)BlendDFactor.GL_ONE_MINUS_SRC_ALPHA;
            layer[i].lighting = true;
        }

        /// <summary>
        /// ORIGINAL COMMENT: swaps base (layer 1) and detail (layer 2) in case of alpha
        /// </summary>
        private void MakeAlpha()
        {
            if(alphamode == 2)
            {
                int tmp;
                tmp = layer[1].texmapid;
                layer[1].texmapid = layer[2].texmapid;
                layer[2].texmapid = tmp;

                layer[1].texcoff = 1;
                layer[2].texcoff = 0;

                layer[1].texmapid = (int)texmapid[1];
                layer[2].texmapid = (int)texmapid[0];

                layer[1].depthfunc = DepthFunc.GL_LESS;
                layer[2].depthfunc = DepthFunc.GL_EQUAL;

                layer[1].depthWrite = true;
                layer[2].depthWrite = true;

                layer[1].blend = false;
                layer[2].blend = true;
                layer[2].blendsrc = (uint)BlendDFactor.GL_ZERO;
                layer[2].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;

                layer[1].alphaTest = true;
                layer[1].alpharef = 0.5f;
            }
        }

        private bool IsBumpMap(string path) =>
                   (path.Contains("_b.")
                   || path.Contains("_n.")
                   || path.Contains("_b_")
                   || path.Contains("_n_")
                   || path.Contains("_deb.")
                   || path.Contains("_crb.")
                   || path.Contains("deb_")
                   || path.Contains("_crb_"));

        private string GetCleanMapName(string fname) => Path.GetFileName(fname)
                .Replace(".dds", ".")
                .Replace(".tga", ".")
                .Replace("_c.", "")
                .Replace("_de.", "")
                .Replace("_di.", "")
                .Replace("_cr.", "")
                .Replace("_deb.", "")
                .Replace("_crb.", "")
                .Replace(".", "");

        /// <summary>
        /// a super long function ported over verbatim from the vb function BuildShader in modVisMeshShader.bas
        /// quirky use of indicies - TODO: verify intended behavior - possibly quick unit test then use to implement ExtractEntity.
        /// Look if any more code needs porting over - some definitely does eventually see todo comment further down.
        /// </summary>
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
            var tech = string.Empty;
            switch (fxfile.ToLower())
            {
                case "skinnedmesh.fx":
                    if (mapnum > 0) shortname = GetCleanMapName(map[0]);
                    tech = technique.ToLower();
                    layernum = 1;
                    hasBump = true; // not sure why the vb code sets this to true
                    hasWreck = false;
                    if (tech == "alpha_test")
                    {
                        SetAlphaTest((int)layernum);
                    }
                    else
                    {
                        SetBase((int)layernum);
                    }
                    break;
                case "bundledmesh.fx":
                    if (mapnum > 0) shortname = GetCleanMapName(map[0]);
                    if(mapnum == 3)
                    {
                        if (map[0].Contains("SpecularLUT"))
                        {
                            hasBump = false;
                        }
                        else
                        {
                            hasBump = true;
                        }
                    }
                    if(mapnum == 4)
                    {
                        hasBump = true;
                        hasWreck = true;
                    }
                    if (technique.Contains("AnimatedUV"))
                    {
                        hasAnimatedUV = true;
                    }
                    if (technique.Contains("envmap"))
                    {
                        hasEnvMap = true;
                        //LoadEnvMap <- TODO: Look at and port this func call if necessary
                    }
                    // ORIGINAL COMMENT: "alpha in bumpmap, dunno how BF2 detects this2
                    if(alphamode > 0 && technique.ToLower() == "alpha_testcolormapgloss")
                    {
                        hasBumpAlpha = true;
                    }
                    if (alphamode > 0 && technique.ToLower() == "colormapglossalpha_test")
                    {
                        hasBumpAlpha = true;
                    }

                    layernum = 1;
                    SetBase(1);
                    //wreck (no bump)
                    if (mapnum == 3)
                    {
                        //todo: implment from here down see modVisMeshShader.bas
                        layer[1].depthWrite = true;

                        layernum = 2;
                        layer[2].texcoff = 0;
                        layer[2].texmapid = (int)texmapid[2]; // i think this variable name is refering to an opengl texture ID
                        layer[2].depthfunc = DepthFunc.GL_EQUAL;
                        layer[2].depthWrite = false;
                        if (alphamode == 1) {
                            layer[2].depthfunc = DepthFunc.GL_EQUAL;// ORIGINAL COMMENT: note: does not render correctly, but we don't care
                        }
                        if (alphamode == 2) {
                            layer[2].depthfunc = DepthFunc.GL_EQUAL;
                        }

                        layer[2].blend = true;
                        layer[2].blendsrc = (uint)BlendDFactor.GL_ZERO;
                        layer[2].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;
                        layer[2].lighting = false;
                    }

                    if (mapnum == 4)
                    {
                        layer[1].depthWrite = true;

                        layernum = 2;
                        layer[2].texcoff = 0;
                        layer[2].texmapid = (int)texmapid[3];
                        layer[2].depthfunc = DepthFunc.GL_EQUAL;
                        layer[2].depthWrite = false;
                        if (alphamode == 1)
                        {
                            layer[2].depthfunc = DepthFunc.GL_EQUAL; //ORIGINAL COMMENT: note: does not render correctly, but we don't care
                        }
                        if (alphamode == 2)
                        {
                            layer[2].depthfunc = DepthFunc.GL_EQUAL;
                        }
                        layer[2].blend = true;
                        layer[2].blendsrc = (uint)BlendDFactor.GL_ZERO;
                        layer[2].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;
                        layer[2].lighting = false;
                    }

                    break;
                case "staticmesh.fx":
                    hasDetail = false;
                    hasDirt = false;
                    hasCrack = false;
                    hasCrackN = false;
                    hasDetaiN = false;
                    hasEnvMap = false;
                    alphaTest = 0;
                    twosided = false;
                    hasBump = true;

                    if (hasDetail)
                    {
                        if(mapnum > 1)
                        {
                            shortname = GetCleanMapName(map[1]);
                        }
                        else if (mapnum > 0)
                        {
                            shortname = GetCleanMapName(map[0]);
                        }
                    }
                    //ORIGINAL COMMENT: --- FFP ------------------------------------------------ 
                    //ORIGINAL COMMENT: check if file is in vegetation directory
                    bool veggie = _assetFileName.Contains("vegitation");
                    //ORIGINAL COMMENT: todo: check texture file paths instead so file is displayed properly outside veggie dir?
                    switch (technique)
                    {
                        case "":
                            break;
                        case "ColormapGloss":
                        case "EnvColormapGloss":
                            layernum = 1;
                            SetBase(1);
                            break;
                        case "Alpha":
                            layernum = 1;
                            SetAlpha(1);
                            break;
                        case "Alpha_Test":
                            layernum = 1;
                            SetAlphaTest(1);
                            break;
                        case "Base":
                            if (veggie)
                            {
                                alphaTest = 0.5f;
                                twosided = true;
                                layernum = 1;
                                layer[1].texcoff = 0;
                                layer[1].texmapid = (int)texmapid[0];
                                layer[1].depthfunc = DepthFunc.GL_LESS;
                                layer[1].depthWrite = true;
                                layer[1].alphaTest = true;
                                layer[1].alpharef = 0.25f;
                                layer[1].twosided = true;
                            }
                            else
                            {
                                layernum = 1;
                                SetBase(1);
                            }
                            break;
                        case "BaseDetail":
                        case "BaseDetailNDetail":
                        case "BaseDetailNDetailenvmap": // ORIGINAL COMMENT: this is from BF2142, i think?
                            if (veggie)
                            {
                                layernum = 2;
                                // ORIGINAL COMMENT: detail (trunk texture)
                                layer[1].texcoff = 1;
                                layer[1].texmapid = (int)texmapid[1];
                                layer[1].depthfunc = DepthFunc.GL_LESS;
                                layer[1].depthWrite = true;
                                layer[1].blend = false;
                                layer[1].lighting = true;

                                // base (trunk dirt)
                                layer[2].texcoff = 0;
                                layer[2].texmapid = (int)texmapid[0];
                                layer[2].depthfunc = DepthFunc.GL_EQUAL;
                                layer[2].depthWrite = false;
                                layer[2].blend = true;
                                layer[2].blendsrc = (uint)BlendDFactor.GL_DST_COLOR;
                                layer[2].blenddst = (uint)BlendDFactor.GL_SRC_COLOR;
                                layer[2].lighting = false;
                            }
                            else
                            {
                                layernum = 2;
                                SetBase(1);
                                SetDetail(2);
                                MakeAlpha();
                            }
                            break;
                        case "BaseDetailCrack":
                        case "BaseDetailCrackNCrack":
                        case "BaseDetailCrackNDetail":
                        case "BaseDetailCrackNDetailNCrack":
                            layernum = 3;
                            SetBase(1);
                            SetDetail(2);
                            SetCrack(3);

                            layer[1].texcoff = 0;
                            layer[2].texcoff = 1;
                            layer[3].texcoff = 3; //fixed

                            layer[1].texmapid = (int)texmapid[0];
                            layer[2].texmapid = (int)texmapid[1];
                            layer[3].texmapid = (int)texmapid[2];
                            break;
                        case "BaseDetailDirt":
                        case "BaseDetailDirtNDetail":
                            layernum = 4;
                            SetBase(1);
                            SetDetail(2);
                            SetDirt(4);// ORIGINAL COMMENT: we swap dirt and crack for FH2
                            SetCrack(3); // ORIGINAL COMMENT: we swap dirt and crack for FH2
                            MakeAlpha(); // no idea what these original comments mean
                            break;
                        default:// ORIGINAL COMMENT: auto generate
                            // the way this default case is written it looks almost like thise shouldn't be if, else if, else if... but if, if, if ect.
                            // but this is how the original vb code is
                            if (technique.Contains("base"))
                            {
                                layernum++;
                                SetBase((int)layernum);
                            }
                            else if (technique.Contains("detail"))
                            {
                                layernum++;
                                SetDetail((int)layernum);
                            }
                            else if (technique.Contains("dirt"))
                            {
                                layernum++;
                                SetDirt((int)layernum);
                            }
                            else if (technique.Contains("crack"))
                            {
                                layernum++;
                                SetCrack((int)layernum);
                            }
                            else if (technique.Contains("humanskin"))
                            {
                                layernum++;
                                SetBase((int)layernum);
                            }
                            else
                            {
                                layernum = 1;
                                SetBase((int)layernum);
                            }
                            break;
                    }
                    // ORIGINAL COMMENT: texmap to UV offset lookup table
                    int mapnum_ = 0; // named thusly to avoid conflict with member variable mapnum
                    int detail = 0;
                    int crack = 0;
                    if (technique.Contains("Base"))
                    {
                        mapuvid[mapnum_] = (uint)mapnum;
                        mapnum_++;
                    }
                    if (technique.Contains("Detail"))
                    {
                        mapuvid[mapnum_] = 1;
                        detail = mapnum_;
                        mapnum_++;
                    }
                    if (technique.Contains("Dirt"))
                    {
                        mapuvid[mapnum_] = 2;
                        mapnum_++;
                    }
                    if (technique.Contains("Crack"))
                    {
                        mapuvid[mapnum_] = 3;
                        crack = mapnum_;
                        mapnum_++;
                    }
                    if (technique.Contains("NDetail"))
                    {
                        mapuvid[mapnum_] = (uint)detail;
                        mapnum_++;
                    }
                    if (technique.Contains("NCrack"))
                    {
                        mapuvid[mapnum_] = (uint)crack;
                        mapnum_++;
                    }

                    //compute material hash
                    var maxmaps = mapnum;
                    string str = fxfile + technique;
                    for(int i=0; i<maxmaps; i++)
                    {
                        str = str + map[i];
                    }
                    hash = String.GetHashCode(str);

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
        public int hash;

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
