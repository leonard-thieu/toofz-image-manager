using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using toofz.NecroDancer.Data;

namespace toofz.NecroDancer.ImageManager
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var dataDirectory = args[0];

            var serializer = new NecroDancerDataSerializer();
            NecroDancerData data;
            using (var fs = File.OpenRead(Path.Combine(dataDirectory, "necrodancer.xml")))
            {
                data = serializer.Deserialize(fs);
            }

            var enemyImageFiles = new List<EnemyImageFiles>();
            foreach (var enemy in data.Enemies)
            {
                enemyImageFiles.Add(new EnemyImageFiles(dataDirectory, enemy));
            }

            var imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
            var enemiesDirectory = Path.Combine(imageDirectory, "enemies");
            Directory.CreateDirectory(enemiesDirectory);

            var tasks = new List<Task>();
            foreach (var spritesheet in enemyImageFiles)
            {
                var firstFrame = (from f in spritesheet.Enemy.Frames
                                  where f.AnimType == "normal"
                                  orderby f.InAnim
                                  select spritesheet.Frames[f.InSheet - 1])
                                  .FirstOrDefault();
                if (firstFrame == null)
                {
                    firstFrame = spritesheet.Frames.First();
                }

                var baseFileName = $"{spritesheet.Enemy.Name}{spritesheet.Enemy.Type}";
                var filePath = Path.Combine(enemiesDirectory, $"{baseFileName}.png");
                firstFrame.Save(filePath);

                var gifTask = SaveAnimatedImage(enemiesDirectory, spritesheet);
                tasks.Add(gifTask);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static Task SaveAnimatedImage(string enemiesDirectory, EnemyImageFiles spritesheet)
        {
            var frames = (from f in spritesheet.Enemy.Frames
                          where f.AnimType == "normal"
                          orderby f.InAnim
                          select spritesheet.Frames[f.InSheet - 1])
                          .ToList();
            if (!frames.Any())
            {
                var frameCount = spritesheet.Frames.Count > 2 ? 2 : 1;
                frames = spritesheet.Frames.Take(frameCount).ToList();
            }

            var baseFileName = $"{spritesheet.Enemy.Name}{spritesheet.Enemy.Type}";
            var filePath = Path.Combine(enemiesDirectory, $"{baseFileName}.gif");

            return Task.Run(() =>
            {
                using (var collection = new MagickImageCollection())
                {
                    var frameDelay = (int)((1.08 / frames.Count) * 100);
                    foreach (var frame in frames)
                    {
                        byte[] data = null;
                        using (var ms = new MemoryStream())
                        {
                            frame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            data = ms.ToArray();
                        }
                        var magickImage = new MagickImage(data);
                        magickImage.AnimationDelay = frameDelay;
                        collection.Add(magickImage);
                    }

                    // Optionally reduce colors
                    var settings = new QuantizeSettings();
                    settings.Colors = 256;
                    collection.Quantize(settings);

                    // Optionally optimize the images (images should have the same size).
                    collection.OptimizePlus();

                    // Save gif
                    collection.Write(filePath);
                }
            });
        }
    }
}
