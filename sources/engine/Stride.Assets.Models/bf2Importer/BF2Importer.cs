using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Importer.Common;
using Stride.Assets.Models.bf2Importer.BFP4FExplorerWV;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Graphics;
using Stride.Graphics.Data;
using System.Runtime.InteropServices;
using static Stride.Engine.ModelComponent;

namespace Stride.Assets.Models.bf2Importer
{
    public static class CastingHelper
    {
        public static T CastToStruct<T>(this byte[] data) where T : struct
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
            pData.Free();
            return result;
        }

        public static byte[] CastToArray<T>(this T data) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
            pResult.Free();
            return result;
        }
    }
    // TODO: MOVE bf2Importer namespace into a new project under "90-Tools/3D-Importers"
    // was having trouble doing so, so just made a new sub folder / namespace in
    // Stride.Assets.Models for now
    public static class BF2Importer
    {
        #region staticmesh

        private static TexturedVertex GetVertex(int pos, Helper.BF2MeshGeometry geometry)
        {
            return new TexturedVertex(
                new Vector4(geometry.vertices[pos], geometry.vertices[pos + 1], geometry.vertices[pos + 2], 1),
                Color.White,
                new Vector2(geometry.vertices[pos + 7], geometry.vertices[pos + 8])
            );
        }

        private static List<TexturedVertex> ExtractVertices(this BF2StaticMesh mesh, int geoMatIdx)
        {
            List<TexturedVertex> vertices = new List<TexturedVertex>();
            if (geoMatIdx >= mesh.geomat.Count)
                geoMatIdx = mesh.geomat.Count() - 1;
            Helper.BF2MeshSTMGeometryMaterial lod0 = mesh.geomat[geoMatIdx];
            for (int i = 0; i < lod0.numMaterials; i++)
            {
                Helper.BF2MeshSTMMaterial mat = lod0.materials[i];

                int m = mesh.geometry.vertices.Count / (int)mesh.geometry.numVertices;
                for (int j = 0; j < mat.numIndicies; j++)
                {
                    int pos = (mesh.geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex) * m;
                    vertices.Add(GetVertex(pos, mesh.geometry));
                }

            }
            return vertices;
        }
        private static string GetName(this Helper.BF2MeshSTMMaterial material, Dictionary<string, int> numberOfEachKey)
        {
            string materialName = $"{material.technique}{material.shaderFile}";
            if (numberOfEachKey.ContainsKey(materialName))
            {
                numberOfEachKey[materialName]++;
            }
            else
            {
                numberOfEachKey[materialName] = 0;
            }
            return $"{materialName}{numberOfEachKey[materialName]}";
        }
        /// <summary>
        /// set an EntityInfo object's TextureDependencies
        /// property based on a List of Helper.BF2MeshSTMGeometryMaterial
        /// from a parsed BF2 staticmesh file
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="materials"></param>
        /// <returns></returns>
        private static EntityInfo WithTextureDependencies(this EntityInfo entityInfo, BF2StaticMesh mesh, string modelPath)
        {
            // extract unique texture paths
            var accumulatedTexturesList = new List<string>();
            foreach (var mat in mesh.geomat)
            {
                foreach (var mat2 in mat.materials)
                {
                    accumulatedTexturesList = mat2.textureMapFiles.Aggregate(accumulatedTexturesList, (acc, next) =>
                    {
                        if (!acc.Contains(next))
                        {
                            acc.Add(next);
                        }
                        return acc;
                    });
                }
            }
            entityInfo.TextureDependencies = accumulatedTexturesList;
            return entityInfo;
        }

        private static EntityInfo WithMaterials(this EntityInfo entityInfo, BF2StaticMesh mesh)
        {
            var numberOfEachKey = new Dictionary<string, int>();
            entityInfo.Materials = new Dictionary<string, Materials.MaterialAsset>();
            foreach (var mat in mesh.geomat)
            {
                foreach (var mat2 in mat.materials)
                {

                    
                    // do stuff here to set MaterialAsset
                    //materialAsset.Attributes.
                    var name = mat2.GetName(numberOfEachKey);
                    var materialAsset = new Materials.MaterialAsset();
                    entityInfo.Materials[name] = materialAsset;
                }
            }
            return entityInfo;
        }

        private static EntityInfo WithModels(this EntityInfo info, BF2StaticMesh mesh)
        {
            var numberOfEachKey = new Dictionary<string, int>();
            info.Models = new List<MeshParameters>();
            foreach(var m in mesh.geomat)
            {
                foreach(var m2 in m.materials)
                {
                    var param = new MeshParameters();
                    param.MaterialName = m2.GetName(numberOfEachKey);
                    param.MeshName = m2.GetName(numberOfEachKey);
                    param.BoneNodes = new HashSet<string>();
                    param.NodeName = "";
                    info.Models.Add(param);
                }
            }
            return info;
        }


        private static EntityInfo ExtractStaticMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
            var parsedMesh = new BF2StaticMesh(File.ReadAllBytes(filePath));
            var entityInfo = new EntityInfo();

            var e = new EntityInfo()
                .WithTextureDependencies(parsedMesh, Path.GetDirectoryName(filePath))
                .WithMaterials(parsedMesh)
                .WithModels(parsedMesh);

            e.AnimationNodes = new List<string>();
            e.Nodes = new List<NodeInfo>();
            var ni = new NodeInfo();
            ni.Name = "";
            e.Nodes.Add(ni);
            var v = ExtractVertices(parsedMesh, 0);
            
            return e;

        }

        private static Model ConvertStaticMesh(string inputFilePath, string outputFilePath)
        {
            var parsedMesh = new BF2StaticMesh(File.ReadAllBytes(inputFilePath));
            var model = new Model();
            var strideBf2Meshes = new List<StrideBf2MeshInfo>();

            // Build the vertices data buffer 
            var sourceVertsArray = parsedMesh.CompactVertices;

            var vertexBuffer = new byte[sourceVertsArray.Length * sizeof(float)];

            for (var i=0; i < parsedMesh.geomat.Count; i++)
            {
                // Build the vertex declaration
                var vertexElements = new List<VertexElement>();
                int stride = 0;
                vertexElements.Add(VertexElement.Position<Vector3>(0, stride));
                vertexElements.Add(VertexElement.TextureCoordinate<Vector2>(0, stride));
                stride += Vector2.SizeInBytes;


                var verts = ExtractVertices(parsedMesh, i);

                System.Buffer.BlockCopy(sourceVertsArray, 0, vertexBuffer, 0, vertexBuffer.Length);

                // Build the indices data buffer
                var thisMesh = parsedMesh.geomat[i];
                
                uint numIndices = 0;
                foreach(var mat in thisMesh.materials)
                {
                    numIndices += mat.numIndicies;
                }
                var indicesBuffer = new ushort[numIndices];
                foreach (var mat in thisMesh.materials)
                    for (int j = 0; j < numIndices; j++)
                    {
                        int pos = parsedMesh.geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex;

                        indicesBuffer[j] = (ushort)pos;
                    }
                var indicesBytesBuffer = new byte[numIndices * sizeof(ushort)];
                System.Buffer.BlockCopy(indicesBuffer,0,indicesBytesBuffer,0, indicesBytesBuffer.Length);

                // Build the mesh data
                var vertexDeclaration = new VertexDeclaration(vertexElements.ToArray());
                var vertexBufferBinding = new VertexBufferBinding(GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.VertexBuffer, vertexBuffer)), vertexDeclaration, parsedMesh.geometry.vertices.Count, vertexDeclaration.VertexStride, 0);
                var indexBufferBinding = new IndexBufferBinding(GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.IndexBuffer, indicesBytesBuffer)), false, (int)numIndices, 0);
                
                var vbb = new List<VertexBufferBinding>();
                vbb.Add(vertexBufferBinding);

                var drawData = new MeshDraw();
                drawData.VertexBuffers = vbb.ToArray();
                drawData.IndexBuffer = indexBufferBinding;
                drawData.PrimitiveType = PrimitiveType.TriangleList;
                drawData.DrawCount = (int)numIndices;

                var meshInfo = new StrideBf2MeshInfo(drawData,i);
                strideBf2Meshes.Add(meshInfo);

            }
            return model;
        }

        private static Model ConvertBundledMesh(string inputFilePath, string outputFilePath)
        {
            var model = new Model();
            return model;
        }

        #endregion

        private static EntityInfo ExtractBundledMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
            var parsedMesh = new BF2BundledMesh(File.ReadAllBytes(filePath));
            EntityInfo entityInfo = new EntityInfo();
            return entityInfo;

        }

        public static Model Convert(string inputFilePath, string outputFilePath)
        {
            return Path.GetExtension(inputFilePath).ToLower() switch
            {
                ".staticmesh" => ConvertStaticMesh(inputFilePath, outputFilePath),
                ".bundledmesh" => ConvertBundledMesh(inputFilePath, outputFilePath),
                _ => null
            };
        }

        public static EntityInfo ExtractEntityInfo(string filePath, bool extractTextureDependencies)
        {
            return Path.GetExtension(filePath).ToLower() switch
            {
                ".staticmesh" => ExtractStaticMeshEntityInfo(filePath, extractTextureDependencies),
                ".bundledmesh" => ExtractBundledMeshEntityInfo(filePath, extractTextureDependencies),
                _ => null
            };
        }
    }
}
