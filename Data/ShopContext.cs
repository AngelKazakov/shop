using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data.Models;

namespace RandomShop.Data
{
    public class ShopContext : IdentityDbContext
    {
        public ShopContext(DbContextOptions<ShopContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; init; }

        public virtual DbSet<Country> Countries { get; init; }

        public virtual DbSet<OrderLine> OrderLines { get; init; }

        public virtual DbSet<OrderStatus> OrderStatuses { get; init; }

        public virtual DbSet<PaymentType> PaymentTypes { get; init; }

        public virtual DbSet<Product> Products { get; init; }

        public virtual DbSet<ProductCategory> ProductCategories { get; init; }

        public virtual DbSet<ProductConfiguration> ProductConfigurations { get; init; }

        public virtual DbSet<ProductImage> ProductImages { get; init; }

        public virtual DbSet<ProductItem> ProductItems { get; init; }

        public virtual DbSet<Promotion> Promotions { get; init; }

        public virtual DbSet<PromotionCategory> PromotionCategories { get; init; }

        public virtual DbSet<ShippingMethod> ShippingMethods { get; init; }

        public virtual DbSet<ShopOrder> ShopOrders { get; init; }

        public virtual DbSet<ShoppingCart> ShoppingCarts { get; init; }

        public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; init; }

        public virtual DbSet<User> Users { get; init; }

        public virtual DbSet<UserAddress> UserAddresses { get; init; }

        public virtual DbSet<UserPaymentMethod> UserPaymentMethods { get; init; }

        public virtual DbSet<UserReview> UserReviews { get; init; }

        public virtual DbSet<Variation> Variations { get; init; }

        public virtual DbSet<VariationOption> VariationOptions { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProductItem>()
                .Property(p => p.Price)
                .HasColumnType("decimal");

            builder.Entity<ShippingMethod>()
                .Property(x => x.Price)
                .HasColumnType("decimal");

            builder.Entity<ShopOrder>()
                .Property(x => x.OrderTotal)
                .HasColumnType("decimal");

            builder.Entity<UserAddress>()
                .HasKey(x => new { x.UserId, x.AddressId });

            builder.Entity<ProductConfiguration>()
                .HasKey(x => new { x.ProductItemId, x.VariationOptionId });

            builder.Entity<PromotionCategory>()
                .HasKey(x => new { x.PromotionId, x.ProductCategoryId });

            builder.Entity<ProductCategory>()
                .HasKey(x => new { x.Id, x.ParentCategoryId });


            //builder.Entity<PromotionCategory>().
            //    HasOptional(e => e.ParentCategory).
            //    WithMany().
            //    HasForeignKey(m => m.ParentCategoryID);
        }
    }
}