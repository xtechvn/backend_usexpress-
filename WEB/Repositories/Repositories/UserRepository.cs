using Caching.Elasticsearch;
using DAL;
using DAL.Generic;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserDAL _UserDAL;
        private readonly MailConfig _MailConfig;
        private readonly MFADAL _MFADAL;

        public UserRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<MailConfig> mailConfig, ILogger<UserRepository> logger)
        {
            _logger = logger;
            _MailConfig = mailConfig.Value;
            _UserDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _MFADAL = new MFADAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<UserDetailViewModel> CheckExistAccount(AccountModel entity)
        {
            try
            {
                var _encryptPassword = EncodeHelpers.MD5Hash(entity.Password);
                var _model = await _UserDAL.GetByUserName(entity.UserName);
                if (_model != null)
                {
                    if (_encryptPassword == _model.Password || _encryptPassword == _model.ResetPassword)
                    {
                        if (_model.Password != _model.ResetPassword)
                        {
                            if (_encryptPassword == _model.Password)
                            {
                                _model.ResetPassword = _encryptPassword;
                            }
                            else
                            {
                                _model.Password = _encryptPassword;
                            }

                            await _UserDAL.UpdateAsync(_model);
                        }

                        return await GetDetailUser(_model.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckExistAccount - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return null;
        }

        public async Task<bool> ResetPassword(string input)
        {
            try
            {
                User _model;
                if (StringHelpers.IsValidEmail(input))
                {
                    _model = await _UserDAL.GetByEmail(input);
                }
                else
                {
                    _model = await _UserDAL.GetByUserName(input);
                }

                if (_model != null)
                {
                    var _Password = StringHelpers.CreateRandomPassword();
                    _model.ResetPassword = EncodeHelpers.MD5Hash(_Password);
                    await _UserDAL.UpdateAsync(_model);

                    var _Subject = "Tin nhắn từ hệ thống";
                    var _Body = "Mật khẩu đăng nhập của bạn vừa được thay đổi: <b>" + _Password + "</b>";
                    await EmailHelper.SendMailAsync(_model.Email, _Subject, _Body, string.Empty, string.Empty);

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPassword - UserRepository: " + ex);
                _logger.LogError("ResetPassword: " + ex.Message);
            }
            return false;
        }

        public async Task<UserDetailViewModel> GetDetailUser(int Id)
        {
            var model = new UserDetailViewModel();
            try
            {
                model.Entity = await _UserDAL.FindAsync(Id);
                model.RoleIdList = await _UserDAL.GetUserRoleId(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailUser - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return model;
        }

        public GenericViewModel<UserGridModel> GetPagingList(string userName, string strRoleId, int status, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<UserGridModel>();
            try
            {
                model.ListData = _UserDAL.GetUserPagingList(userName, strRoleId, status, currentPage, pageSize, out int totalRecord);
                model.PageSize = pageSize;
                model.CurrentPage = currentPage;
                model.TotalRecord = totalRecord;
                model.TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - UserRepository: " + ex);
            }
            return model;
        }

        public async Task<int> Create(UserViewModel model)
        {
            try
            {
                var entity = new User()
                {
                    UserName = StringHelpers.ConvertStringToNoSymbol(model.UserName.ToLower()).Replace(" ", ""),
                    FullName = model.FullName,
                    Password = EncodeHelpers.MD5Hash(model.Password),
                    ResetPassword = EncodeHelpers.MD5Hash(model.Password),
                    Phone = model.Phone,
                    BirthDay = !string.IsNullOrEmpty(model.BirthDayPicker) ?
                                DateTime.ParseExact(model.BirthDayPicker, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                              : model.BirthDay,
                    Gender = model.Gender,
                    Email = model.Email,
                    Avata = model.Avata,
                    Address = model.Address,
                    Status = model.Status,
                    Note = model.Note,
                    CreatedBy = 1,
                    CreatedOn = DateTime.Now
                };

                // Check exist User Name or Email
                var userList = await _UserDAL.GetAllAsync();
                var exmodel = userList.Where(s => s.Status == 0 && (s.UserName == model.UserName || s.Email == model.Email));
                if (exmodel != null && exmodel.Count() > 0)
                {
                    return -1;
                }

                var userId = (int)await _UserDAL.CreateAsync(entity);

                if (model.RoleId != -1)
                {
                    await _UserDAL.UpdateUserRole(userId, new int[] { model.RoleId }, 0);
                }
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                LogHelper.InsertLogTelegram("Create - UserRepository: " + ex);
            }

            return 0;
        }

        public async Task<int> UpdateUserRole(int userId, int[] arrayRole, int type)
        {
            try
            {
                await _UserDAL.UpdateUserRole(userId, arrayRole, type);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateUserRole - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        public async Task<int> ChangeUserStatus(int userId)
        {
            try
            {
                var model = await _UserDAL.FindAsync(userId);
                if (model.Status == 0)
                {
                    model.Status = 1;
                }
                else
                {
                    model.Status = 0;
                }
                await _UserDAL.UpdateAsync(model);
                return model.Status;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeUserStatus - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return -1;
        }

        public async Task<User> FindById(int id)
        {
            var model = new User();
            try
            {
                model = await _UserDAL.FindAsync(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindById - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return model;
        }

        public async Task<int> Update(UserViewModel model)
        {
            try
            {
                var entity = await _UserDAL.FindAsync(model.Id);
                entity.FullName = model.FullName;
                entity.Phone = model.Phone;
                entity.BirthDay = !string.IsNullOrEmpty(model.BirthDayPicker) ?
                                  DateTime.ParseExact(model.BirthDayPicker, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                 : model.BirthDay;
                entity.Gender = model.Gender;
                entity.Email = model.Email;
                if (model.Avata != null)
                {
                    entity.Avata = model.Avata;
                }
                entity.Address = model.Address;
                entity.Status = model.Status;
                entity.Note = model.Note;
                entity.ModifiedBy = 1;
                entity.ModifiedOn = DateTime.Now;

                // Check exist User Name or Email
                var userList = await _UserDAL.GetAllAsync();
                var exmodel = userList.Where(s => s.Id != entity.Id && s.Status == 0 && (s.UserName == entity.UserName || s.Email == entity.Email)).ToList();
                if (exmodel != null && exmodel.Count > 0)
                {
                    return -1;
                }
                await _UserDAL.UpdateAsync(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        public async Task<List<User>> GetUserSuggestionList(string name)
        {
            List<User> data = new List<User>();
            try
            {
                data = await _UserDAL.GetAllAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    data = data.Where(s => s.UserName.Trim().ToLower().Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower()))).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggestionList - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return data;
        }

        public async Task<string> ResetPasswordByUserId(int userId)
        {
            var rs = string.Empty;
            try
            {
                var _model = await _UserDAL.FindAsync(userId);
                var _newPassword = StringHelpers.CreateRandomPassword();
                _model.ResetPassword = EncodeHelpers.MD5Hash(_newPassword);
                _model.Password = EncodeHelpers.MD5Hash(_newPassword);
                await _UserDAL.UpdateAsync(_model);
                rs = _newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPasswordByUserId - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return rs;
        }

        public async Task<int> ChangePassword(UserPasswordModel model)
        {
            var rs = 0;
            try
            {
                var _model = await _UserDAL.FindAsync(model.Id);
                if (_model.Password == EncodeHelpers.MD5Hash(model.Password))
                {
                    _model.ResetPassword = EncodeHelpers.MD5Hash(model.NewPassword);
                    _model.Password = EncodeHelpers.MD5Hash(model.NewPassword);
                    await _UserDAL.UpdateAsync(_model);
                    rs = model.Id;
                }
                else
                {
                    rs = -1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangePassword - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return rs;
        }

    }
}
