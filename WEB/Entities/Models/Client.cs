using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Client
    {
        public Client()
        {
            AddressClient = new HashSet<AddressClient>();
            Order = new HashSet<Order>();
        }

        public long Id { get; set; }
        public long? ClientMapId { get; set; }
        public int? SourceRegisterId { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordBackup { get; set; }
        public int? Gender { get; set; }
        public DateTime? TokenCreatedDate { get; set; }
        public string ActiveToken { get; set; }
        public string ForgotPasswordToken { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public string Avartar { get; set; }
        public DateTime JoinDate { get; set; }
        public bool? IsReceiverInfoEmail { get; set; }
        public int? TotalOrder { get; set; }
        public string SourceLoginId { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool? IsRegisterAffiliate { get; set; }
        public string ReferralId { get; set; }

        public virtual ICollection<AddressClient> AddressClient { get; set; }
        public virtual ICollection<Order> Order { get; set; }
    }
}
