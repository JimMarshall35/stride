using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Assets.Models.bf2Importer.new_importer;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Graphics.Data;
using Stride.Importer.Common;
using Stride.Rendering;
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
            mesh.Name = "Mesh1";
            mesh.MaterialIndex = 0;
            mesh.NodeIndex = 0;
            model.Add(mesh);
            return model;
        }

        internal static EntityInfo ExtractEntityInfo(string path)
        {
            var bfmesh = Bf2Loader.LoadBf2File(path, LogErrorMessage);
            EntityInfo entityInfo = new EntityInfo();
            var meshParams = new List<MeshParameters>();
            var mp = new MeshParameters();
            mp.NodeName = "mesh";
            mp.MeshName = path;
            return entityInfo;
        }

        private static void LogErrorMessage(string error)
        {

        }
    }
}
