using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IClientRepository
    {
        GenericViewModel<ClientListingModel> GetPagingList(ClientSearchModel searchModel, int currentPage, int pageSize);
        string ReportClient(ClientSearchModel searchModel, string FilePath);
        Task<ClientDetailModel> GetDetail(long clientId);
        Task<List<AddressModel>> GetClientAddressList(long clientId);
        Task<string> ResetPassword(long clientId);
        Task<string> ResetPasswordDefault(long clientId);
        Task<int> ChangeStatus(long clientId);
        Task<long> ChangeDefaultAddress(long addressId);
        Task<long> UpdateAddressClient(AddressClientViewModel model);
        Task<long> addNewClient(ClientViewModel model);
        Task<bool> checkPhoneExist(string phone, long client_id);
        Task<bool> checkEmailExist(string email, long client_id);
        Task<long> addNewAddressClient(AddressClientViewModel model);
        Task<ClientViewModel> getClientDetail(long client_id);
        Task<ClientViewModel> getClientDetailByEmail(string email);
        Task<string> sendMailCode(string email, string code_email);
        Task<ClientViewModel> getClientByClientMapId(int client_id);
        int GetTotalClientInDay();
        Task<long> addNewAddressReceiverClient(AddressClientViewModel model);
        Task<AddressClient> getAddressClientById(long address_id);
        Task<long> deleteAddressClient(long client_id, long address_id);

        Task<long> UpdateClientMapId(long client_id, long client_map_id);
        Task<long> UpdateClient(ClientViewModel clientViewModel);
        Task<List<AddressReceiverOrderViewModel>> GetAddressReceiverByAddressId(long address_id);
        public Task<long> updateClientInfoAddress(ClientInfoViewModel clientViewModel);
        Task<long> updateClientChangePassword(ClientChangePasswordViewModel model);
        Task<long> registerAffiliate(long client_id, string ReferralId);
        Task<long> GetAddressIDByClientID(long client_id);
    }
}
