using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class GroupProductStoreRepository : IGroupProductStoreRepository
    {
        private readonly GroupProductStoreDAL _groupProductStoreDAL;
        public GroupProductStoreRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _groupProductStoreDAL = new GroupProductStoreDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public Task<List<GroupProductStore>> GetAll()
        {
            return _groupProductStoreDAL.GetAllAsync();
        }
    }
}
