using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PictureController : ControllerBase
    {
        private readonly IImageService _imageService;


        public PictureController(
            IImageService imageService
        )
        {
            _imageService = imageService;
        }

        [HttpPost]
        [Route("Upload")]
        public ActionResult<UploadPictureViewModel> Upload([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new UploadPictureViewModel() { Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new UploadPictureViewModel() { Error = "{missing-file}" };
            }
            var code = Guid.NewGuid().ToString("N");
            var folderPathLevel1 = code.Substring(0, 2);
            var folderPathLevel2 = code.Substring(2, 2);
            var folderPathLevel3 = code.Substring(4, 2);
            var filePath = string.Format("{0}/{1}/{2}", folderPathLevel1, folderPathLevel2, folderPathLevel3);
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            var fileName = string.Format("{0}{1}", code, extension);

            // var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\" + filePath.Replace("/", "\\"));
            var path = Path.Combine("C:\\webapps\\isale-images", filePath.Replace("/", "\\"));
            var pathAndFileName = Path.Combine(path, fileName);
            path = path.Replace("\\", "/");
            pathAndFileName = pathAndFileName.Replace("\\", "/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                _imageService.ResizeImage(file, 800, pathAndFileName);
            }
            catch (System.Exception)
            {
                return new UploadPictureViewModel() { Error = "{missing-file}" };
            }

            //var cdnServer = "http://localhost:5001/images";
            var cdnServer = "https://images.isale.online";
            var url = string.Format("{0}/{1}/{2}", cdnServer, filePath, fileName);
            return new UploadPictureViewModel() { Url = url };
        }

        [HttpPost]
        [Route("UploadBase64")]
        public ActionResult<UploadPictureViewModel> UploadBase64(UploadBase64ViewModel uploadImageModel)
        {
            if (uploadImageModel == null || string.IsNullOrWhiteSpace(uploadImageModel.Base64))
            {
                return new UploadPictureViewModel() { Error = "{missing-file}" };
            }
            try
            {
                var code = Guid.NewGuid().ToString("N");
                var folderPathLevel1 = code.Substring(0, 2);
                var folderPathLevel2 = code.Substring(2, 2);
                var folderPathLevel3 = code.Substring(4, 2);
                var filePath = string.Format("{0}/{1}/{2}", folderPathLevel1, folderPathLevel2, folderPathLevel3);
                var extension = ".jpg";
                var fileName = string.Format("{0}{1}", code, extension);

                var path = Path.Combine("C:\\webapps\\isale-images", filePath.Replace("/", "\\"));
                var pathAndFileName = Path.Combine(path, fileName);
                path = path.Replace("\\", "/");
                pathAndFileName = pathAndFileName.Replace("\\", "/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                try
                {
                    string converted = uploadImageModel.Base64.Replace('-', '+');
                    converted = converted.Replace('_', '/');
                    _imageService.ResizeImage(converted, 800, pathAndFileName);
                }
                catch (System.Exception)
                {
                    return new UploadPictureViewModel() { Error = "{missing-file}" };
                }

                //var cdnServer = "http://localhost:5001/images";
                var cdnServer = "https://images.isale.online";
                var url = string.Format("{0}/{1}/{2}", cdnServer, filePath, fileName);
                return new UploadPictureViewModel() { Url = url };
            }
            catch (System.Exception)
            {
                return new UploadPictureViewModel() { Error = "{missing-file}" };
            }
        }
    }
}
