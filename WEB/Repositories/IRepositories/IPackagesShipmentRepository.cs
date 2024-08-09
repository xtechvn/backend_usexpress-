using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
   public interface IPackagesShipmentRepository
   {
        public Task<List<PackagesShipment>> GetTrackingList();
        public Task<long> UpdatePackagesShipmentStatus(long id, int status);

    }
}
