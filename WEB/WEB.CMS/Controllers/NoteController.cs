using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aspose.Cells;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;

namespace WEB.CMS.Controllers
{
    public class NoteController : Controller
    {
        private readonly INoteRepository _NoteRepository;
        public NoteController(INoteRepository noteRepository)
        {
            _NoteRepository = noteRepository;
        }

        public async Task<IActionResult> CommentList(long DataId, int Type)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            var models = await _NoteRepository.GetListByType(DataId, Type);
            ViewBag.UserId = _UserLogin;
            return PartialView(models);
        }

        public async Task<IActionResult> Delete(long NoteId)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            var rs = await _NoteRepository.Delete(NoteId, _UserLogin);

            if (rs > 0)
            {
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Xóa bình luận thành công",
                    dataFile = JsonConvert.SerializeObject(rs)
                });
            }
            else if (rs == -1)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Bạn không có quyền xóa bình luận này",
                });
            }
            else if (rs == -2)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Thời gian hiệu lực để xóa bình luận đã hết",
                });
            }
            else
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Xóa bình luận thất bại"
                });
            }
        }

        public async Task<IActionResult> UpSert(long Id, string Comment, long DataId, int Type)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            var noteModel = new Note()
            {
                Id = Id,
                DataId = DataId,
                Comment = Comment,
                Type = Type,
                UserId = _UserLogin
            };

            var rs = await _NoteRepository.UpSert(noteModel);

            if (rs > 0)
            {
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Cập nhật bình luận thành công",
                    dataFile = JsonConvert.SerializeObject(rs)
                });
            }
            else
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Cập nhật bình luận thất bại"
                });
            }
        }
    }
}
