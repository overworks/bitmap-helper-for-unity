using System.IO;
using UnityEngine;

// 사실 UnityEngine 네임스페이스에 넣어야 될 것 같은데 그러면 안되겠지...
namespace Mh
{
    public static class Texture2DExtension
    {
        // EncodeToPNG...등과 인터페이스를 맞춤.
        public static byte[] EncodeToBMP(this Texture2D tex)
        {
            if (!tex.isReadable)
            {
                Debug.LogError("Texture is not readable.");
                return null;
            }

            TextureFormat format = tex.format;
            if (format != TextureFormat.RGB24 && format != TextureFormat.ARGB32)
            {
                // 일단은 24비트 RGB와 32비트 ARGB만... 다른 메서드들도 그러고...
                Debug.LogError("EncodeToBMP support RGB24 or ARGB32.");
                return null;
            }

            const ushort SIGNATURE = 0x4d42; // "BM"의 리틀 엔디언
            const int FILE_HEADER_SIZE = 14;
            const int INFO_HEADER_SIZE = 40;

            int width = tex.width;
            int height = tex.height;

            ushort bpp = 0;
            if (format == TextureFormat.RGB24)
            {
                bpp = 24;
            }
            else if (format == TextureFormat.ARGB32)
            {
                bpp = 32;
            }

            int pitch = bpp / 8 * width;
            int pad = 0;
            if ((pitch % 4) > 0)
            {
                pad = 4 - (pitch % 4);
                pitch += pad;
            }

            // 헤더의 크기가 54이므로 2를 더해줘야 4의 배수가 된다.
            int fileSize = FILE_HEADER_SIZE + INFO_HEADER_SIZE + (pitch * height) + 2;

            using (MemoryStream stream = new MemoryStream(fileSize))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // File Header
                    writer.Write(SIGNATURE);        // 파일 시그니처
                    writer.Write((uint)fileSize);   // 파일 크기
                    writer.Write((ushort)0);        // 예약1
                    writer.Write((ushort)0);        // 예약2
                    writer.Write((uint)(FILE_HEADER_SIZE + INFO_HEADER_SIZE));  // 오프셋

                    // Info Header
                    writer.Write((uint)INFO_HEADER_SIZE);   // 인포 헤더 크기
                    writer.Write(width);            // 너비
                    writer.Write(height);           // 높이
                    writer.Write((ushort)1);        // 플레인 번호(1 고정)
                    writer.Write(bpp);              // 픽셀당 비트 수
                    writer.Write((uint)0);          // 압축 방식(0: BI_RGB)
                    writer.Write((uint)(pitch * height));   // 이미지 데이터 크기. 0으로 해도 되지만...
                    writer.Write(2834);             // 가로 pixel per meter. 72dpi면 2834.64...이므로
                    writer.Write(2834);             // 세로 pixel per meter.
                    writer.Write((uint)0);          // 팔레트 컬러 수
                    writer.Write((uint)0);          // important 수

                    Color32[] pixels = tex.GetPixels32();
                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            Color32 pixel = pixels[y * width + x];
                            writer.Write(pixel.b);
                            writer.Write(pixel.g);
                            writer.Write(pixel.r);

                            // 픽셀마다 조건문 거는게 맘에 안드는데... 별도의 함수로 만들기도 좀 그렇고.
                            if (format == TextureFormat.ARGB32)
                                writer.Write(pixel.a);
                        }

                        for (int i = 0; i < pad; ++i)
                        {
                            writer.Write((byte)0);
                        }
                    }

                    writer.Write((ushort)0);    // 마지막으로 2바이트를 0으로 채움.
                    writer.Close();
                }
                return stream.ToArray();
            }
        }
    }
}