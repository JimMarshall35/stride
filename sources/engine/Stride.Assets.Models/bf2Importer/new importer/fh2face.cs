using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    /// <summary>
    /// internal structure
    /// </summary>
    public class fh2face
    {
        /// <summary>
        /// geom index
        /// </summary>
        byte geom;

        /// <summary>
        /// LOD index
        /// </summary>
        byte lod;

        /// <summary>
        /// LOD material index
        /// </summary>
        byte mat;


        /// <summary>
        /// selection
        /// </summary>
        byte sel;

        /// <summary>
        /// selecton mask
        /// </summary>
        byte flag;

        /// <summary>
        /// marked as bad face (skip when processing)
        /// </summary>
        byte bad;


        /// <summary>
        /// vertex index 1
        /// </summary>
        int v1;

        /// <summary>
        /// vertex index 2
        /// </summary>
        int v2;

        /// <summary>
        /// vertex index 3
        /// </summary>
        int v3;


        /// <summary>
        /// polygon ID
        /// </summary>
        int poly;

        /// <summary>
        /// element ID
        /// </summary>
        int elem;

        /// <summary>
        /// face index of neighbor on edge 1
        /// </summary>
        int f1;

        /// <summary>
        /// face index of neighbor on edge 2
        /// </summary>
        int f2;

        /// <summary>
        /// face index of neighbor on edge 3
        /// </summary>
        int f3;


        /// <summary>
        /// face normal vector
        /// </summary>
        bf2Vec3 n;

        /// <summary>
        /// face surface area
        /// </summary>
        float area;

        /// <summary>
        /// v1 corner angle
        /// </summary>
        float angle1;

        /// <summary>
        /// v2 corner angle
        /// </summary>
        float angle2;

        /// <summary>
        /// v3 corner angle
        /// </summary>
        float angle3;

        /// <summary>
        /// material hash (for comparing)
        /// </summary>
        int mathash;
    }
}
