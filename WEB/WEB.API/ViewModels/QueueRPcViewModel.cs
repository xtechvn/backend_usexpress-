using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.ViewModels
{
    public sealed class QueueRPcViewModel
    {

        public long Id { get; set; }
        public string link_crawl { get; set; }
        public string Processing { get; set; }
        public QueueRPcViewModel(string _link_crawl, string _Processing)
        {
            Id = DateTime.Now.Ticks;
            Processing = _Processing;
            link_crawl = _link_crawl;
        }

    }
}
