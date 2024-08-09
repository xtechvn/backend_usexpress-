using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ImageProductViewModel : ImageProduct
    {
        public long ProductMapId { get; set; } // productId map voi Id product o DB cu         
    }
 
}
