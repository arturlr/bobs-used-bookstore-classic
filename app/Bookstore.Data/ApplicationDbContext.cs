using System.ComponentModel.DataAnnotations.Schema;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Address> Address { get; set; }

        public DbSet<Book> Book { get; set; }

        public DbSet<Customer> Customer { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<Offer> Offer { get; set; }

        public DbSet<ReferenceDataItem> ReferenceData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .Property(x => x.Sub)
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450);
            
            modelBuilder.Entity<Customer>()
                .HasIndex(x => x.Sub)
                .IsUnique();

            // Configure Book relationships
            modelBuilder.Entity<Book>()
                .HasOne(x => x.Publisher)
                .WithMany()
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(x => x.BookType)
                .WithMany()
                .HasForeignKey(x => x.BookTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(x => x.Genre)
                .WithMany()
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(x => x.Condition)
                .WithMany()
                .HasForeignKey(x => x.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Offer relationships
            modelBuilder.Entity<Offer>()
                .HasOne(x => x.Publisher)
                .WithMany()
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(x => x.BookType)
                .WithMany()
                .HasForeignKey(x => x.BookTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(x => x.Genre)
                .WithMany()
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(x => x.Condition)
                .WithMany()
                .HasForeignKey(x => x.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order relationship
            modelBuilder.Entity<Order>()
                .HasOne(x => x.Customer)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ReferenceData table name
            modelBuilder.Entity<ReferenceDataItem>().ToTable("ReferenceData");

            // Configure ShoppingCartItem composite key
            modelBuilder.Entity<ShoppingCartItem>()
                .HasKey(x => new { x.Id, x.ShoppingCartId });
            
            modelBuilder.Entity<ShoppingCartItem>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }
    }
}