using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace AppLandingPage.Behaviors
{
    public interface ICampaignAdsRepositoryFactory
    {
        List<CampaignAds> GetAll();
        List<CampaignAds> GetListAllAsync();
        Task<CampaignAds> GetById(int Id);
    }
}
