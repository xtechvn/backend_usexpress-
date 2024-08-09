using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class AddressClientDAL : GenericService<AddressClient>
    {
        public AddressClientDAL(string connection) : base(connection)
        {
            _connection = connection;
        }

        public async Task<AddressClient> GetByClientId(int clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByIdAddressClient - AddressClientDAL: " + ex);
                return null;
            }
        }

    }
}
