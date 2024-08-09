using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Repositories.IRepositories;
using static Utilities.Contants.Constants;

namespace WEB.CMS.Controllers
{
    public class AttachFileController : Controller
    {
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IAttachFileRepository _AttachFileRepository;

        public AttachFileController(IWebHostEnvironment hostEnvironment, IAttachFileRepository attachFileRepository)
        {
            _WebHostEnvironment = hostEnvironment;
            _AttachFileRepository = attachFileRepository;
        }

        public async Task<IActionResult> UploadFile(IFormFile[] attachFiles, long DataId, int Type)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            var ListModel = new List<AttachFile>();
            if (attachFiles != null && attachFiles.Length > 0)
            {
                foreach (var file in attachFiles)
                {
                    var model = new AttachFile()
                    {
                        DataId = DataId,
                        UserId = _UserLogin,
                        CreateDate = DateTime.Now,
                        Type = Type,
                        Ext = Path.GetExtension(file.FileName),
                        Capacity = file.Length
                    };

                    string _FileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string _UploadFolder = @"uploads/images";
                    string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                    if (!Directory.Exists(_UploadDirectory))
                    {
                        Directory.CreateDirectory(_UploadDirectory);
                    }

                    string filePath = Path.Combine(_UploadDirectory, _FileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                    model.Path = "/" + _UploadFolder + "/" + _FileName;
                    ListModel.Add(model);
                }
            }

            var rs = await _AttachFileRepository.CreateMultiple(ListModel);

            if (rs != null && rs.Count > 0)
            {
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Upload file thành công",
                    dataFile = JsonConvert.SerializeObject(rs)
                });
            }
            else
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Upload file thất bại"
                });
            }
        }

        public async Task<IActionResult> DeleteFile(long AttachId)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            var rs = await _AttachFileRepository.Delete(AttachId, _UserLogin);
            if (rs > 0)
            {
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Xóa file thành công",
                    dataFile = JsonConvert.SerializeObject(rs)
                });
            }
            else if (rs == -1)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Bạn không có quyền xóa file này",
                });
            }
            else
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Xóa file thất bại"
                });
            }
        }
    }
}
