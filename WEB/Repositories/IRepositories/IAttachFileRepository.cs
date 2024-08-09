using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAttachFileRepository
    {
        Task<List<AttachFile>> GetListByType(long DataId, int type);
        Task<long> Delete(long Id, int userLogin);
        Task<List<object>> CreateMultiple(List<AttachFile> models);
    }
}
