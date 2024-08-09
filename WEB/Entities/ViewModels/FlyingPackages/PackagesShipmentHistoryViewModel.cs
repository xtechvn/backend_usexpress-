using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.FlyingPackages
{
    public class PackagesShipmentHistoryViewModel
    {
        public long FlyingPackageID { get; set; }
        public int Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public int TotalPieces { get; set; }
        public double TotalWeight { get; set; }
        public string FlightNo { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Note { get; set; }

    }
}
