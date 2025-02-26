using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Assets;
using Stride.Core.Diagnostics;
using Stride.Core.IO;
using Stride.Importer.Common;
using Stride.Assets.Models.bf2Importer;
using Stride.Assets.Textures;

namespace Stride.Assets.Models
{
    
    internal class BF2AssetImporter : ModelAssetImporter
    {
        internal const string FileExtensions = ".staticmesh;.bundledmesh";
        public override Guid Id => new Guid("607245EF-DEC6-476C-A24A-90FBED3D5D5F");

        public override string Description => "bf2 importer";

        public override string SupportedFileExtensions => FileExtensions;

        public override void GetAnimationDuration(UFile localPath, Logger logger, AssetImporterParameters importParameters, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;
        }

        public override EntityInfo GetEntityInfo(UFile localPath, Logger logger, AssetImporterParameters importParameters)
            => NewBf2Importer.ExtractEntityInfo(localPath.FullPath);
    }
}
