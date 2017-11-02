using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace toofz.NecroDancer.ImageManager
{
    internal abstract class ImageFiles : IEnumerable<Bitmap>
    {
        protected ImageFiles(string dataDirectory, string category, string imagePath, string name, int frameCount)
        {
            path = Path.Combine(dataDirectory, imagePath);
            baseName = Path.Combine(category, name);
            FrameCount = frameCount;

            using (var image = Image.FromFile(path))
            {
                var size = GetFrameSize(image);
                var width = size.Width;
                var height = size.Height;

                int srcX;
                int srcY;

                for (int y = 0; y < 2; y++)
                {
                    srcY = y * height;

                    for (int x = 0; x < FrameCount; x++)
                    {
                        srcX = x * width;
                        var i = (y * FrameCount) + x;

                        using (var frameImage = GetFrameImage(image, srcX, srcY, width, height))
                        {
                            frames.Add(ResizeImage(frameImage, 36, 36));
                        }
                    }
                }
            }
        }

        private readonly List<Bitmap> frames = new List<Bitmap>();
        private readonly string path;
        private readonly string baseName;
        protected int FrameCount { get; }

        protected abstract Size GetFrameSize(Image image);

        public IEnumerator<Bitmap> GetEnumerator() => frames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static Bitmap GetFrameImage(Image image, int srcX, int srcY, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                var destRect = new Rectangle(0, 0, width, height);
                g.DrawImage(image, destRect, srcX, srcY, width, height, GraphicsUnit.Pixel);

                return bitmap;
            }
        }

        private static Bitmap ResizeImage(Bitmap image, int width, int height, float minScale = 1)
        {
            var pageUnit = GraphicsUnit.Pixel;
            var srcRect = image.GetBounds(ref pageUnit);

            var scaleX = (float)width / image.Width;
            var scaleY = (float)height / image.Height;
            var scale = new[] { scaleX, scaleY, minScale }.Min();
            scale = Math.Max(scale, float.Epsilon);

            var destWidth = image.Width * scale;
            var destHeight = image.Height * scale;
            var destX = (width - destWidth) / 2;
            var destY = (height - destHeight) / 2;
            var destRect = new RectangleF(destX, destY, destWidth, destHeight);

            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image, destRect, srcRect, pageUnit);
            }

            return bitmap;
        }
    }
}
