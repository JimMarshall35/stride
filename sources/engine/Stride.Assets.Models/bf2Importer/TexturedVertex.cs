using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;

namespace Stride.Assets.Models.bf2Importer
{
    internal struct TexturedVertex
    {
        public TexturedVertex(Vector4 position, Color4 colour, Vector2 uv)
        {
            Position = position;
            Colour = colour;
            UV = uv;
        }
        public Vector4 Position;
        public Color4 Colour;
        public Vector2 UV;
    }
}
