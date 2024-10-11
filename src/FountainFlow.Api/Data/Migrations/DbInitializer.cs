using System;
using System.Collections.Generic;
using System.Linq;
using FountainFlow.Api.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FountainFlow.Api.Data;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        SeedData(scope.ServiceProvider.GetService<FFDbContext>());

    }

    private static void SeedData(FFDbContext context)
    {
        context.Database.Migrate();

        if (!context.Story.Any())
        {
            var dsOptions = new List<Story>()
            {
                new Story()
                {
                    Id = Guid.NewGuid(),
                    Title = "Arrival",
                    Author = "Eric Heisserer",
                    DevelopmentStage = DevelopmentStage.Model,
                    PublishedUTC = DateTime.UtcNow,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new Story()
                {
                    Id = Guid.NewGuid(),
                    Title = "Sing Street",
                    Author = "John Carney",
                    DevelopmentStage = DevelopmentStage.Model,
                    PublishedUTC = DateTime.UtcNow,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                }
            };
        }
    }
}
