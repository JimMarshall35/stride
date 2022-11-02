using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Importer.Common;
using Stride.Assets.Models.bf2Importer.BFP4FExplorerWV;

namespace Stride.Assets.Models.bf2Importer
{
    // TODO: MOVE bf2Importer namespace into a new project under "90-Tools/3D-Importers"
    // was having trouble doing so, so just made a new sub folder / namespace in
    // Stride.Assets.Models for now
    public static class BF2Importer
    {
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
                    string materialName = $"{mat2.technique}{mat2.shaderFile}";
                    if (numberOfEachKey.ContainsKey(materialName))
                    {
                        numberOfEachKey[materialName]++;
                    }
                    else
                    {
                        numberOfEachKey[materialName] = 0;
                    }
                    var materialAsset = new Materials.MaterialAsset();
                    // do stuff here to set MaterialAsset
                    //materialAsset.Attributes.
                    var name = $"{materialName}{mat2.technique}{numberOfEachKey[materialName]}";
                    entityInfo.Materials[name] = materialAsset;
                }
            }
            return entityInfo;
        }

        private static EntityInfo WithModels(this EntityInfo info, BF2StaticMesh mesh)
        {
            info.Models = new List<MeshParameters>();
            for (int i=0; i < mesh.geometry.numGeom; i++)
            {
                var meshParams = new MeshParameters();
                info.Models.Add(meshParams);
            }
            return info;
        }


        private static EntityInfo ExtractStaticMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
            var parsedMesh = new BF2StaticMesh(File.ReadAllBytes(filePath));
            var entityInfo = new EntityInfo();

            return new EntityInfo()
                .WithTextureDependencies(parsedMesh, Path.GetDirectoryName(filePath))
                .WithMaterials(parsedMesh)
                .WithModels(parsedMesh);

        }
        private static EntityInfo ExtractBundledMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
            var parsedMesh = new BF2BundledMesh(File.ReadAllBytes(filePath));
            EntityInfo entityInfo = new EntityInfo();
            return entityInfo;

        }
        public static EntityInfo ExtractEntityInfo(string filePath, bool extractTextureDependencies)
        {
            EntityInfo entityInfo = new EntityInfo();
            return Path.GetExtension(filePath).ToLower() switch
            {
                ".staticmesh" => ExtractStaticMeshEntityInfo(filePath, extractTextureDependencies),
                ".bundledmesh" => ExtractBundledMeshEntityInfo(filePath, extractTextureDependencies),
                _ => null
            };
        }
    }
}
