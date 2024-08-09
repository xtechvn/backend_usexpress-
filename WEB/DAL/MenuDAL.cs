using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class MenuDAL : GenericService<Menu>
    {
        private PermissionDAL _PermissionDAL;
        public MenuDAL(string connection ) : base(connection)
        {
            _PermissionDAL = new PermissionDAL(connection);
        }

        public async Task<List<Permission>> GetPermissionList()
        {
            try
            {
                var rslist = await _PermissionDAL.GetAllAsync();
                return rslist.Where(s => s.Status == 0).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPermissionList : MenuDAL: " + ex);
                return new List<Permission>();
            }

        }
    }
}
