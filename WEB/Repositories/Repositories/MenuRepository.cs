using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly MenuDAL _MenuDAL;
        public MenuRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _MenuDAL = new MenuDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<MenuViewModel>> GetMenuParentAndChildAll()
        {
            var rslist = new List<MenuViewModel>();
            try
            {
                var ListMenu = await _MenuDAL.GetAllAsync();
                ListMenu = ListMenu.Where(n => n.Status == (int)Utilities.Contants.Status.HOAT_DONG).ToList();
                var ParentList = ListMenu.Where(s => s.ParentId <= 0);
                foreach (var item in ParentList)
                {
                    var modelrs = new MenuViewModel();

                    var ChildList = new List<Menu>();
                    GetListMenuChild(item.Id, ListMenu, ref ChildList);

                    modelrs.Parent = item;
                    modelrs.ChildList = ChildList;
                    rslist.Add(modelrs);
                }
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetMenuParentAndChildAll - MenuRepository: " + ex);
            }
            return rslist;
        }

        public void GetListMenuChild(int ParentId, List<Menu> listdata, ref List<Menu> ListRs)
        {
            var childlist = listdata.Where(s => s.ParentId == ParentId).ToList();
            if (childlist != null && childlist.Count > 0)
            {
                ListRs.AddRange(childlist);
                foreach (var item in childlist)
                {
                    GetListMenuChild(item.Id, listdata, ref ListRs);
                }
            }
        }

        public async Task<List<Permission>> GetPermissionList()
        {
            var rslist = new List<Permission>();
            try
            {
                rslist = await _MenuDAL.GetPermissionList();
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPermissionList - MenuRepository: " + ex);
            }
            return rslist;
        }
    }
}
