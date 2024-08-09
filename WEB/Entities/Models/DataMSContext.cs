using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Entities.Models
{
    public partial class DataMSContext : DbContext
    {
        public DataMSContext()
        {
        }

        public DataMSContext(DbContextOptions<DataMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Action> Action { get; set; }
        public virtual DbSet<AddressClient> AddressClient { get; set; }
        public virtual DbSet<AffiliateGroupProduct> AffiliateGroupProduct { get; set; }
        public virtual DbSet<AllCode> AllCode { get; set; }
        public virtual DbSet<Article> Article { get; set; }
        public virtual DbSet<ArticleCategory> ArticleCategory { get; set; }
        public virtual DbSet<ArticleRelated> ArticleRelated { get; set; }
        public virtual DbSet<ArticleTag> ArticleTag { get; set; }
        public virtual DbSet<AttachFile> AttachFile { get; set; }
        public virtual DbSet<AutomaticPurchaseAmz> AutomaticPurchaseAmz { get; set; }
        public virtual DbSet<AutomaticPurchaseHistory> AutomaticPurchaseHistory { get; set; }
        public virtual DbSet<CampaignAds> CampaignAds { get; set; }
        public virtual DbSet<CampaignGroupProduct> CampaignGroupProduct { get; set; }
        public virtual DbSet<Cashback> Cashback { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<ClientLinkAff> ClientLinkAff { get; set; }
        public virtual DbSet<District> District { get; set; }
        public virtual DbSet<GroupProduct> GroupProduct { get; set; }
        public virtual DbSet<GroupProductStore> GroupProductStore { get; set; }
        public virtual DbSet<ImageProduct> ImageProduct { get; set; }
        public virtual DbSet<IndustrySpecialLuxury> IndustrySpecialLuxury { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<Label> Label { get; set; }
        public virtual DbSet<LocationProduct> LocationProduct { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Mfauser> Mfauser { get; set; }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<OrderProgress> OrderProgress { get; set; }
        public virtual DbSet<PackagesShipment> PackagesShipment { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Position> Position { get; set; }
        public virtual DbSet<PriceProductLevel> PriceProductLevel { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductClassification> ProductClassification { get; set; }
        public virtual DbSet<ProductHistory> ProductHistory { get; set; }
        public virtual DbSet<ProductOtherLink> ProductOtherLink { get; set; }
        public virtual DbSet<PropertiesProduct> PropertiesProduct { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RolePermission> RolePermission { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<Voucher> Voucher { get; set; }
        public virtual DbSet<VoucherCampaign> VoucherCampaign { get; set; }
        public virtual DbSet<VoucherLogActivity> VoucherLogActivity { get; set; }
        public virtual DbSet<Ward> Ward { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Action>(entity =>
            {
                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ControllerName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.Action)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_Action_Menu");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.Action)
                    .HasForeignKey(d => d.PermissionId)
                    .HasConstraintName("FK_Action_Permission");
            });

            modelBuilder.Entity<AddressClient>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(500);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DistrictId).HasMaxLength(5);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("Đây là số điện thoại nhận hàng");

                entity.Property(e => e.ProvinceId).HasMaxLength(5);

                entity.Property(e => e.ReceiverName).HasMaxLength(255);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.WardId).HasMaxLength(5);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AddressClient)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AddressClient_Client");
            });

            modelBuilder.Entity<AllCode>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DownTime).HasColumnType("datetime");

                entity.Property(e => e.Image11).HasMaxLength(350);

                entity.Property(e => e.Image169)
                    .IsRequired()
                    .HasMaxLength(350);

                entity.Property(e => e.Image43).HasMaxLength(350);

                entity.Property(e => e.Lead)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.PublishDate).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.UpTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<ArticleRelated>(entity =>
            {
                entity.HasOne(d => d.Article)
                    .WithMany(p => p.ArticleRelated)
                    .HasForeignKey(d => d.ArticleId)
                    .HasConstraintName("FK_ArticleRelated_Article");
            });

            modelBuilder.Entity<ArticleTag>(entity =>
            {
                entity.HasOne(d => d.Article)
                    .WithMany(p => p.ArticleTag)
                    .HasForeignKey(d => d.ArticleId)
                    .HasConstraintName("FK_ArticleTags_Article");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.ArticleTag)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_ArticleTags_Tags");
            });

            modelBuilder.Entity<AttachFile>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Ext)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Path).HasMaxLength(400);
            });

            modelBuilder.Entity<AutomaticPurchaseAmz>(entity =>
            {
                entity.Property(e => e.AutoBuyMappingId).HasColumnName("AutoBuyMappingID");                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.OrderCode)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OrderDetailUrl).HasColumnName("OrderDetailURL");

                entity.Property(e => e.OrderEstimatedDeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.OrderMappingId).HasMaxLength(150);

                entity.Property(e => e.OrderedSuccessUrl).HasColumnName("OrderedSuccessURL");

                entity.Property(e => e.ProductCode).HasMaxLength(100);

                entity.Property(e => e.PurchaseUrl)
                    .IsRequired()
                    .HasColumnName("PurchaseURL");

                entity.Property(e => e.PurchasedOrderId)
                    .HasColumnName("PurchasedOrderID")
                    .HasMaxLength(150);

                entity.Property(e => e.PurchasedSellerName).HasMaxLength(250);

                entity.Property(e => e.PurchasedSellerStoreUrl)
                    .HasColumnName("PurchasedSellerStoreURL")
                    .HasMaxLength(250);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
            });

            modelBuilder.Entity<AutomaticPurchaseHistory>(entity =>
            {
                entity.Property(e => e.AutomaticPurchaseId).HasColumnName("AutomaticPurchaseID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<CampaignAds>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CampaignName).HasMaxLength(300);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(400);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Cashback>(entity =>
            {
                entity.Property(e => e.CashbackDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(500);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(e => e.ActiveToken).HasMaxLength(100);

                entity.Property(e => e.Avartar)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.ClientName).HasMaxLength(256);

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ForgotPasswordToken).HasMaxLength(100);

                entity.Property(e => e.IsReceiverInfoEmail).HasColumnName("isReceiverInfoEmail");

                entity.Property(e => e.JoinDate).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(400);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PasswordBackup)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferralId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceLoginId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.TokenCreatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<ClientLinkAff>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientId).ValueGeneratedOnAdd();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LinkAff)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.Property(e => e.DistrictId)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Location).HasMaxLength(30);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProvinceId)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<GroupProduct>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.ImagePath)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.Path)
                    .HasMaxLength(400)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<GroupProductStore>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.LinkStoreMenu)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<ImageProduct>(entity =>
            {
                entity.Property(e => e.Image)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ImageProduct)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_ImageProduct_Product");
            });

            modelBuilder.Entity<IndustrySpecialLuxury>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.GroupLabelId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasComment("Nhóm sản phẩm áp dụng so sánh với nhóm sản phẩm nổi trội");

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.Type).HasComment("1: sync client ; 2 : sync order");
            });

            modelBuilder.Entity<Label>(entity =>
            {
                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.DescExpire).HasMaxLength(300);

                entity.Property(e => e.Domain)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Icon).HasMaxLength(500);

                entity.Property(e => e.PrefixOrderCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.StoreName).HasMaxLength(50);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<LocationProduct>(entity =>
            {
                entity.HasKey(e => e.LocationProductId);

                entity.Property(e => e.CreateOn).HasColumnType("datetime");

                entity.Property(e => e.LocationProductId).ValueGeneratedOnAdd();

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Icon).HasMaxLength(50);

                entity.Property(e => e.Link).HasMaxLength(200);

                entity.Property(e => e.MenuCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Title).HasMaxLength(250);
            });

            modelBuilder.Entity<Mfauser>(entity =>
            {
                entity.ToTable("MFAUser");

                entity.Property(e => e.BackupCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SecretKey)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsFixedLength();

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.UserCreatedYear)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.Property(e => e.Comment).HasMaxLength(400);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderNo)
                    .HasName("index_order_no")
                    .IsUnique();

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.ClientName).HasMaxLength(100);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TrackingId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");

                entity.Property(e => e.UtmCampaign)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UtmFirstTime)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UtmMedium)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.UtmSource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_Order_Client");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasComment("Pound");

                entity.Property(e => e.CreateOn).HasColumnType("datetime");

                entity.Property(e => e.ImageThumb)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItem_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItem_Product");
            });

            modelBuilder.Entity<OrderProgress>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PackagesShipment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AirlinesId)
                    .IsRequired()
                    .HasColumnName("AirlinesID")
                    .HasMaxLength(250);

                entity.Property(e => e.ArrivalTime).HasColumnType("datetime");

                entity.Property(e => e.Awbcode)
                    .IsRequired()
                    .HasColumnName("AWBCode")
                    .HasMaxLength(250);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DepartureTime).HasColumnType("datetime");

                entity.Property(e => e.Destination).HasMaxLength(250);

                entity.Property(e => e.Origin).HasMaxLength(250);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.Property(e => e.PositionName)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<PriceProductLevel>(entity =>
            {
                entity.HasKey(e => e.PriceId);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.LabelId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(2000);

                entity.Property(e => e.ToDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.ProductCode)
                    .HasName("index_product_code");

                entity.Property(e => e.CreateOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasColumnType("ntext");

                entity.Property(e => e.Information).HasColumnType("ntext");

                entity.Property(e => e.LinkSource)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.Manufacturer).HasMaxLength(400);

                entity.Property(e => e.Path)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Rating)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SellerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SellerName)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.UnitWeight)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateLast).HasColumnType("datetime");

                entity.Property(e => e.Variations).HasMaxLength(400);
            });

            modelBuilder.Entity<ProductClassification>(entity =>
            {
                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.Link).IsRequired();

                entity.Property(e => e.LinkProductTarget)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(400);

                entity.Property(e => e.ProductName).HasMaxLength(400);

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.UtmPath)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.HasOne(d => d.Capaign)
                    .WithMany(p => p.ProductClassification)
                    .HasForeignKey(d => d.CapaignId)
                    .HasConstraintName("FK_ProductClassification_CampaignAds");

                entity.HasOne(d => d.GroupIdChoiceNavigation)
                    .WithMany(p => p.ProductClassification)
                    .HasForeignKey(d => d.GroupIdChoice)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductClassification_groupProduct");

                entity.HasOne(d => d.Label)
                    .WithMany(p => p.ProductClassification)
                    .HasForeignKey(d => d.LabelId)
                    .HasConstraintName("FK_ProductClassification_label");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ProductClassification)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_ProductClassification_User");
            });

            modelBuilder.Entity<ProductHistory>(entity =>
            {
                entity.Property(e => e.CreateOn).HasColumnType("datetime");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductHistory)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductHistory_Product");
            });

            modelBuilder.Entity<ProductOtherLink>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LinkOrder)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasMaxLength(4000);

                entity.Property(e => e.PriceCheckout).HasComment("Là giá checkout khi khách đã chuyển tiền và Sale bắt đầu tạo đơn");

                entity.Property(e => e.PriceDyamic).HasComment("Khi sửa giá do biến động giá khi đầu Mỹ tiến hành mua sau khi đã tạo đơn. Thí sẽ lưu giá sửa đó vào trường này");

                entity.Property(e => e.ProductOrderId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(255);
            });

            modelBuilder.Entity<PropertiesProduct>(entity =>
            {
                entity.Property(e => e.CreateOn).HasColumnType("datetime");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PropertiesProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PropertiesProduct_Product");
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProvinceId)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.RolePermission)
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePermission_Menu");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.RolePermission)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePermission_Permission");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RolePermission)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePermission_Role");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.TagName).HasMaxLength(100);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(500);

                entity.Property(e => e.Avata).HasMaxLength(500);

                entity.Property(e => e.BirthDay).HasColumnType("datetime");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.FullName).HasMaxLength(500);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Note).HasMaxLength(2500);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ResetPassword)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRole)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_User");
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.Property(e => e.CampaignId).HasColumnName("campaign_id");

                entity.Property(e => e.Cdate)
                    .HasColumnName("cdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.EDate)
                    .HasColumnName("eDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.GroupUserPriority)
                    .HasColumnName("group_user_priority")
                    .IsUnicode(false)
                    .HasComment("Trường này để lưu nhóm những user được áp dụng trên voucher này");

                entity.Property(e => e.IsFreeLuxury).HasColumnName("is_free_luxury");

                entity.Property(e => e.IsLimitVoucher).HasColumnName("is_limit_voucher");

                entity.Property(e => e.IsMaxFee).HasColumnName("is_max_fee");

                entity.Property(e => e.IsMaxPriceProduct).HasColumnName("is_max_price_product");

                entity.Property(e => e.IsPriceFirstPoundFee)
                    .HasColumnName("is_price_first_pound_fee")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IsPublic)
                    .HasColumnName("is_public")
                    .HasComment("Nêu set true thì hiểu voucher này được public cho các user thanh toán đơn hàng");

                entity.Property(e => e.LimitTotalDiscount).HasColumnName("limit_total_discount");

                entity.Property(e => e.LimitUse).HasColumnName("limitUse");

                entity.Property(e => e.MinTotalAmount).HasColumnName("min_total_amount");

                entity.Property(e => e.PriceSales)
                    .HasColumnName("price_sales")
                    .HasColumnType("money");

                entity.Property(e => e.RuleType)
                    .HasColumnName("rule_type")
                    .HasComment("Trường này dùng để phân biệt voucher triển khai này chạy theo rule nào. Ví dụ: rule giảm giá với 1 số tiền vnđ trên toàn bộ đơn hàng. Giảm giá 20% phí first pound đầu tiên của nhãn hàng amazon. 1: triển khai rule giảm giá cho toàn bộ đơn hàng. 2 là rule áp dụng cho 20% phí first pound đầu tiên.");

                entity.Property(e => e.StoreApply)
                    .HasColumnName("store_apply")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Udate)
                    .HasColumnName("udate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Unit)
                    .HasColumnName("unit")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VoucherCampaign>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CampaignVoucher)
                    .HasColumnName("campaign_voucher")
                    .HasMaxLength(400);

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<VoucherLogActivity>(entity =>
            {
                entity.ToTable("voucherLogActivity");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CartId).HasColumnName("cart_id");

                entity.Property(e => e.CreateDate)
                    .HasColumnName("create_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.PriceSaleVnd).HasColumnName("price_sale_vnd");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasComment("Trang thai giao dịch voucher. 1: khoa. 0: dang ap dung");

                entity.Property(e => e.StoreId).HasColumnName("store_id");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

                entity.HasOne(d => d.Voucher)
                    .WithMany(p => p.VoucherLogActivity)
                    .HasForeignKey(d => d.VoucherId)
                    .HasConstraintName("FK_voucherLogActivity_Voucher");
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.Property(e => e.DistrictId)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Location).HasMaxLength(30);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.WardId)
                    .IsRequired()
                    .HasMaxLength(5);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
