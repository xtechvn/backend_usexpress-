using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly VoucherDAL _VoucherDAL;

        public VoucherRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _VoucherDAL = new VoucherDAL(_StrConnection);
        }

        public async Task<Voucher> getDetailVoucher(string voucher_name)
        {
            try
            {
                return await _VoucherDAL.FindByVoucherCode(voucher_name);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[Repository] getDetailVoucher in VoucherRepository" + ex);
                return null;
            }
        }

        public async Task<List<Voucher>> getListVoucherPublic(bool is_public)
        {
            try
            {
                return await _VoucherDAL.getVoucherPublic(is_public);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[Repository] getVoucherPublic in VoucherRepository" + ex);
                return null;
            }
        }
    }
}
