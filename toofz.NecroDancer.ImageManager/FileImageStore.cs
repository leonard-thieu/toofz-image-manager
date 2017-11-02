#if FEATURE_IMAGEFILE
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#endif

namespace toofz.NecroDancer.ImageManager
{
    internal sealed class FileImageStore
    {
        public FileImageStore(string directory)
        {
            this.directory = directory;
        }

        private readonly string directory;

#if FEATURE_IMAGEFILE
        public async Task SaveImageAsync(ImageFile image)
        {
            var path = Path.Combine(directory, image.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = File.Create(path))
            {
                await fs.WriteAsync(image.Data, 0, image.Data.Length).ConfigureAwait(false);
            }
        }

        public async Task SaveImagesAsync(IEnumerable<ImageFile> images)
        {
            foreach (var image in images)
            {
                await SaveImageAsync(image).ConfigureAwait(false);
            }
        }
#endif
    }
}
