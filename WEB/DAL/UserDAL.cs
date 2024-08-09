using DAL.Generic;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class UserDAL : GenericService<User>
    {
        public UserDAL(string connection) : base(connection)
        {
        }

        public List<UserGridModel> GetUserPagingList(string userName, string strRoleId, int status, int currentPage, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var ListUser = _DbContext.User.AsQueryable();

                    if (!string.IsNullOrEmpty(userName))
                    {
                        ListUser = ListUser.Where(s => s.UserName.Contains(userName) || s.FullName.Contains(userName) || s.Email.Contains(userName));
                    }

                    if (status != -1)
                    {
                        ListUser = ListUser.Where(s => s.Status == status);
                    }

                    if (!string.IsNullOrEmpty(strRoleId))
                    {
                        var ListUserId = GetListUserIdByRole(strRoleId);
                        ListUser = ListUser.Where(s => ListUserId.Contains(s.Id));
                    }

                    totalRecord = ListUser.Count();
                    ListUser = ListUser.OrderByDescending(s => s.CreatedOn).Skip((currentPage - 1) * pageSize).Take(pageSize);


                    var data = ListUser.Select(s => new UserGridModel
                    {
                        Id = s.Id,
                        Avata = s.Avata,
                        UserName = s.UserName,
                        FullName = s.FullName,
                        Phone = s.Phone,
                        Email = s.Email,
                        Note = s.Note,
                        BirthDay = s.BirthDay,
                        Address = s.Address,
                        Status = s.Status,
                        CreatedOn = s.CreatedOn,
                        RoleList = (from a in _DbContext.UserRole.Where(a => a.UserId == s.Id)
                                    join b in _DbContext.Role on a.RoleId equals b.Id into bs
                                    from b in bs.DefaultIfEmpty()
                                    select new Role
                                    {
                                        Id = b.Id,
                                        Name = b.Name,
                                        Status = b.Status
                                    }).ToList()
                    }).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserPagingList - UserDAL: " + ex);
            }
            return null;
        }

        /// <summary>
        /// UpdateUserRole
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="type">
        /// 0 : add roles
        /// 1 : remove roles</param>
        /// <returns></returns>
        public async Task UpdateUserRole(int userId, int[] arrayRole, int type = 0)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    foreach (var roleId in arrayRole)
                    {
                        if (type == 0)
                        {
                            var model = new UserRole
                            {
                                UserId = userId,
                                RoleId = roleId
                            };

                            await _DbContext.UserRole.AddAsync(model);
                            await _DbContext.SaveChangesAsync();
                        }
                        else
                        {
                            var model = await _DbContext.UserRole.Where(s => s.UserId == userId && s.RoleId == roleId).FirstOrDefaultAsync();
                            _DbContext.UserRole.Remove(model);
                            await _DbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateUserRole - UserDAL: " + ex);
            }

        }

        public async Task<List<int>> GetUserRoleId(int userId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.UserRole.Where(x => x.UserId == userId).Select(s => s.RoleId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserRoleId - UserDAL: " + ex);
                return new List<int>();
            }
        }

        public async Task<User> GetByUserName(string input)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.User.FirstOrDefaultAsync(s => s.UserName.Equals(input));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }

        public async Task<User> GetByEmail(string input)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.User.FirstOrDefaultAsync(s => s.Email.Equals(input));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }

        public List<int> GetListUserIdByRole(string strRoleId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var arrRoleId = strRoleId.Split(',').Select(s => int.Parse(s)).ToArray();
                    return _DbContext.UserRole.Where(s => arrRoleId.Contains(s.RoleId)).Select(s => s.UserId).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserIdByRole - UserDAL: " + ex);
                return new List<int>();
            }

        }
    }
}