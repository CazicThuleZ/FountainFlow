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
            var stories = new List<Story>()
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

            context.Story.AddRange(stories);
        }

        if (!context.Archetypes.Any())
        {
            // Create archetypes first
            var saveTheCatId = Guid.NewGuid();
            var aristotleId = Guid.NewGuid();
            var storyCircleId = Guid.NewGuid();
            var heroJourneyId = Guid.NewGuid();

            var archetypes = new List<Archetype>()
            {
                new Archetype()
                {
                    Id = aristotleId,
                    Domain = "Classical Greek",
                    Architect = "Aristotle",
                    Description = "Basic 3 act story",
                    ExternalLink = "https://aristotle.com/",
                    Icon = "Aristotle_Story.png",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new Archetype()
                {
                    Id = saveTheCatId,
                    Domain = "Save the Cat",
                    Architect = "Blake Snyder",
                    Description = "A story structure model for screenwriting",
                    ExternalLink = "https://savethecat.com/",
                    Icon = "saveTheCatLogo.png",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new Archetype()
                {
                    Id = storyCircleId,
                    Domain = "Story Circle",
                    Architect = "Dan Harmon",
                    Description = "Dan Harmon's screenwriting model",
                    ExternalLink = "https://www.harmontown.com/",
                    Icon = "Story_Circle_Icon.png",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new Archetype()
                {
                    Id = heroJourneyId,
                    Domain = "Hero's Journey",
                    Architect = "Joseph Campbell",
                    Description = "Pursuit of the hero archetype",
                    ExternalLink = "https://www.jcf.org/",
                    Icon = "Joseph Cambell.png",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                }
            };

            context.Archetypes.AddRange(archetypes);
            context.SaveChanges();


            var archetypeBeats = new List<ArchetypeBeat>()
            {
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = saveTheCatId,
                    Name = "Opening Image",
                    Description = "A snapshot of the protagonist's life before the journey begins",
                    PercentOfStory = 1,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = saveTheCatId,
                    Name = "Setup",
                    Description = "Establish the hero's world and what's missing in their life",
                    PercentOfStory = 10,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                // Story Circle beats
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = storyCircleId,
                    Name = "Comfort Zone",
                    Description = "Character is in their familiar situation",
                    PercentOfStory = 12,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = storyCircleId,
                    Name = "Need",
                    Description = "Character wants something",
                    PercentOfStory = 12,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },

                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = heroJourneyId,
                    Name = "Ordinary World",
                    Description = "Hero's normal life before the adventure",
                    PercentOfStory = 8,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = heroJourneyId,
                    Name = "Call to Adventure",
                    Description = "Hero faces a challenge or opportunity",
                    PercentOfStory = 8,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = aristotleId,
                    Name = "Beginning",
                    Description = "Setup the story and introduce the characters",
                    PercentOfStory = 33,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = aristotleId,
                    Name = "Middle",
                    Description = "Conflict and obstacles",
                    PercentOfStory = 33,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeBeat()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = aristotleId,
                    Name = "End",
                    Description = "Resolution and conclusion",
                    PercentOfStory = 33,
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                }
            };

            context.ArchetypeBeats.AddRange(archetypeBeats);

            // Add ArchetypeGenres
            var archetypeGenres = new List<ArchetypeGenre>()
            {
                // Save the Cat genres
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = saveTheCatId,
                    Name = "Monster in the House",
                    Description = "A story about survival against a deadly force in a confined setting",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = saveTheCatId,
                    Name = "Golden Fleece",
                    Description = "A quest that changes the hero",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                // Story Circle genres
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = storyCircleId,
                    Name = "Television Episode",
                    Description = "30-minute story structure for TV",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                // Hero's Journey genres
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = heroJourneyId,
                    Name = "Epic Fantasy",
                    Description = "Mythological journey in a fantastic setting",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                // Aristotle genres
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = aristotleId,
                    Name = "Tragedy",
                    Description = "A story with a sad ending",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                },
                new ArchetypeGenre()
                {
                    Id = Guid.NewGuid(),
                    ArchetypeId = aristotleId,
                    Name = "Comedy",
                    Description = "A story with a happy ending",
                    CreatedUTC = DateTime.UtcNow,
                    UpdatedUTC = DateTime.UtcNow
                }
            };

            context.ArchetypeGenres.AddRange(archetypeGenres);
        }

        context.SaveChanges();
    }
}