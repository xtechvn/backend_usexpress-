using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppLandingPage.Behaviors
{
    public interface IProductClassificationFactody
    {
        Task<List<ProductClassification>> GetByProductGroupId(int id);
        Task<ProductClassification> GetByLink(string link);
        List<ProductClassification> GetAll();
        Task<ProductClassification> GetById(int Id);
    }
}
