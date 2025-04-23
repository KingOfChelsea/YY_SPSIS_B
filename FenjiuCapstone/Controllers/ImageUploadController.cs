using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    public class ImageUploadController : ApiController
    {
        [HttpPost]
        [Route("api/v1/ImageUpload")]
        
        public async Task<HttpResponseMessage> UploadImage()
        {
            // 检查是否是 multipart 请求
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "请使用 multipart/form-data 格式");
            }
            // 检查路径是否存在
            string root = HttpContext.Current.Server.MapPath("~/UploadedImages");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            //处理文件上传的核心类
            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var file in provider.FileData)
                {
                    var originalFileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var fileExtension = Path.GetExtension(originalFileName);
                    var newFileName = Guid.NewGuid().ToString("N") + fileExtension;
                    var newFilePath = Path.Combine(root, newFileName);

                    File.Move(file.LocalFileName, newFilePath);

                    var fileUrl = "/UploadedImages/" + newFileName;

                    return JsonResponseHelper.CreateJsonResponse(new
                    {
                        message = "上传成功",
                        fileName = newFileName,
                        filePath = fileUrl
                    });
                }

                return JsonResponseHelper.CreateJsonResponse(new { success = false,message="未检测到上传文件" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }
    }
}
