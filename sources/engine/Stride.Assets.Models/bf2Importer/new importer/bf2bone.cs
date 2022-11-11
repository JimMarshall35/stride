using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// bf2 bone structure
    /// 68 bytes
    /// </summary>
    public class bf2bone
    {
        /// <summary>
        /// bone ID (4 bytes)
        /// </summary>
        public uint id;

        /// <summary>
        /// inverse bone matrix (64 bytes)
        /// </summary>
        public bf2Mat4x4 matrix;

        #region internal

        /// <summary>
        /// world space deformed skin transform
        /// </summary>
        public bf2Mat4x4 skinmat;

        #endregion
    }
}
