using System.IO;

namespace Mh
{
    public struct BMPFileHeader
    {
        public const ushort SIGNATURE = 0x4d42;    // "BM"의 리틀 엔디안
        public const uint HEADER_SIZE = 14;

        public ushort signature { get { return SIGNATURE; } }
        public uint filesize;
        public ushort reserved1;
        public ushort reserved2;
        public uint offset;

        public BMPFileHeader(ushort filesize = 0, uint offset = 0)
        {
            this.filesize = filesize;
            this.offset = offset;
            reserved1 = 0;
            reserved2 = 0;
        }

        public bool Read(BinaryReader reader)
        {
            if (reader == null)
            {
                return false;
            }

            uint magic = reader.ReadUInt16();
            if (magic != signature)
            {
                return false;
            }

            filesize = reader.ReadUInt32();
            reserved1 = reader.ReadUInt16();
            reserved2 = reader.ReadUInt16();
            offset = reader.ReadUInt32();

            return true;
        }

        public bool Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                return false;
            }

            writer.Write(signature);
            writer.Write(filesize);
            writer.Write(reserved1);
            writer.Write(reserved2);
            writer.Write(offset);

            return true;
        }
    }
}