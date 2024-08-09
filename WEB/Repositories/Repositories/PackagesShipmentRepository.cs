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
    public class PackagesShipmentRepository : IPackagesShipmentRepository
    {
        private readonly PackagesShipmentDAL _packagesShipmentDAL;


        public PackagesShipmentRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _packagesShipmentDAL = new PackagesShipmentDAL(_StrConnection);

        }
        public Task<List<PackagesShipment>> GetTrackingList()
        {
            return _packagesShipmentDAL.GetTrackingList();
        }

        public Task<long> UpdatePackagesShipmentStatus(long id, int status)
        {
            return _packagesShipmentDAL.UpdatePackagesShipmentStatus(id,status);

        }
    }
}
