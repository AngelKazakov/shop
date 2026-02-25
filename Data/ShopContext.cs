using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data.Models;

namespace RandomShop.Data
{
    public class ShopContext : IdentityDbContext<User>
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

        public virtual DbSet<Category> Categories { get; init; }

        public virtual DbSet<ProductConfiguration> ProductConfigurations { get; init; }

        public virtual DbSet<ProductImage> ProductImages { get; init; }

        public virtual DbSet<ProductItem> ProductItems { get; init; }

        public virtual DbSet<Promotion> Promotions { get; init; }

        public virtual DbSet<ProductPromotion> ProductPromotions { get; init; }

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

        public virtual DbSet<ProductCategory> ProductCategories { get; init; }

        public virtual DbSet<UserFavoriteProduct> UserFavoriteProducts { get; init; }

        public virtual DbSet<UserReviewLike> ReviewLikes { get; init; }

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

            builder.Entity<OrderLine>()
                .Property(x => x.Price)
                .HasColumnType("decimal");

            builder.Entity<UserAddress>()
                .HasKey(x => new { x.UserId, x.AddressId });

            builder.Entity<ProductConfiguration>()
                .HasKey(x => new { x.ProductItemId, x.VariationOptionId });

            builder.Entity<ProductPromotion>()
                .HasKey(x => new { x.ProductId, x.PromotionId });

            builder.Entity<ShopOrder>()
                .HasMany(x => x.UserPaymentMethods)
                .WithOne(x => x.ShopOrder)
                .HasForeignKey(x => x.ShopOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<UserReview>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserReviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Category>()
                .HasMany(x => x.ProductCategories)
                .WithOne(s => s.Category)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Category>()
                .HasMany(x => x.Variations)
                .WithOne(s => s.Category)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Category>()
                .HasOne(x => x.ParentCategory)
                .WithMany(x => x.Subcategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<User>()
                .HasOne(u => u.ShoppingCart)
                .WithOne(sc => sc.User)
                .HasForeignKey<ShoppingCart>(sc => sc.UserId);

            builder.Entity<ProductCategory>().HasKey(x => new { x.ProductId, x.CategoryId });

            builder.Entity<UserFavoriteProduct>()
                .HasOne(f => f.Product)
                .WithMany(p => p.UserFavoriteProducts)
                .HasForeignKey(x => x.ProductId);

            builder.Entity<UserFavoriteProduct>()
                .HasOne(f => f.User).WithMany(u => u.UserFavoriteProducts)
                .HasForeignKey(f => f.UserId);

            builder.Entity<UserReviewLike>()
                .HasOne(x => x.Review)
                .WithMany(x => x.UserReviewLikes)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShopOrder>()
                .HasIndex(x => x.OrderNumber);
            // .IsUnique();
        }
    }
}