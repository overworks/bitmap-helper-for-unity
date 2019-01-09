using System.IO;

namespace Mh
{
    public enum BitmapCompression : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5,
        BI_ALPHABITFIELDS = 6,

        BI_CMYK = 11,
        BI_CMYKRLE = 12,
        BI_CMYKRLD = 13,
    }

    public interface IDIBHeader
    {
        uint headerSize { get; }
        int width { get; }
        int height { get; }
        ushort bpp { get; }
        BitmapCompression compression { get; }
        uint rawImageSize { get; }         // can be 0
        uint numPalette { get; }

        bool Read(BinaryReader reader);
        bool Write(BinaryWriter writer);
    }

    public class BitmapCoreHeader : IDIBHeader
    {
        public const uint HEADER_SIZE = 12;
        public const ushort PLANE_COUNT = 1;

        public uint headerSize { get { return HEADER_SIZE; } }    // size of header. 12
        public int width { get; set; }          // 원래는 부호없는 16비트 타입이지만 인터페이스와 맞추기 위해...
        public int height { get; set; }
        public ushort planes { get { return PLANE_COUNT; } }      // must be 1
        public ushort bpp { get; set; }         // bit per pixel. 1, 2, 4, 8, 16, 24, 32.
        public BitmapCompression compression { get { return BitmapCompression.BI_RGB; } }
        public uint rawImageSize { get { return 0; } }
        public uint numPalette { get { return 0; } }

        public bool Read(BinaryReader reader)
        {
            if (reader == null)
            {
                return false;
            }

            uint headerSize = reader.ReadUInt32();

            if (headerSize != HEADER_SIZE)
            {
                //throw new IOException("DIB Header size is invalid.");
                return false;
            }

            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();
            ushort planes = reader.ReadUInt16();

            if (planes != PLANE_COUNT)
            {
                //throw new IOException("plane must be 1.");
                return false;
            }

            ushort bpp = reader.ReadUInt16();

            if (bpp != 1 && bpp != 2 && bpp != 4 && bpp != 8 && bpp != 16 && bpp != 24 && bpp != 32)
            {
                //throw new IOException("bpp is invalid.");
                return false;
            }

            this.width = width;
            this.height = height;
            this.bpp = bpp;

            return true;
        }

        public bool Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                return false;
            }

            writer.Write(headerSize);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            writer.Write(planes);
            writer.Write(bpp);

            return true;
        }
    }

    public class BitmapInfoHeader : IDIBHeader
    {
        public const uint HEADER_SIZE = 40;
        public const ushort PLANE_COUNT = 1;

        public uint headerSize { get { return HEADER_SIZE; } }    // size of header. 40.
        public int width { get; set; }
        public int height { get; set; }
        public ushort planes { get { return PLANE_COUNT; } }      // must be 1
        public ushort bpp { get; set; }         // bit per pixel. 1, 2, 4, 8, 16, 24, 32.
        public BitmapCompression compression { get; set; }
        public uint rawImageSize { get; set; }     // can be 0
        public int xPPM { get; set; }
        public int yPPM { get; set; }
        public uint numPalette { get; set; }
        public uint numImportant { get; set; }

        public bool Read(BinaryReader reader)
        {
            if (reader == null)
            {
                return false;
            }

            uint headerSize = reader.ReadUInt32();
            if (headerSize != 40)
            {
                //throw new IOException("DIB Header size is invalid.");
                return false;
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            ushort planes = reader.ReadUInt16();
            if (planes != PLANE_COUNT)
            {
                //throw new IOException("plane must be 1.");
                return false;
            }

            ushort bpp = reader.ReadUInt16();
            if (bpp != 1 && bpp != 2 && bpp != 4 && bpp != 8 && bpp != 16 && bpp != 24 && bpp != 32)
            {
                //throw new IOException("bpp is invalid.");
                return false;
            }

            uint compression = reader.ReadUInt32();
            uint rawImageSize = reader.ReadUInt32();
            int xPPM = reader.ReadInt32();
            int yPPM = reader.ReadInt32();
            uint palette = reader.ReadUInt32();
            uint important = reader.ReadUInt32();

            this.width = width;
            this.height = height;
            this.bpp = bpp;
            this.compression = (BitmapCompression)compression;
            this.rawImageSize = rawImageSize;
            this.xPPM = xPPM;
            this.yPPM = yPPM;
            this.numPalette = palette;
            this.numImportant = important;

            return true;
        }

        public bool Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                return false;
            }

            writer.Write(headerSize);
            writer.Write(width);
            writer.Write(height);
            writer.Write(planes);
            writer.Write(bpp);
            writer.Write((uint)compression);
            writer.Write(rawImageSize);
            writer.Write(xPPM);
            writer.Write(yPPM);
            writer.Write(numPalette);
            writer.Write(numImportant);

            return true;
        }
    }

    // BitmpCoreHeader와 BitmpInfoHeader 외에도 여러가지 헤더 포맷이 있지만 일단 보류.
}