using System.Drawing;
using System.IO;
using toofz.NecroDancer.Data;

namespace toofz.NecroDancer.ImageManager
{
    internal sealed class ItemImageFiles : ImageFiles
    {
        public ItemImageFiles(string dataDirectory, Item item) :
            base(dataDirectory, "items", Path.Combine("items", item.ImagePath), item.Name, item.FrameCount)
        {
            Item = item;
        }

        public Item Item { get; }

        protected override Size GetFrameSize(Image image)
        {
            // Items always have 2 rows of sprites. Each sprite is equally sized.
            //   - Top:    Normal
            //   - Bottom: Shadow
            return new Size
            {
                Width = image.Width / FrameCount,
                Height = image.Height / 2,
            };
        }
    }
}
