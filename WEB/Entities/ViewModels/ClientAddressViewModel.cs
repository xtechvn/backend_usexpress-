using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ClientAddressViewModel
    {
    }

    public class AddressModel : AddressClient
    {
        public string FullAddress { get; set; }
        public long OrderId { get; set; }
        public string Email { get; set; }
    }
}
