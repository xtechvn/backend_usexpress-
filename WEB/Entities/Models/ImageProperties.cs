using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ImageProperties
    {
        public int Id { get; set; }
        public int? DataId { get; set; }
        public string Path { get; set; }
        public DateTime? CreateOn { get; set; }
        public bool? IsShowBanner { get; set; }
        public bool? IsShowCover { get; set; }
        public int? PositionId { get; set; }
    }
}
