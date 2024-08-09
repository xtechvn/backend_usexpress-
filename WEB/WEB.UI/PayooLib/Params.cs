namespace Payoo.Lib
{
    public class APIRequest
    {
        public string RequestData;
        public string Signature;
    }
    public class APIResponse
    {
        public string ResponseData;
        public string Signature;
    }
    public class GetOrderInfoRequest
    {
        public string OrderId;
        public string ShopId;
    }
    public class CreateBillingCodeRequest
    {
        public string OrderNo;
        public string ShopID;
        public string FromShipDate;
        public string ShipNumDay;
        public string Description;
        public string CyberCash;
        public string PaymentExpireDate;
        public string NotifyUrl;
        public string InfoEx;
        public string BillingCode;
    }
    public class CreateBillingCodeResponse
    {
        public string BillingCode;
        public string ResponseCode;
    }
    public class CancelOrder
    {
        public string ShopID;
        public string OrderID;
        public string NewStatus;
        public string UpdateLog;
    }

    public class PayooConnectionPackage
    {
        private string _Data = "";
        private string _Signature = "";
        private string _PayooSessionID = "";
        private string _KeyFields = "";

        public string Data
        {
            set { this._Data = value; }
            get { return this._Data; }
        }
        public string Signature
        {
            set { this._Signature = value; }
            get { return this._Signature; }
        }
        public string PayooSessionID
        {
            set { this._PayooSessionID = value; }
            get { return this._PayooSessionID; }
        }
        public string KeyFields
        {
            set { this._KeyFields = value; }
            get { return this._KeyFields; }
        }

    }

    public class PaymentNotification : PayooOrder
    {
        private string _PaymentMethod = "";

        public string PaymentMethod
        {
            get { return _PaymentMethod; }
            set { _PaymentMethod = value; }
        }


        private string _State = "";

        public string State
        {
            set { this._State = value; }
            get { return this._State; }
        }
    }
    public class PayooOrder
    {
        private string _Session;
        private string _BusinessUsername;
        private long _ShopID;
        private string _ShopTitle;
        private string _ShopDomain;
        private string _ShopBackUrl;
        private string _OrderNo;
        private long _OrderCashAmount;
        private string _StartShippingDate; //Format: dd/mm/yyyy
        private short _ShippingDays;
        private string _OrderDescription;
        private string _NotifyUrl = "";
        private string _validityTime; //yyyyMMddhhmmss
        private string _customerName;
        private string _customerPhone;
        private string _customerAddress;
        //private string _customerCity;
        private string _customerEmail;
        private string _BillingCode;
        private string _PaymentExpireDate;

        public string Session
        {
            set { this._Session = value; }
            get { return this._Session; }
        }
        public string BusinessUsername
        {
            set { this._BusinessUsername = value; }
            get { return this._BusinessUsername; }
        }
        public long ShopID
        {
            set { this._ShopID = value; }
            get { return this._ShopID; }
        }
        public string ShopTitle
        {
            set { this._ShopTitle = value; }
            get { return this._ShopTitle; }
        }
        public string ShopDomain
        {
            set { this._ShopDomain = value; }
            get { return this._ShopDomain; }
        }
        public string ShopBackUrl
        {
            set { this._ShopBackUrl = value; }
            get { return this._ShopBackUrl; }
        }
        public string OrderNo
        {
            set { this._OrderNo = value; }
            get { return this._OrderNo; }
        }
        public long OrderCashAmount
        {
            set { this._OrderCashAmount = value; }
            get { return this._OrderCashAmount; }
        }
        public string StartShippingDate
        {
            set { this._StartShippingDate = value; }
            get { return this._StartShippingDate; }
        }
        public short ShippingDays
        {
            set { this._ShippingDays = value; }
            get { return this._ShippingDays; }
        }
        public string OrderDescription
        {
            set { this._OrderDescription = value; }
            get { return this._OrderDescription; }
        }
        public string NotifyUrl
        {
            set { this._NotifyUrl = value; }
            get { return this._NotifyUrl; }
        }
        public string ValidityTime
        {
            get { return _validityTime; }
            set { _validityTime = value; }
        }
        public string CustomerName
        {
            get { return _customerName; }
            set { _customerName = value; }
        }

        public string CustomerPhone
        {
            get { return _customerPhone; }
            set { _customerPhone = value; }
        }

        public string CustomerAddress
        {
            get { return _customerAddress; }
            set { _customerAddress = value; }
        }
        public string CustomerEmail
        {
            get { return _customerEmail; }
            set { _customerEmail = value; }
        }

        /*public string CustomerCity
        {
            get { return _customerCity; }
            set { _customerCity = value; }
        }
        */
        public string BillingCode
        {
            get { return _BillingCode; }
            set { _BillingCode = value; }
        }

        public string PaymentExpireDate
        {
            get { return _PaymentExpireDate; }
            set { _PaymentExpireDate = value; }
        }
    }

    public class CreateOrderResponse
    {
        public string result;
        public Order order;
    }
    public class Order
    {
        public string payment_url;
    }
}