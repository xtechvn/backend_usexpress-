using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WEB.CMS.ViewComponents
{
    public class NoteViewComponent : ViewComponent
    {
        private readonly INoteRepository _NoteRepository;
        public NoteViewComponent(INoteRepository noteRepository)
        {
            _NoteRepository = noteRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(long DataId, int Type)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            var models = await _NoteRepository.GetListByType(DataId, Type);
            ViewBag.DataId = DataId;
            ViewBag.Type = Type;
            ViewBag.UserId = _UserLogin;
            return View(models);
        }
    }
}
