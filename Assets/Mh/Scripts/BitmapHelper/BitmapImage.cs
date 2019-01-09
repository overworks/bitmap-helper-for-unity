using System.IO;
using UnityEngine;

namespace Mh
{
    public class BitmapImage
    {
        const ushort SIGNATURE = 0x4d42;    // "BM"의 리틀 엔디안

        public BMPFileHeader fileHeader;
        public IDIBHeader dibHeader;
        public byte[] rawData;

        //public uint maskR = 0x00ff0000;
        //public uint maskG = 0x0000ff00;
        //public uint maskB = 0x000000ff;
        //public uint maskA = 0x00000000;

        private Texture2D cachedTexture;

        public static implicit operator Texture2D(BitmapImage bitmapImage) { return bitmapImage.ToTexture2D(); }

        public Texture2D ToTexture2D()
        {
            if (dibHeader == null || rawData == null)
            {
                return null;
            }

            if (cachedTexture == null)
            {
                Texture2D texture = null;
                if (dibHeader.bpp == 32)
                {
                    texture = new Texture2D(dibHeader.width, dibHeader.height, TextureFormat.ARGB32, false);
                    texture.filterMode = FilterMode.Point;
                    texture.LoadRawTextureData(rawData);
                    texture.Apply();
                }
                else if (dibHeader.bpp == 24)
                {
                    int width = Mathf.Abs(dibHeader.width);
                    int height = Mathf.Abs(dibHeader.height);       // 음수가 들어갈 수도 있다.
                    Color32[] pixels = new Color32[width * height];

                    int padding = 0;
                    if ((width * 3) % 4 > 0)
                    {
                        padding = 4 - ((width * 3) % 4);
                    }

                    int i = 0;
                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            // 24비트 컬러는 BGR 순서다.
                            byte b = rawData[i];
                            byte g = rawData[i + 1];
                            byte r = rawData[i + 2];
                            pixels[x + (y * width)] = new Color32(r, g, b, 0xff);

                            i += 3;
                        }

                        i += padding;
                    }

                    texture = new Texture2D(dibHeader.width, dibHeader.height, TextureFormat.RGB24, false);
                    texture.filterMode = FilterMode.Point;
                    texture.SetPixels32(pixels);
                    texture.Apply();
                }
                else if (dibHeader.bpp == 16)
                {
                    int width = Mathf.Abs(dibHeader.width);
                    int height = Mathf.Abs(dibHeader.height);
                    byte[] data = rawData;

                    // 패딩이 있으면 다시 계산.
                    if ((width * 2) % 4 > 0)
                    {
                        data = new byte[width * height * 2];

                        int padding = 4 - ((width * 2) % 4);

                        int i = 0;
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                // 테스트 안해봄... 바이트를 바꾸거나 비트 시프트 해줘야 하던가?;;;
                                data[x + y * width] = rawData[i];
                                data[x + y * width + 1] = rawData[i + 1];

                                i += 2;
                            }

                            i += padding;
                        }
                    }
                    
                    texture = new Texture2D(dibHeader.width, dibHeader.height, TextureFormat.RGB565, false);
                    texture.filterMode = FilterMode.Point;
                    texture.LoadRawTextureData(data);
                    texture.Apply();
                }

                if (texture != null)
                {
                    cachedTexture = texture;
                }
            }
            return cachedTexture;
        }

        public bool Load(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return Load(stream);
            }
        }

        public bool Load(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Load(reader);
            }
        }

        public bool Load(BinaryReader reader)
        {
            if (!fileHeader.Read(reader) || fileHeader.signature != SIGNATURE)
            {
                return false;
            }

            uint dibHeaderSize = reader.ReadUInt32();
            reader.BaseStream.Seek(-4, SeekOrigin.Current);  // 4바이트 뒤로 돌림

            if (dibHeaderSize == 12)
            {
                dibHeader = new BMPCoreHeader();
            }
            else if (dibHeaderSize == 40)
            {
                dibHeader = new BMPInfoHeader();
            }

            if (dibHeader == null || !dibHeader.Read(reader))
            {
                return false;
            }

            // 지금은... 무압축만 처리하자...
            if (dibHeader.compression != BitmapCompression.BI_RGB)
            {
                Debug.LogWarning("Sorry, now BMP Loader can support Uncompressed bitmap file only.");
                return false;
            }

            // 안쓰이지만 일단...
            //if (dibHeader.compression == BMPCompression.BI_BITFIELDS || dibHeader.compression == BMPCompression.BI_ALPHABITFIELDS)
            //{
            //    maskR = reader.ReadUInt32();
            //    maskG = reader.ReadUInt32();
            //    maskB = reader.ReadUInt32();

            //    if (dibHeader.compression == BMPCompression.BI_ALPHABITFIELDS)
            //    {
            //        maskA = reader.ReadUInt32();
            //    }
            //}

            reader.BaseStream.Seek(fileHeader.offset, SeekOrigin.Begin);
            uint rawImageSize = dibHeader.rawImageSize;
            if (rawImageSize == 0)
            {
                // 헤더에 포함된 image size가 더미 데이터 0이 들어가있을 수도 있다.
                rawImageSize = (uint)(reader.BaseStream.Length - reader.BaseStream.Position);
            }
            rawData = reader.ReadBytes((int)rawImageSize);

            return true;
        }
    }

}