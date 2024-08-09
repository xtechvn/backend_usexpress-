using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class ImageProductRepository : IImageProductRepository
    {
        private readonly ILogger<LabelRepository> _logger;
        private readonly ImageProductDAL _ImageProductDAL;
        public ImageProductRepository(IOptions<DataBaseConfig> dataBaseConfig, ILogger<LabelRepository> logger)
        {
            _logger = logger;
            _ImageProductDAL = new ImageProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<ImageProduct> GetAll()
        {
            return _ImageProductDAL.GetAll();
        }

        public Task<ImageProduct> GetById(int Id)
        {
            return _ImageProductDAL.GetById(Id);
        }
        public async Task<long> Create(ImageProductViewModel model)
        {
            try
            {
                ImageProduct imageProduct = new ImageProduct();
                imageProduct.Image = model.Image;
                imageProduct.ProductId = model.ProductId;
                long rs = 0;
                rs = await _ImageProductDAL.CreateItem(imageProduct);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Create - ImageProductRepository: " + ex);
                return -1;
            }
        }
        public async Task<long> Update(ImageProductViewModel model)
        {
            try
            {
                ImageProduct imageProduct = new ImageProduct();
                imageProduct.Id = model.Id;
                imageProduct.Image = model.Image;
                imageProduct.ProductId = model.ProductId;
                long rs = 0;
                rs = await _ImageProductDAL.UpdateItem(imageProduct);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - ImageProductRepository: " + ex);
                return -1;
            }
           
        }

    }
}
