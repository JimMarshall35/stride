using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.Assets.Models.bf2Importer.new_importer
{
    public static class StreamHelpers
    {
        public static ushort ReadU16(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static uint ReadU32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static int ReadS32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public static ulong ReadU64(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static bf2Vec2 ReadVector2(Stream s)
        {
            bf2Vec2 result = new bf2Vec2();
            result.x = ReadFloat(s);
            result.y = ReadFloat(s);
            return result;
        }

        public static bf2Vec3 ReadVector3(Stream s)
        {
            bf2Vec3 result = new bf2Vec3();
            result.x = ReadFloat(s);
            result.y = ReadFloat(s);
            result.z = ReadFloat(s);
            return result;
        }

        public static string ReadCString(Stream s)
        {
            uint len = ReadU32(s);
            byte[] data = new byte[len];
            s.Read(data, 0, (int)len);
            return Encoding.ASCII.GetString(data);
        }

        public static string ReadTString(Stream s)
        {
            MemoryStream m = new MemoryStream();
            byte b = 0;
            while ((b = (byte)s.ReadByte()) != 0xA)
                m.WriteByte(b);
            return Encoding.ASCII.GetString(m.ToArray());
        }
    }
}
