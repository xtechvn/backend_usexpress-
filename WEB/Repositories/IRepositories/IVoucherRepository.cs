using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
   public interface IVoucherRepository
    {
        Task<Voucher> getDetailVoucher(string voucher_name);
        Task<List<Voucher>> getListVoucherPublic(bool is_public);
    }
}
