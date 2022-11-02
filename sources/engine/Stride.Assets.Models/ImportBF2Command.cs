using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Animations;
using Stride.Core.BuildEngine;
using Stride.Core.Serialization.Contents;
using Stride.Rendering;

namespace Stride.Assets.Models
{
    [Description("Import Bf2")]
    internal class ImportBF2Command : ImportModelCommand
    {
        private static string[] supportedExtensions = BF2AssetImporter.FileExtensions.Split(';');

        public override string Title => throw new NotImplementedException();

        protected override Dictionary<string, AnimationClip> LoadAnimation(ICommandContext commandContext, ContentManager contentManager, out TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        protected override Model LoadModel(ICommandContext commandContext, ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        protected override Skeleton LoadSkeleton(ICommandContext commandContext, ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        public static bool IsSupportingExtensions(string ext)
        {
            if (string.IsNullOrEmpty(ext))
                return false;

            var extToLower = ext.ToLowerInvariant();

            return supportedExtensions.Any(supExt => supExt.Equals(extToLower));
        }
    }
}
