
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
   public class VariationViewModel
    {
        public string asin { get; set; }

        public bool selected { get; set; }        

        public DataObjectViewModel dimensions { get; set; }
    }
}
