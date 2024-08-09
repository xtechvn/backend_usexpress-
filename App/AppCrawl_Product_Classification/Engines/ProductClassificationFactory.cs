using AppLandingPage.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppLandingPage.Engines
{
    public class ProductClassificationFactory : IProductClassificationFactody
    {
        private readonly IProductClassificationFactody _ProductClassification;
        public ProductClassificationFactory(IProductClassificationFactody productClassification)
        {
            _ProductClassification = productClassification;
        }

        public List<Entities.Models.ProductClassification> GetAll()
        {
            return _ProductClassification.GetAll();
        }

        public Task<Entities.Models.ProductClassification> GetById(int Id)
        {
            return _ProductClassification.GetById(Id);
        }

        public Task<Entities.Models.ProductClassification> GetByLink(string link)
        {
            return _ProductClassification.GetByLink(link);
        }

        public Task<List<Entities.Models.ProductClassification>> GetByProductGroupId(int id)
        {
            return _ProductClassification.GetByProductGroupId(id);
        }
    }
}
