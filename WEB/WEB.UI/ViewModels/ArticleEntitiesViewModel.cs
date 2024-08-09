using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class ArticleEntitiesViewModel
    {
        public List<ArticleFeModel> news_list { get; set; }
        public int total_news { get; set; }
    }
}
