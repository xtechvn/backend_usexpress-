using Entities.Models;
using Entities.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<UserDetailViewModel> CheckExistAccount(AccountModel entity);
        Task<bool> ResetPassword(string input);
        GenericViewModel<UserGridModel> GetPagingList(string userName, string strRoleId, int status, int currentPage, int pageSize);
        Task<UserDetailViewModel> GetDetailUser(int Id);
        Task<int> Create(UserViewModel model);
        Task<int> Update(UserViewModel model);
        Task<int> UpdateUserRole(int userId, int[] arrayRole, int type);
        Task<int> ChangeUserStatus(int userId);
        Task<User> FindById(int id);
        Task<List<User>> GetUserSuggestionList(string userName);
        Task<string> ResetPasswordByUserId(int userId);
        Task<int> ChangePassword(UserPasswordModel model);
    }
}
