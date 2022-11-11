using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public static class Bf2Loader
    {
        public static bf2mesh LoadBf2File(string filePath, Action<string> logErrorMessage)
        {
            bf2mesh mesh;
            try
            {
                mesh = new bf2mesh(filePath);
            }
            catch(Bf2ImportException e)
            {
                logErrorMessage(e.Message);
                return null;
            }

            return mesh;
        }
    }
}
