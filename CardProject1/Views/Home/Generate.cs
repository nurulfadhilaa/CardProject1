using System.IO;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using CardProject1.Models.Entities;

namespace CardProject1.Views.Home
{
    public class Generate
    {
        public async Task GenerateImageWithTextAsync(string imagePath, string fontPath, string outputPath, List<Tag> tags)
        {
            if (!System.IO.File.Exists(imagePath) || !System.IO.File.Exists(fontPath))
            {
                throw new FileNotFoundException("Image or font file not found.");
            }

            using (Image image = Image.Load(imagePath))
            {
                FontCollection fonts = new FontCollection();
                FontFamily fontFamily = fonts.Add(fontPath);
                Font font = fontFamily.CreateFont(24, FontStyle.Bold);

                foreach (var tag in tags)
                {
                    if (!string.IsNullOrEmpty(tag.Value))
                    {
                        image.Mutate(ctx => ctx.DrawText(tag.Value, font, Color.Red, new PointF(tag.XCoordinate, tag.YCoordinate)));
                    }
                }

                await image.SaveAsJpegAsync(outputPath);
            }
        }
    }
}
