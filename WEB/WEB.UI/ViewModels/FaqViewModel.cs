using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class FaqViewModel
    {
        public List<CategoryViewModel> list_faq_menu { get; set; }//ds menu help

        public List<ArticleViewModel> article_list { get; set; } //ds bai viet theo cate_id
        public string path_help_active { get; set; }
        public string cate_name_active { get; set; }
    }
}
