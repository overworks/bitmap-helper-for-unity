using System.IO;

namespace Mh
{
    public class BitmapLoader
    {
        public static BitmapImage Load(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return Load(stream);
            }
        }

        public static BitmapImage Load(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Load(reader);
            }
        }

        public static BitmapImage Load(BinaryReader reader)
        {
            BitmapImage image = new BitmapImage();
            if (image.Load(reader))
            {
                return image;
            }

            return null;
        }


    }
}