//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using SkiaSharp;

namespace Spune.UIShared.Functions;

/// <summary>
/// This class contains image functions.
/// </summary>
public static class ImageFunction
{
    /// <summary>
    /// Converts an image with an SVG in it to a PNG.
    /// </summary>
    /// <param name="image">Image containing the SVG.</param>
    /// <param name="scaleX">X scale of the conversion to apply.</param>
    /// <param name="scaleY">Y scale of the conversion to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SvgToPngAsync(Image image, float scaleX, float scaleY)
    {
        if (image.Source is not SvgImage svgImage || svgImage.Source is null) return;
        await using var ms = new MemoryStream();
        using (var skBitmap = new SKBitmap((int)(svgImage.Size.Width * scaleX), (int)(svgImage.Size.Height * scaleY)))
        {
            using SKCanvas canvas = new(skBitmap);
            canvas.Scale(scaleX, scaleY);
            canvas.DrawPicture(svgImage.Source.Picture);
            canvas.Flush();
            canvas.Save();

            using var skImage = SKImage.FromBitmap(skBitmap);
            using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(ms);
        }

        ms.Position = 0;
        image.Source = new Bitmap(ms);
    }

    /// <summary>
    /// Loads the not available image.
    /// </summary>
    /// <returns>The image.</returns>
    public static SvgImage LoadNoImage()
    {
        using var stream = AssetLoader.Open(new Uri("avares://Spune.UIShared/Images/NoImage.svg"));
        var svgImage = new SvgImage { Source = SvgSource.LoadFromStream(stream) };
        return svgImage;
    }
}