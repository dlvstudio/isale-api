using Microsoft.AspNetCore.Http;

public interface IImageService
{
    void ResizeImage(IFormFile file, int maxWidth, string fileNameWithPath);
    void ResizeImage(string base64, int maxWidth, string fileNameWithPath);
}