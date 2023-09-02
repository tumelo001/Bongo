using Bongo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bongo.Data
{
    public class SeedData
    {
        private class User
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public static void EnsurePopulated(IApplicationBuilder app)
        {
            AppDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

            

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (!context.Colors.Any())
            {
                context.Colors.AddRange(
                     new Color { ColorValue = "#FFC107", ColorName = "Amber" },
                     new Color { ColorValue = "#4CAF50", ColorName = "Green" },
                     new Color { ColorValue = "#2196F3", ColorName = "Blue" },
                     new Color { ColorValue = "#E91E63", ColorName = "Pink" },
                     new Color { ColorValue = "#FF5722", ColorName = "Deep Orange" },
                     new Color { ColorValue = "#9C27B0", ColorName = "Light Purple" },
                     new Color { ColorValue = "#FF9800", ColorName = "Orange" },
                     new Color { ColorValue = "#03A9F4", ColorName = "Light Blue" },
                     new Color { ColorValue = "#FF0000", ColorName = "Red" },
                     new Color { ColorValue = "#00FFFF", ColorName = "Cyan" },
                     new Color { ColorValue = "#FF1493", ColorName = "Deep Pink" },
                     new Color { ColorValue = "#008080", ColorName = "Teal" },
                     new Color { ColorValue = "#800080", ColorName = "Purple" },
                     new Color { ColorValue = "#FFFFFF", ColorName = "No-color" }
                 );

            }
            context.SaveChanges();
            if (!context.ModuleColors.Any())
            {
                context.ModuleColors.Add(new ModuleColor { ModuleCode = "Admin", ColorId = 3 });
            }
            context.SaveChanges();
        }
    }
}
