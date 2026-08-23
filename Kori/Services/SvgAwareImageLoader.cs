using System;
using System.IO;
using System.Threading.Tasks;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Svg.Skia;

namespace Kori.Services;

public sealed class SvgAwareImageLoader : RamCachedWebImageLoader
{
    private const int RasterSize = 128;

    protected override async Task<Bitmap?> LoadAsync(string url)
    {
        var bytes = await LoadDataFromExternalAsync(url).ConfigureAwait(false);
        if (bytes is null)
        {
            return null;
        }

        try
        {
            var bitmap = IsSvg(url, bytes)
                ? RasterizeSvg(bytes)
                : DecodeRaster(bytes);

            if (bitmap is not null)
            {
                await SaveToGlobalCache(url, bytes).ConfigureAwait(false);
            }

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSvg(string url, byte[] bytes)
    {
        if (url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var text = System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 512));
        return text.TrimStart('﻿', ' ', '\t', '\r', '\n').StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
               || text.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }

    private static Bitmap DecodeRaster(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new Bitmap(stream);
    }

    private static Bitmap? RasterizeSvg(byte[] bytes)
    {
        using var svg = new SKSvg();
        using var stream = new MemoryStream(bytes);
        var picture = svg.Load(stream);
        if (picture is null)
        {
            return null;
        }

        var bounds = picture.CullRect;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        var scale = RasterSize / Math.Max(bounds.Width, bounds.Height);
        using var skBitmap = picture.ToBitmap(
            SKColors.Transparent,
            scale,
            scale,
            SKColorType.Rgba8888,
            SKAlphaType.Premul,
            SKColorSpace.CreateSrgb());

        if (skBitmap is null)
        {
            return null;
        }

        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var pngStream = data.AsStream();
        return new Bitmap(pngStream);
    }
}
