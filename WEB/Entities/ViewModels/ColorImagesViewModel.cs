using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ColorImagesViewModel
    {
        public string product_code { get; set; }
        public string color_name  { get; set; }
        public List<ImageSizeViewModel> obj_list_img_size { get; set; }
    }
}
