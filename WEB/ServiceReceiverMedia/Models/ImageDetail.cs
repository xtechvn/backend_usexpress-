using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceReceiverMedia.Models
{
    //Class thông tin ảnh gửi lên.
    [System.Serializable]
    public class ImageDetail
    {
        public string data_file;
        public string extend;
    }
    //Class thông tin ảnh gửi lên.
    [System.Serializable]
    public class PaymentImageDetail
    {
        public string data_file;
        public string extend;

    }
}
