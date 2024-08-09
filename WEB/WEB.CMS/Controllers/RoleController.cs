using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _RoleRepository;
        private readonly IMenuRepository _MenuRepository;
        public RoleController(IRoleRepository roleRepository, IMenuRepository menuRepository)
        {
            _RoleRepository = roleRepository;
            _MenuRepository = menuRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<string> GetRoleSuggestionList(string name)
        {
            try
            {
                var rolelist = await _RoleRepository.GetAll();

                if (!string.IsNullOrEmpty(name))
                {
                    rolelist = rolelist.Where(s => StringHelpers.ConvertStringToNoSymbol(s.Name.Trim().ToLower())
                                                   .Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower())))
                                                   .ToList();
                }
                var suggestionlist = rolelist.Take(5).Select(s => new
                {
                    id = s.Id,
                    name = s.Name
                }).ToList();

                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch
            {
                return null;
            }
        }

        [HttpPost]
        public IActionResult Search(string roleName, string strUserId, int currentPage = 1, int pageSize = 8)
        {
            var model = new GenericViewModel<RoleDataModel>();
            try
            {
                model = _RoleRepository.GetPagingList(roleName, strUserId, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }

        public async Task<IActionResult> GetDetail(int Id, int tabActive = 1)
        {
            var model = new Role();
            var userRoleModel = new RoleUserViewModel();
            try
            {
                model = await _RoleRepository.GetById(Id);
                if (tabActive == 2)
                {
                    userRoleModel.RoleId = Id;
                    userRoleModel.ListUser = await _RoleRepository.GetListUserOfRole(Id);
                }
            }
            catch
            {

            }
            ViewBag.TabActive = tabActive;
            ViewBag.ListUserInRole = userRoleModel;
            return View(model);
        }

        public async Task<IActionResult> RoleListUser(int Id)
        {
            var model = new RoleUserViewModel();
            try
            {
                model.RoleId = Id;
                model.ListUser = await _RoleRepository.GetListUserOfRole(Id);
            }
            catch
            {
            }
            return View(model);
        }

        public async Task<IActionResult> RolePermission(int Id)
        {
            var memuList = await _MenuRepository.GetMenuParentAndChildAll();
            var permissionList = await _MenuRepository.GetPermissionList();
            var rolePermission = await _RoleRepository.GetRolePermissionById(Id);
            var userList = await _RoleRepository.GetListUserOfRole(Id);
            var arrayUser = userList.Select(s => new { id = s.Id, name = s.UserName });

            ViewBag.MenuList = memuList;
            ViewBag.RoleId = Id;
            ViewBag.PermissionList = permissionList;
            ViewBag.UserList = JsonConvert.SerializeObject(arrayUser);

            return View(rolePermission);
        }

        public async Task<IActionResult> AddOrUpdate(int Id)
        {
            var model = new RoleViewModel();
            if (Id != 0)
            {
                var roleEntity = await _RoleRepository.GetById(Id);
                model = new RoleViewModel()
                {
                    Id = roleEntity.Id,
                    Name = roleEntity.Name,
                    Description = roleEntity.Description,
                    Status = roleEntity.Status
                };
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpSert(RoleViewModel model)
        {
            try
            {
                var rs = await _RoleRepository.Upsert(model);
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
                    message = "Cập nhật thất bại"
                });
            }
        }

        /// <summary>
        /// Update User Role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userId"></param>
        /// <param name="type">
        /// 1 : Add
        /// 0 : Remove
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int roleId, int userId, int type = 1)
        {
            try
            {
                var rs = await _RoleRepository.UpdateUserRole(roleId, userId, type);
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
            catch
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Cập nhật thất bại"
                });
            }
        }

        /// <summary>
        /// Update Role Permission
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// 1: Add
        /// 0: Remove
        /// <returns></returns>
        public async Task<IActionResult> UpdateRolePermission(string data, int type)
        {
            try
            {
                var rs = await _RoleRepository.AddOrDeleteRolePermission(data, type);
                if (rs)
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
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> Delete(int roleId)
        {
            try
            {
                var rs = await _RoleRepository.DeleteRole(roleId);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công."
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm quyền đang được sử dụng cho chức năng phân quyền người dùng. Bạn không thể xóa."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại."
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
    }
}