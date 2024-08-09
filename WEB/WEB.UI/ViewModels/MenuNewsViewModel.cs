using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class MenuNewsViewModel
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public bool has_child { get; set; }
        public List<GroupProductViewModel> menu_child { get; set; }
    }
}
