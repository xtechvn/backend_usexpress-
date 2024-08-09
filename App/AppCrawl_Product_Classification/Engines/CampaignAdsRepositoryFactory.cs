using AppLandingPage.Behaviors;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppLandingPage.Engines
{
    public class CampaignAdsRepositoryFactory : ICampaignAdsRepositoryFactory
    {
        private readonly ICampaignAdsRepositoryFactory _CampaignAdsRepository;
        public CampaignAdsRepositoryFactory(ICampaignAdsRepositoryFactory campaignAdsRepositoryFactory)
        {
            _CampaignAdsRepository = campaignAdsRepositoryFactory;
        }

        public List<CampaignAds> GetAll()
        {
            return _CampaignAdsRepository.GetAll();
        }

        public Task<CampaignAds> GetById(int Id)
        {
            return _CampaignAdsRepository.GetById(Id);
        }

        public List<CampaignAds> GetListAllAsync()
        {
            return _CampaignAdsRepository.GetListAllAsync();
        }
    }
}
