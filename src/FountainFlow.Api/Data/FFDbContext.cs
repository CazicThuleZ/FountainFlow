using FountainFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FountainFlow.Api.Data;

public class FFDbContext : DbContext
{
    public FFDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Story> Story { get; set; } = null!;
    public DbSet<StoryLine> StoryLines { get; set; } = null!;
    public DbSet<LogLine> LogLines { get; set; } = null!;
    public DbSet<Theme> Themes { get; set; } = null!;
    public DbSet<ThemeExtension> ThemeExtensions { get; set; } = null!;
    public DbSet<Archetype> Archetypes { get; set; } = null!;
    public DbSet<ArchetypeBeat> ArchetypeBeats { get; set; } = null!;
    public DbSet<ArchetypeGenre> ArchetypeGenres { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Story entity configuration
        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(sb => sb.StoryLines)
                .WithOne(sl => sl.Story)
                .HasForeignKey(sl => sl.StoryId);

            entity.HasOne(s => s.LogLine)
                .WithOne()
                .HasForeignKey<Story>(s => s.LogLineId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => new { t.Title, t.DevelopmentStage })
                .IsUnique();

            entity.Property(e => e.PublishedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(t => t.Title)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(a => a.Author)
                .IsRequired()
                .HasColumnType("text");
        });

        // StoryLine entity configuration
        modelBuilder.Entity<StoryLine>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Sequence).IsRequired();

            entity.Property(e => e.LineType)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.LineText)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.HasOne(sl => sl.ArchetypeBeat)
                .WithMany()  // No navigation property in ArchetypeBeat class
                .HasForeignKey(sl => sl.ArchetypeBeatId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LogLine entity configuration
        modelBuilder.Entity<LogLine>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");

            // Configure optional relationship with Archetype
            entity.HasOne(ll => ll.Archetype)
                .WithMany()
                .HasForeignKey(ll => ll.ArchetypeId)
                .OnDelete(DeleteBehavior.SetNull);  // Ensure deleting LogLine doesn't affect Archetype

            entity.HasOne(ll => ll.Theme)
                .WithMany()
                .HasForeignKey(ll => ll.ThemeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Theme and ThemeExtension entity configuration
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(t => t.ThemeExtensions)
                .WithOne(te => te.Theme)
                .HasForeignKey(te => te.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Text)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.TagList)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<ThemeExtension>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Notion)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");
        });

        // Archetype entity configuration
        modelBuilder.Entity<Archetype>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(a => a.ArchetypeBeats)
                .WithOne(ab => ab.Archetype)
                .HasForeignKey(ab => ab.ArchetypeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.ArchetypeGenres)
                .WithOne(ag => ag.Archetype)
                .HasForeignKey(ag => ag.ArchetypeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Domain)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");
        });

        // ArchetypeBeat entity configuration
        modelBuilder.Entity<ArchetypeBeat>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ParentSequence)
                .IsRequired();

            entity.Property(e => e.ChildSequence)
                .IsRequired(false);

            entity.Property(e => e.GrandchildSequence)
                .IsRequired(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.Prompt)
                .IsRequired(false)
                .HasColumnType("text");

            entity.Property(e => e.PercentOfStory)
                .IsRequired();

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(e => new { e.ArchetypeId, e.ParentSequence, e.ChildSequence, e.GrandchildSequence });                
        });

        // ArchetypeGenre entity configuration
        modelBuilder.Entity<ArchetypeGenre>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CreatedUTC)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedUTC)
                .HasColumnType("timestamp with time zone");
        });
    }
}
