using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Tag
    {
        public Tag()
        {
            ArticleTag = new HashSet<ArticleTag>();
        }

        public long Id { get; set; }
        public string TagName { get; set; }
        public DateTime? CreatedOn { get; set; }

        public virtual ICollection<ArticleTag> ArticleTag { get; set; }
    }
}
