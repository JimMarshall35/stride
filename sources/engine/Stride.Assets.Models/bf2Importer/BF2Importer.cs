using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Importer.Common;

namespace Stride.Assets.Models.bf2Importer
{
    // TODO: MOVE bf2Importer namespace into a new project
    // was having trouble sojust made a new sub folder / namespace in
    // Stride.Assets.Models for now
    public static class BF2Importer
    {
        private static EntityInfo ExtractStaticMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
            var parsedMesh = new BFP4FExplorerWV.BF2StaticMesh(File.ReadAllBytes(filePath));
            EntityInfo entityInfo = new EntityInfo();
            return entityInfo;

        }
        private static EntityInfo ExtractBundledMeshEntityInfo(string filePath, bool extractTextureDependencies)
        {
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
