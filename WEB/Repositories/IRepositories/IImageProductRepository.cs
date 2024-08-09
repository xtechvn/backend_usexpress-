using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IImageProductRepository
    {
        List<ImageProduct> GetAll();
        Task<ImageProduct> GetById(int Id);
        Task<long> Create(ImageProductViewModel model);
        Task<long> Update(ImageProductViewModel model);
        Task<int> Delete(int id);
    }
}
