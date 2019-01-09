using System.IO;
using UnityEngine;

namespace Mh
{
    public static class BitmapUtility
    {
        public static bool SaveToBMP(this Texture2D tex, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("path is null or empty");
                return false;
            }

            if (!tex.isReadable)
            {
                Debug.LogError("Texture is not readable.");
                return false;
            }

            ushort bpp = 0;
            BitmapCompression bmpCompression = BitmapCompression.BI_RGB;

            // 지금은 RGB와 ARGB만 하자.
            switch (tex.format)
            {
                case TextureFormat.ARGB32:
                    bpp = 32;
                    bmpCompression = BitmapCompression.BI_RGB;
                    break;

                case TextureFormat.RGB24:
                    bpp = 24;
                    bmpCompression = BitmapCompression.BI_RGB;
                    break;

                case TextureFormat.RGB565:
                    bpp = 16;
                    bmpCompression = BitmapCompression.BI_RGB;
                    break;

                default:
                    Debug.LogError("Unsupported texture format.");
                    return false;
            }


            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    byte[] rawData = tex.GetRawTextureData();
                    uint rawImageSize = (uint)rawData.Length;

                    BMPFileHeader fileHeader = default(BMPFileHeader);
                    fileHeader.filesize = BMPFileHeader.HEADER_SIZE + BMPInfoHeader.HEADER_SIZE + rawImageSize;
                    fileHeader.reserved1 = 0;
                    fileHeader.reserved2 = 0;
                    fileHeader.offset = BMPFileHeader.HEADER_SIZE + BMPInfoHeader.HEADER_SIZE;
                    fileHeader.Write(writer);

                    // 유니티 내부의 raw data가 패딩을 하나 안하나 모르겠네...
                    BMPInfoHeader infoHeader = new BMPInfoHeader();
                    infoHeader.headerSize = BMPInfoHeader.HEADER_SIZE;
                    infoHeader.width = tex.width;
                    infoHeader.height = tex.height;
                    infoHeader.planes = 1;
                    infoHeader.bpp = bpp;
                    infoHeader.compression = bmpCompression;
                    infoHeader.rawImageSize = rawImageSize;
                    infoHeader.xPPM = 2835;     // 72 dpi
                    infoHeader.yPPM = 2835;     // 72 dpi
                    infoHeader.numPalette = 0;
                    infoHeader.numImportant = 0;
                    infoHeader.Write(writer);

                    writer.Write(rawImageSize);
                    writer.Close();
                }
            }

            return false;
        }
    }
}