using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    /*
    public class AutomaticPurchaseConstants
    {
    }*/
    public enum AutomaticPurchaseStatus
    {
        New =0,
        PurchaseSuccess=1,
        PurchaseFailure=2,
        PurchaseCancelled=3,
        ErrorOnExcution=4,
        AcceptPurchaseUsed=5
    }
    public enum OrderDeliveryStatus
    {
        OrderPlaced=0,
        CarrierPickedUpPakage=1,
        OnDelivering=2,
        Delivered=3,
        PakageLost=4,
        OutOfDelivery=5,
        CannotHaveArived=6,
        RefundPakage=7,
        CannotTracking=8
    }
    public enum MethodOutputStatusCode
    {
        Success = 0,
        Failed = 1,
        ErrorOnExcution = 2,
        ErrorOnData = 3
    }
}
