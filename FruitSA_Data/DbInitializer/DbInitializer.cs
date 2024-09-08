using FruitSA_Common;
using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FruitSA_Assessment.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _datadase;


        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext database)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _datadase = database;
        }

        public async void Initialize()
        {
            // Migrations if they are not applied
            try
            {
                if (_datadase.Database.GetPendingMigrations().Any())
                {
                    _datadase.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
            }

            if (!_roleManager.RoleExistsAsync(CreateRoles.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(CreateRoles.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(CreateRoles.Role_Customer)).GetAwaiter().GetResult();

                if (_datadase.Users.FirstOrDefault(u => u.Email == "admin@gmail") == null)
                {
                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "Admin",
                        Email = "admin@gmail.com",
                        Name = "Admin",
                        LastName = "admin User"
                    }, "Password123#").GetAwaiter().GetResult();

                    ApplicationUser user = (ApplicationUser)_datadase.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");

                    _userManager.AddToRoleAsync(user, CreateRoles.Role_Admin).GetAwaiter().GetResult();
                }
            }

            if (!_datadase.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Fruit", CategoryCode = "FRU123", IsActive = true, Username = "admin@gmail.com", CreatedAt = DateTime.Now, UpdateAt = DateTime.Now },
                    new Category { Name = "Vegetable", CategoryCode = "VEG234", IsActive = true, Username = "tester@gmail.com", CreatedAt = DateTime.Now.AddDays(-12), UpdateAt = null },
                    new Category { Name = "Meat", CategoryCode = "MEA456", IsActive = true, Username = "systemAdmin@gmail.com", CreatedAt = DateTime.Now, UpdateAt = DateTime.Now },
                   
                };

                _datadase.Categories.AddRange(categories);
                _datadase.SaveChanges();
            }

            if (!_datadase.Products.Any())
            {
                
                var product = new List<Product>
                {
                    new Product { ProductName = "Apple", ProductCode = "2024-091", CategoryId = 1,Description = "Apple", ImagePath = "", Price = 2030, Username = "admin@gmail.com", CreatedAt = DateTime.Now, UpdateAt = DateTime.Now },
                    new Product { ProductName = "Cabbage", ProductCode = "2024-092", CategoryId = 2,Description = "Orange", ImagePath = "", Price = 200, Username = "tester@gmail.com", CreatedAt = DateTime.Now.AddDays(-12), UpdateAt = null },
                    new Product { ProductName = "Sausage", ProductCode = "2024-093", CategoryId = 3,Description = "Banana", ImagePath = "", Price = 400, Username = "systemAdmin@gmail.com", CreatedAt = DateTime.Now, UpdateAt = DateTime.Now },

                };

                _datadase.Products.AddRange(product);
                _datadase.SaveChanges();
            }
        }
    }
}
