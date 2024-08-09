using DAL.Generic;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL
{
    public class GroupProductStoreDAL : GenericService<GroupProductStore>
    {
        public GroupProductStoreDAL(string connection) : base(connection)
        {
        }

    }
}
