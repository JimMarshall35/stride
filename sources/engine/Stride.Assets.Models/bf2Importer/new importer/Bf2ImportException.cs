using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public class Bf2ImportException : Exception
    {
        public Bf2ImportException(string message) : base(message)
        {
        }
    }
}
