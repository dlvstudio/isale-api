using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.Primitives;
using System;
using System.IO;

public class ImageService : IImageService
{
    public void ResizeImage(string base64, int maxWidth, string fileNameWithPath) 
    { 
        byte[] bytes = Convert.FromBase64String(base64);
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            using (Image<Rgba32> image = Image.Load(ms))
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions(){
                        Size = new Size(maxWidth, 0),
                        Mode = ResizeMode.Max
                    }));
                image.Save(fileNameWithPath);
            }
        }
    }

    public void ResizeImage(IFormFile file, int maxWidth, string fileNameWithPath) 
    { 
        var extension = file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
        if (extension.ToLower() == "gif") {
            ResizeGifImage(file, maxWidth, fileNameWithPath);
            return;
        }
        ResizeNormalImage(file, maxWidth, fileNameWithPath);
    }

    private void ResizeNormalImage(IFormFile file, int maxWidth, string fileNameWithPath) {
        using (var stream = file.OpenReadStream()) {
            using (Image<Rgba32> image = Image.Load(stream))
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions(){
                        Size = new Size(maxWidth, 0),
                        Mode = ResizeMode.Max
                    }));
                image.Save(fileNameWithPath);
            }
        }
    } 

    private void ResizeGifImage(IFormFile file, int maxWidth, string fileNameWithPath) {
        using (var stream = file.OpenReadStream()) {
            using (var image = Image.Load(stream))
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions(){
                        Size = new Size(maxWidth, 0),
                        Mode = ResizeMode.Max
                    }));
                var encoder = new GifEncoder()
                {
                    ColorTableMode = GifColorTableMode.Global,
                    Quantizer = new OctreeQuantizer(64)
                };
                image.Save(fileNameWithPath, encoder);
            }
        }
    } 
}