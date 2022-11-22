using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public class bf2mesh
    {
        /// <summary>
        /// header
        /// </summary>
        public bf2head head { get; private set; }

        /// <summary>
        /// unknown - always 0?
        /// </summary>
        public byte u1 { get; private set; }

        // geoms
        public uint geomnum { get; private set; }
        public bf2geom[] geom { get; private set; }

        // vertex attribute table
        public uint vertattribnum { get; private set; }
        public bf2vertattrib[] vertattrib { get; private set; }

        // vertices
        public uint vertformat { get; private set; } // always 4?  (e.g. GL_FLOAT)
        public uint vertstride { get; private set; }
        public uint vertnum { get; private set; }
        public float[] vert { get; private set; }

        // indices
        public uint indexnum { get; private set; }
        public ushort[] index { get; private set; }

        // unknown
        public uint u2 { get; private set; } // always 8?

        #region internal
        /// <summary>
        /// current loaded mesh file
        /// </summary>
        private string filename;

        /// <summary>
        /// filename extension
        /// </summary>
        private string fileext;

        /// <summary>
        /// true if file extension is "staticmesh"
        /// </summary>
        private bool isStaticMesh;

        /// <summary>
        /// true if file extension is "skinnedmesh"
        /// </summary>
        private bool isSkinnedMesh;

        /// <summary>
        /// true if file extension is "bundledmesh"
        /// </summary>
        private bool isBundledMesh;

        /// <summary>
        /// true if file is inside BFP4F directory
        /// </summary>
        private bool isBFP4F;

        /// <summary>
        /// mesh loaded properly
        /// </summary>
        private bool loadok;

        /// <summary>
        /// mesh rendered properly
        /// </summary>
        private bool drawok;

        /// <summary>
        /// vertstride / 4
        /// </summary>
        private int stride;

        /// <summary>
        /// facenum / 3
        /// </summary>
        private int facenum;

        /// <summary>
        /// number of polygons
        /// </summary>
        private int polynum;

        /// <summary>
        /// number of elements
        /// </summary>
        private int elemnum;

        /// <summary>
        /// vertex array normal vector offset
        /// </summary>
        private int normoff;

        /// <summary>
        /// vertex array tangent vector offset
        /// </summary>
        private int tangoff;

        /// <summary>
        /// vertex array UV0 offset
        /// </summary>
        private int texcoff;

        /// <summary>
        /// corrected tangents
        /// </summary>
        private bf2Vec4[] xtan;

        /// <summary>
        /// number of detected uv channels
        /// </summary>
        private int uvnum;

        /// <summary>
        /// has mesh_edit info computed
        /// </summary>
        private bool editinfo;

        /// <summary>
        /// vertex info & flags
        /// </summary>
        fh2vert[] vertinfo;

        /// <summary>
        /// face info & flags
        /// </summary>
        fh2face[] faceinfo;

        /// <summary>
        /// polygon info & flags
        /// </summary>
        fh2poly[] polyinfo;

        /// <summary>
        /// element info & flags
        /// </summary>
        fh2elem eleminfo;

        /// <summary>
        /// deformed vertices flag
        /// </summary>
        bool hasSkinVerts;

        /// <summary>
        /// deformed vertices
        /// </summary>
        bf2Vec3[] skinvert;

        /// <summary>
        /// deformed normals
        /// </summary>
        bf2Vec3[] skinnorm;

        #endregion

        public bf2mesh(string filePath)
        {

            MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath));


            filename = filePath;
            fileext = Path.GetExtension(filePath).ToLower();
            isStaticMesh = (fileext == ".staticmesh");
            isBundledMesh = (fileext == ".bundledmesh");
            isSkinnedMesh = (fileext == ".skinnedmesh");
            isBFP4F = filename.Contains("bfp4f");
            loadok = false;
            drawok = true;

            //--- header --------------------------------------------------------------------------------
            head = new bf2head(stream);
            u1 = (byte)stream.ReadByte(); //stupid little byte that misaligns the entire file!
            //for BFP4F, the value is "1", so perhaps this is a version number as well
            if (u1 == 1)
            {
                isBFP4F = true;
            }

            //--- geom table ---------------------------------------------------------------------------
            geomnum = StreamHelpers.ReadU32(stream);
            geom = new bf2geom[geomnum];
            for(int i=0; i<geomnum; i++)
            {
                geom[i] = new bf2geom(stream);
            }

            //--- vertex attribute table -------------------------------------------------------------------------------
            vertattribnum = StreamHelpers.ReadU32(stream);
            vertattrib = new bf2vertattrib[vertattribnum];
            for(int i=0; i<vertattribnum; i++)
            {
                vertattrib[i] = new bf2vertattrib(stream);
            }

            //--- vertices -----------------------------------------------------------------------------
            vertformat = StreamHelpers.ReadU32(stream);
            vertstride = StreamHelpers.ReadU32(stream);
            vertnum = StreamHelpers.ReadU32(stream);
            uint numFloats = (vertstride / vertformat) * vertnum;
            vert = new float[numFloats];
            for(int i=0; i<numFloats; i++)
            {
                // probably a faster way - jim
                vert[i] = StreamHelpers.ReadFloat(stream);
            }

            //--- indices ------------------------------------------------------------------------------
            indexnum = StreamHelpers.ReadU32(stream);
            index = new ushort[indexnum];
            for (int i = 0; i < indexnum; i++)
            {
                // probably a faster way - jim
                index[i] = StreamHelpers.ReadU16(stream);
            }

            //--- rigs -------------------------------------------------------------------------------
            if (!isSkinnedMesh)
            {
                u2 = StreamHelpers.ReadU32(stream);
            }

            for(int i = 0; i < geomnum; i++)
            {
                for(int j = 0; j<geom[i].lodnum; j++)
                {
                    geom[i].lod[j] = new bf2lod();
                    ReadLodNodeTable(stream, geom[i].lod[j]);
                }
            }

            //--- triangles ------------------------------------------------------------------------------
            for (int i = 0; i < geomnum; i++)
            {
                for(int j = 0; j < geom[i].lodnum; j++)
                {
                    ReadGeomLod(stream, geom[i].lod[j], filePath);
                }
            }

            loadok = true;
        }

        public bf2vertattrib FindVertAttribByUsage(VertexUsage usage)
        {
            foreach(var attrib in vertattrib)
            {
                if(attrib.Usage == usage)
                {
                    return attrib;
                }
            }
            return null;
        }

        private void ReadLodNodeTable(Stream stream, bf2lod lod)
        {
            //bounds (24 bytes)
            lod.min = StreamHelpers.ReadVector3(stream);
            lod.max = StreamHelpers.ReadVector3(stream);

            //unknown (12 bytes)
            if (head.version <= 6) //version 4 and 6
            {
                lod.pivot = StreamHelpers.ReadVector3(stream);
            }

            if (isSkinnedMesh)
            {
                //rignum (4 bytes)
                lod.rignum = StreamHelpers.ReadU32(stream);
                //read rigs
                if (lod.rignum > 0)
                {
                    lod.rig = new bf2rig[lod.rignum];
                    for(int i=0; i < lod.rignum; i++)
                    {
                        lod.rig[i] = new bf2rig();
                        var rig = lod.rig[i];
                        //bonenum (4 bytes)
                        rig.bonenum = StreamHelpers.ReadU32(stream);

                        //bones (68 bytes * bonenum)
                        if (rig.bonenum > 0)
                        {
                            rig.bone = new bf2bone[rig.bonenum];
                            for(int j=0; j < rig.bonenum; j++)
                            {
                                rig.bone[j] = new bf2bone();
                                //bone id (4 bytes)
                                rig.bone[j].id = StreamHelpers.ReadU32(stream);
                                //bone transform (64 bytes)
                                rig.bone[j].matrix = new bf2Mat4x4(stream);
                            }
                        }
                    }
                }
            }
            else
            {
                //nodenum (4 bytes)
                lod.nodenum = StreamHelpers.ReadU32(stream);

                //node matrices (64 bytes * nodenum)
                if (!isBundledMesh)
                {
                    if(lod.nodenum > 0)
                    {
                        lod.node = new bf2Mat4x4[lod.nodenum];
                        for(int i=0; i < lod.nodenum; i++)
                        {
                            lod.node[i] = new bf2Mat4x4(stream);
                        }
                    }
                }

                //node matrices (BFP4F variant)
                if (isBundledMesh && isBFP4F)
                {
                    if(lod.nodenum > 0)
                    {
                        lod.node = new bf2Mat4x4[lod.nodenum];
                        for(int i=0; i < lod.nodenum; i++)
                        {
                            // matrix(64 bytes)
                            lod.node[i] = new bf2Mat4x4(stream);
                            // reads u32 of string length and then that many bytes
                            // and converts to string
                            var name = StreamHelpers.ReadCString(stream);
                            
                        }
                    }
                }
            }
        }

        private void ReadGeomLod(Stream stream, bf2lod lod, string filepath)
        {
            //internal: reset polycount
            lod.polycount = 0;
            //matnum (4 bytes)
            lod.matnum = StreamHelpers.ReadU32(stream);
            //materials (? bytes)
            lod.mat = new bf2mat[lod.matnum];
            for(int i=0; i < lod.matnum; i++)
            {
                lod.mat[i] = new bf2mat(filepath);
                ReadLodMat(stream, lod.mat[i]);
                lod.polycount += lod.mat[i].inum / 3;
            }
        }

        

        private void ReadLodMat(Stream stream, bf2mat mat)
        {
            if (!isSkinnedMesh)
            {
                mat.alphamode = StreamHelpers.ReadU32(stream);
            }
            mat.fxfile = StreamHelpers.ReadCString(stream);
            mat.technique = StreamHelpers.ReadCString(stream);
            mat.mapnum = StreamHelpers.ReadU32(stream);
            if(mat.mapnum > 0)
            {
                mat.map = new string[mat.mapnum];
                for(int i=0; i < mat.mapnum; i++)
                {
                    mat.map[i] = StreamHelpers.ReadCString(stream);
                }
            }
            mat.vstart = StreamHelpers.ReadU32(stream);
            mat.istart = StreamHelpers.ReadU32(stream);
            mat.inum = StreamHelpers.ReadU32(stream);
            mat.vnum = StreamHelpers.ReadU32(stream);

            mat.u4 = StreamHelpers.ReadU32(stream);
            mat.u5 = StreamHelpers.ReadU32(stream);

            if (!isSkinnedMesh)
            {
                if(head.version == 11)
                {
                    mat.mmin = StreamHelpers.ReadVector3(stream);
                    mat.mmax = StreamHelpers.ReadVector3(stream);
                }
            }

            //--- internal --------------------------------------
            
            mat.PopulateShaderInfo();
            //'quick hack: needed for proper tangent computation
            //If vmesh.isBundledMesh Then .hasAnimatedUV = InString(.technique, "AnimatedUV")
        }
    }
}
