using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Animations;
using Stride.Assets.Models.bf2Importer;
using Stride.Core.BuildEngine;
using Stride.Core.Serialization.Contents;
using Stride.Rendering;
using Stride.Core.Mathematics;


namespace Stride.Assets.Models
{
    [Description("Import Bf2")]
    internal class ImportBF2Command : ImportModelCommand
    {
        private static string[] supportedExtensions = BF2AssetImporter.FileExtensions.Split(';');

        public override string Title => throw new NotImplementedException();

        protected override Dictionary<string, AnimationClip> LoadAnimation(ICommandContext commandContext, ContentManager contentManager, out TimeSpan duration)
        {
            duration = TimeSpan.FromSeconds(0);
            return new Dictionary<string, AnimationClip>();
        }

        protected override Model LoadModel(ICommandContext commandContext, ContentManager contentManager)
        {
            return NewBf2Importer.Convert(SourcePath);
        }

        protected override Skeleton LoadSkeleton(ICommandContext commandContext, ContentManager contentManager)
        {
            var s = new Skeleton();
            s.Nodes = new ModelNodeDefinition[1];
            var d = new ModelNodeDefinition();
            d.ParentIndex = -1;
            d.Transform.Rotation = Quaternion.Identity;
            d.Transform.Scale = Vector3.One;
            d.Flags = ModelNodeFlags.Default;
            s.Nodes[0] = d;
            return s;
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
