using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// bf2 geom
    /// </summary>
    public class bf2geom
    {
        public int lodnum { get; private set; }
        public bf2lod[] lod { get; private set; }
        public bf2geom(Stream s)
        {
            lodnum = StreamHelpers.ReadS32(s);
            lod = new bf2lod[lodnum];
        }
    }
}
