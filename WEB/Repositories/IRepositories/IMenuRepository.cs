using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IMenuRepository
    {
        Task<List<MenuViewModel>> GetMenuParentAndChildAll();
        Task<List<Permission>> GetPermissionList();
    }
}
