using System.IO;
using System.Collections.Generic;

namespace Stride.Assets.Models.bf2Importer.BFP4FExplorerWV
{
    public class BF2CollisionMesh
    {
        public Helper.BF2CollisionMeshHeader header;
        public List<Helper.BF2CollisionMeshGeometry> geometry;
        public BF2CollisionMesh(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            header = new Helper.BF2CollisionMeshHeader(m);
            geometry = new List<Helper.BF2CollisionMeshGeometry>();
            for (int i = 0; i < (int)header.numChunks; i++)
                geometry.Add(new Helper.BF2CollisionMeshGeometry(m, header.version));
        }


        //public List<RenderObject> ConvertForEngine(Engine3D engine)
        //{
        //    List<RenderObject> result = new List<RenderObject>();
        //    foreach(Helper.BF2CollisionMeshGeometry geom in geometry)
        //        foreach(Helper.BF2CollisionMeshSubGeometry subgeo in geom.subGeom)
        //            foreach(Helper.BF2CollisionMeshColData col in subgeo.colData)
        //            {
        //                RenderObject o = new RenderObject(engine.device, RenderObject.RenderType.TriListWired, engine.defaultTexture, engine);
        //                List<RenderObject.VertexWired> list = new List<RenderObject.VertexWired>();
        //                for(int i=0;i<col.numFaces;i++)
        //                {
        //                    ushort[] face = col.faces[i];
        //                    list.Add(new RenderObject.VertexWired(col.vertices[face[0]].ToRawVec3(), Color4.Black));
        //                    list.Add(new RenderObject.VertexWired(col.vertices[face[1]].ToRawVec3(), Color4.Black));
        //                    list.Add(new RenderObject.VertexWired(col.vertices[face[2]].ToRawVec3(), Color4.Black));
        //                }
        //                o.verticesWired = list.ToArray();
        //                if (list.Count != 0)
        //                {
        //                    o.InitGeometry();
        //                    result.Add(o);
        //                }
        //            }
        //    return result;
        //}
    }
}
