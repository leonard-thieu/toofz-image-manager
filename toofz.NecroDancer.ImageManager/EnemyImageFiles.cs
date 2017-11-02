using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using toofz.NecroDancer.Data;

namespace toofz.NecroDancer.ImageManager
{
    internal sealed class EnemyImageFiles
    {
        private static Bitmap GetFrameImage(Image image, Rectangle srcRect)
        {
            var bitmap = new Bitmap(srcRect.Width, srcRect.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                var destRect = new Rectangle(Point.Empty, srcRect.Size);
                g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);

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

        public EnemyImageFiles(string dataDirectory, Enemy enemy)
        {
            Enemy = enemy;
            var path = Path.Combine(dataDirectory, enemy.SpriteSheet.Path);
            var frameCount = enemy.SpriteSheet.FrameCount;

            using (var image = Image.FromFile(path))
            {
                var width = enemy.SpriteSheet.FrameWidth;
                var height = enemy.SpriteSheet.FrameHeight;

                var columns = image.Width / width;
                var rows = image.Height / height;

                for (int y = 0; y < rows; y++)
                {
                    var srcY = y * height;

                    for (int x = 0; x < columns; x++)
                    {
                        var srcX = x * width;

                        var srcRect = new Rectangle(srcX, srcY, width, height);
                        using (var frameImage = GetFrameImage(image, srcRect))
                        {
                            Frames.Add(ResizeImage(frameImage, 48, 48));
                        }
                    }
                }
            }
        }

        public Enemy Enemy { get; }
        public List<Bitmap> Frames { get; } = new List<Bitmap>();

        public Bitmap GetBitmap(Frame frame)
        {
            return Frames[frame.InSheet - 1];
        }
    }
}
