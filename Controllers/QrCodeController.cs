using System;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ZXing;
using ZXing.Common;


namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private readonly IImageService _imageService;


        public QrCodeController(
            IImageService imageService
        )
        {
            _imageService = imageService;
        }

        public IActionResult Get([FromQuery] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest();
            }

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(10);

            var stream = new MemoryStream();
            qrCodeBitmap.Save(stream, ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "image/png", $"{Guid.NewGuid().ToString("N")}.png");
        }

        [HttpGet]
        [Route("Barcode")]
        public IActionResult Barcode(string code)
        {
            var writer = new BarcodeWriterPixelData()
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions                
                {
                    Height = 50,
                    Width = 300,
                    NoPadding = true,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(code);
            Byte[] byteArray;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                var ms = new MemoryStream();
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image   
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG   
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "image/png", $"{Guid.NewGuid().ToString("N")}.png");
            }
        }
    }
}
