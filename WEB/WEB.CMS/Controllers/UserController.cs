using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IUserRepository _UserRepository;
        private readonly IRoleRepository _RoleRepository;
        private readonly IMFARepository _mFARepository;
        public UserController(IUserRepository userRepository, IRoleRepository roleRepository, IWebHostEnvironment hostEnvironment, IMFARepository mFARepository)
        {
            _UserRepository = userRepository;
            _RoleRepository = roleRepository;
            _WebHostEnvironment = hostEnvironment;
            _mFARepository = mFARepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> GetUserSuggestionList(string name)
        {
            try
            {
                var userlist = await _UserRepository.GetUserSuggestionList(name);
                var suggestionlist = userlist.Select(s => new
                {
                    id = s.Id,
                    name = s.UserName
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        public IActionResult Search(string userName, string strRoleId, int status = -1, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<UserGridModel>();
            try
            {
                model = _UserRepository.GetPagingList(userName, strRoleId, status, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }

        public async Task<IActionResult> AddOrUpdate(int Id, bool IsClone = false)
        {
            var model = new User();
            if (Id != 0)
            {
                model = await _UserRepository.FindById(Id);
                if (IsClone)
                {
                    model = new User
                    {
                        FullName = model.FullName,
                        UserName = model.UserName,
                        Email = model.Email,
                        Address = model.Address,
                        BirthDay = model.BirthDay,
                        Gender = model.Gender,
                        Status = model.Status,
                        Note = model.Note,
                        Phone = model.Phone,
                    };
                }
            }
            else
            {
                model.Gender = 1;
            }

            ViewBag.RoleList = await _RoleRepository.GetAll();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpSert(IFormFile imagefile, UserViewModel model)
        {
            try
            {
                string imageUrl = string.Empty;
                if (imagefile != null)
                {
                    string _FileName = Guid.NewGuid() + Path.GetExtension(imagefile.FileName);
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
                            await imagefile.CopyToAsync(fileStream);
                        }
                    }
                    model.Avata = "/" + _UploadFolder + "/" + _FileName;
                }

                int rs = 0;
                if (model.Id != 0)
                {
                    rs = await _UserRepository.Update(model);
                }
                else
                {
                    rs = await _UserRepository.Create(model);
                }

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tên đăng nhập hoặc email đã tồn tại"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDetail(int Id)
        {
            var model = new UserDetailViewModel();
            try
            {
                model = await _UserRepository.GetDetailUser(Id);
                var mfa_record = await _mFARepository.get_MFA_DetailByUserID(Id);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                ViewBag.IsMFAEnabled = (mfa_record != null && mfa_record.UserId == Id);
            }
            catch
            {
                ViewBag.IsMFAEnabled = false;
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int userId, int[] arrRole, int type)
        {
            try
            {
                var rs = await _UserRepository.UpdateUserRole(userId, arrRole, type);

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserStatus(int id)
        {
            try
            {
                var rs = await _UserRepository.ChangeUserStatus(id);
                if (rs != -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        status = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordByUserId(int userId)
        {
            try
            {
                var rs = await _UserRepository.ResetPasswordByUserId(userId);
                if (!string.IsNullOrEmpty(rs))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> UserProfile()
        {
            var model = new User();
            int userid = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                userid = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                model = await _UserRepository.FindById(userid);
            }
            ViewBag.RoleList = await _RoleRepository.GetRoleListByUserId(userid);
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserPasswordModel model)
        {
            try
            {
                var rs = await _UserRepository.ChangePassword(model);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Mật khẩu hiện tại không chính xác",
                        result = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResetMFA(long id)
        {
            try
            {
                var detail = await _mFARepository.get_MFA_DetailByUserID(id);
                detail.Status = 0;
                var rs = await _mFARepository.UpdateAsync(detail);
                if (rs == "Success")
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                    });
                else
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại",
                    });
            } catch(Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString(),
                });
            }

        }
    }
}