using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            dbContext.Users.Add(new User
            {
                Email = "admin@cleanarch.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            });
        }

        if (!await dbContext.Products.AnyAsync(cancellationToken))
        {
            dbContext.Products.AddRange(
                new Product
                {
                    Name = "Mechanical Keyboard",
                    Description = "Hot-swappable mechanical keyboard with RGB backlight.",
                    Price = 129.99m,
                    StockQuantity = 20
                },
                new Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with programmable buttons.",
                    Price = 59.99m,
                    StockQuantity = 40
                });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
