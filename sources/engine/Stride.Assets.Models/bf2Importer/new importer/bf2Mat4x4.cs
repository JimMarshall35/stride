using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public class bf2Mat4x4
    {
        public float[] matrix { get; private set; }
        public bf2Mat4x4(Stream s)
        {
            matrix = new float[16];
            for(int i=0; i<16; i++)
            {
                matrix[i] = StreamHelpers.ReadFloat(s);
            }
        }
    }
}
