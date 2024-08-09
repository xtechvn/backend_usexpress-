using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class GroupProductViewModel
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string image_thumb { get; set; }
        public string desc { get; set; }
    }
}
