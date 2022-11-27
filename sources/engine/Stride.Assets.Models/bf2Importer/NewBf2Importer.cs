using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Assets.Materials;
using Stride.Assets.Models.bf2Importer.new_importer;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Graphics.Data;
using Stride.Importer.Common;
using Stride.Rendering;
using Stride.Rendering.Materials.ComputeColors;
using static Stride.Engine.ModelComponent;

namespace Stride.Assets.Models.bf2Importer
{
    internal static class NewBf2Importer
    {
        internal static PixelFormat GetPixelFormat(this bf2vertattrib attrib) => attrib.VertType switch
        {
            VertexType.FLOAT1 => PixelFormat.R32_Float,
            VertexType.FLOAT2 => PixelFormat.R32G32_Float,
            VertexType.FLOAT3 => PixelFormat.R32G32B32_Float,
            VertexType.FLOAT4 => PixelFormat.R32G32B32A32_Float,
            VertexType.D3DCOLOR => PixelFormat.R8G8B8A8_UNorm_SRgb, // not 100% sure about this one UNorm == fixed point 0 - 1?. value may also be ARGB
            VertexType.UBYTE4 => PixelFormat.R8G8B8A8_UInt,
            VertexType.SHORT2 => PixelFormat.R16G16_SInt,
            VertexType.SHORT4 => PixelFormat.R16G16B16A16_SInt, // d3d9types.h header shows signed short
            _ => PixelFormat.None
        };
        internal static List<VertexElement> BuildVertexDeclaration(this bf2mesh mesh)
        {
            var list = new List<VertexElement>();
            var vertexStride = 0;
            foreach(var attrib in mesh.vertattrib)
            {
                if(attrib.GetPixelFormat() != PixelFormat.None)
                {
                    switch (attrib.Usage)
                    {
                        case VertexUsage.POSITION:
                            list.Add(VertexElement.Position(0, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.BLENDWEIGHT:
                            list.Add(new VertexElement("BLENDWEIGHT", 0, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.BLENDINDICES:
                            list.Add(new VertexElement("BLENDINDICES", 0, attrib.GetPixelFormat(), attrib.offset)); // D3DCOLOR
                            break;
                        case VertexUsage.NORMAL:
                            list.Add(VertexElement.Normal(0, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.PSIZE:
                            //? Not handled in bfMeshViewer - probably never occurs
                            throw new Bf2ImportException("Unsure how to handle this Vertex attribute");
                            break;
                        case VertexUsage.TEXCOORD1:
                            list.Add(VertexElement.TextureCoordinate(0, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.TEXCOORD2:
                            list.Add(VertexElement.TextureCoordinate(1, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.TEXCOORD3:
                            list.Add(VertexElement.TextureCoordinate(2, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.TEXCOORD4:
                            list.Add(VertexElement.TextureCoordinate(3, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.TEXCOORD5:
                            list.Add(VertexElement.TextureCoordinate(4, attrib.GetPixelFormat(), attrib.offset));
                            break;
                        case VertexUsage.TANGENT:
                            list.Add(VertexElement.Tangent(0, attrib.GetPixelFormat(), attrib.offset));
                            break;
                    }
                }
                
                vertexStride += attrib.offset;
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="geom"></param>
        /// <param name="lod"></param>
        /// <returns>
        /// a tuple of verts and indices - 
        /// the indices are for a specific geometry and LOD,
        /// the vertices are for the entire file, for now
        /// </returns>
        /// <exception cref="Bf2ImportException"></exception>
        internal static (byte[], byte[]) GetVertexAndIndexBuffers(this bf2mesh mesh, uint geom, uint lod)
        {
            if(geom >= mesh.geomnum)
            {
                throw new Bf2ImportException($"geom index {geom} was given,  max is {mesh.geomnum}");
            }
            if (lod >= mesh.geom[geom].lodnum)
            {
                throw new Bf2ImportException($"lod index {lod} was given,  max is {mesh.geom[geom].lodnum}");
            }
            // we assume geomnum > 0, and lodnum > 0 for this mesh and geom - should check somewhere earlier when loading

            var matnum = mesh.geom[geom].lod[lod].matnum;
            var indices = new List<ushort>();

            // for the specified mesh and lod gather all indices, which might span accross several materals
            for (int i = 0; i < matnum; i++)
            {
                var mat = mesh.geom[geom].lod[lod].mat[i];
                for(uint j=mat.istart; j < mat.istart + mat.inum; j++)
                {
                    indices.Add(mesh.index[j]);
                }
            }

            // return the verts for all the meshes and LODs - this is how the
            // indices apply - could "cut out" the vertices and adjust the indices based on offset, maybe will have to
            uint vertsSizeBytes = mesh.vertnum * mesh.vertstride;
            int indicesSizeBytes = indices.Count * sizeof(ushort);

            (byte[] verts, byte[] indices) rt = (new byte[vertsSizeBytes], new byte[indicesSizeBytes]);

            System.Buffer.BlockCopy(mesh.vert, 0, rt.verts, 0, (int)vertsSizeBytes);
            System.Buffer.BlockCopy(indices.ToArray(), 0, rt.indices, 0, (int)indicesSizeBytes);
            return rt;
        }

        internal static MeshDraw BuildStrideMeshDraw(this bf2mesh mesh, uint geom, uint lod)
        {
            var vertAttribsList = mesh.BuildVertexDeclaration();
            (var verts, var indices) = mesh.GetVertexAndIndexBuffers(geom, lod);
            var vertexDeclaration = new VertexDeclaration(vertAttribsList.ToArray());

            var indexCountInUShorts = indices.Length / sizeof(ushort);

            var vertexBufferBinding = new VertexBufferBinding(
                GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.VertexBuffer, verts)),
                vertexDeclaration,
                (int)mesh.vertnum,
                (int)mesh.vertstride);

            var indexBufferBinding = new IndexBufferBinding(
                GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.IndexBuffer, indices)),
                false, // is 32 bit? no, indices are ushort's
                (int)indexCountInUShorts);

            
            var vbb = new List<VertexBufferBinding>();
            vbb.Add(vertexBufferBinding);

            var drawData = new MeshDraw();
            drawData.VertexBuffers = vbb.ToArray();
            drawData.IndexBuffer = indexBufferBinding;
            drawData.PrimitiveType = PrimitiveType.TriangleList;
            drawData.DrawCount = indexCountInUShorts;

            return drawData;
        }

        internal static Model Convert(string path)
        {
            var model = new Model();
            var bfmesh = Bf2Loader.LoadBf2File(path, LogErrorMessage);
            var draw = bfmesh.BuildStrideMeshDraw(0, 0);//hard code for now
            var mesh = new Mesh();
            mesh.Draw = draw;
            mesh.Name = "mesh";
            mesh.MaterialIndex = 0;
            mesh.NodeIndex = 0;
            model.Meshes.Add(mesh);
            return model;
        }

        private static string GetMeshName(string path, int index) => $"{Path.GetFileName(path)}{index}_Mesh";
        private static string GetNodeName(string path, int index) => $"{Path.GetFileName(path)}{index}_Node";
        private static string GetMaterialName(bf2mat mat) => $"{mat.hash}_Material";

        private static bool ConsideredEqual(bf2mat mat1, bf2mat mat2)
        {
            if(mat1.alphamode != mat2.alphamode)
            {
                return false;
            }
            if (mat1.fxfile != mat2.fxfile)
            {
                return false;
            }
            if(mat1.technique != mat2.technique)
            {
                return false;
            }
            if (mat1.mapnum != mat2.mapnum)
            {
                return false;
            }
            if (mat1.map.Length != mat2.map.Length)
            {
                return false;
            }
            for(int i=0; i<mat1.map.Length; i++)
            {
                if(mat1.map[i] != mat2.map[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// adds a material to the dictionary if there is not an equivalent one present,
        /// generating a name for the key
        /// </summary>
        /// <param name="materials"> dictionary to fill will unique materials by calling this function </param>
        /// <param name="newCandidateMaterial"> </param>
        /// <param name="path"> used to generate name </param>
        /// <param name="index"> used to generate name </param>
        /// <returns>
        /// the generated name, if there is one in the dictionary that's equivalent it'll return
        /// that ones name (its key)
        /// </returns>
        private static string AddMaterialIfNotPresent(Dictionary<string, bf2mat> materials, bf2mat newCandidateMaterial)
        {
            foreach(var p in materials)
            {
                var name = p.Key;
                var mat = p.Value;
                if(ConsideredEqual(mat, newCandidateMaterial))
                {
                    return name;
                }
            }
            // not in dict
            var generatedName = GetMaterialName(newCandidateMaterial);
            materials.Add(generatedName, newCandidateMaterial);
            return generatedName;
        }

        private static void AddToTextureDependenciesIfNotPresent(List<string> deps, string newCandidate)
        {
            foreach(string dep in deps)
            {
                if(newCandidate == dep)
                {
                    return;
                }
            }
            deps.Add(newCandidate);
        }

        internal static EntityInfo ExtractEntityInfo(string path)
        {
            var bfmesh = Bf2Loader.LoadBf2File(path, LogErrorMessage);
            EntityInfo entityInfo = new EntityInfo();
            var meshParams = new List<MeshParameters>();
            var nodeInfos = new List<NodeInfo>();
            var uniqueMaterials = new Dictionary<string, bf2mat>();
            var textureDependencies = new List<string>();
            var materialsDict = new Dictionary<string, MaterialAsset>();

            for (int i = 0; i < bfmesh.geom.Length; i++)
            {
                var geom = bfmesh.geom[i];
                for(int j=0; j<bfmesh.geom[i].lodnum; j++)
                {
                    bf2lod lod = bfmesh.geom[i].lod[j];

                    var nodeNum = lod.nodenum;
                    for(int k=0; k<nodeNum; k++)
                    {
                        bf2Mat4x4 node = lod.node[k];
                        var nodeInfo = new NodeInfo();
                        nodeInfo.Depth = k;
                        nodeInfo.Name = GetNodeName(path, k);
                        nodeInfos.Add(nodeInfo);
                    }
                    for (int k=0; k< lod.matnum; k++)
                    {
                        var mp = new MeshParameters();
                        mp.MeshName = GetMeshName(path, k);
                        bf2mat mat = lod.mat[k];
                        // in the file, each lod of each geometry has it's own material,
                        // with some being effectively copies of others. We remove the copies
                        // and name each one storing in unique materials. We give the MeshParameter the
                        // same name as the unique material
                        var name = AddMaterialIfNotPresent(uniqueMaterials, mat);
                        mp.MaterialName = name;
                        mp.NodeName = GetNodeName(path, k);

                        meshParams.Add(mp);
                    }
                }
                //var mp = new MeshParameters();
                //bfmesh.
            }

            foreach(var pair in uniqueMaterials)
            {
                var mat = pair.Value;
                foreach(var mapName in mat.map)
                {
                    AddToTextureDependenciesIfNotPresent(textureDependencies, mapName);
                }
                // TODO: add materials to entityinfo
                var strideMat = new MaterialAsset();
                foreach(var layer in mat.layer)
                {
                    //mat.IsBumpMap(layer.texmapFilename);
                }
            }
            
            entityInfo.TextureDependencies = textureDependencies;
            entityInfo.Nodes = nodeInfos;
            entityInfo.Materials = materialsDict;
            entityInfo.Models = meshParams;

            return entityInfo;
        }

        private static void LogErrorMessage(string error)
        {

        }
    }
}
