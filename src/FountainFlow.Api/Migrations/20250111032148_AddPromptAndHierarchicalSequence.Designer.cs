﻿// <auto-generated />
using System;
using FountainFlow.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FountainFlow.Api.Migrations
{
    [DbContext(typeof(FFDbContext))]
    [Migration("20250111032148_AddPromptAndHierarchicalSequence")]
    partial class AddPromptAndHierarchicalSequence
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FountainFlow.Api.Entities.Archetype", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Architect")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExternalLink")
                        .HasColumnType("text");

                    b.Property<string>("Icon")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Archetypes");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ArchetypeBeat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ArchetypeId")
                        .HasColumnType("uuid");

                    b.Property<int?>("ChildSequence")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("GrandchildSequence")
                        .HasColumnType("integer");

                    b.Property<string>("Icon")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ParentSequence")
                        .HasColumnType("integer");

                    b.Property<int>("PercentOfStory")
                        .HasColumnType("integer");

                    b.Property<string>("Prompt")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ArchetypeId", "ParentSequence", "ChildSequence", "GrandchildSequence");

                    b.ToTable("ArchetypeBeats");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ArchetypeGenre", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ArchetypeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Icon")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ArchetypeId");

                    b.ToTable("ArchetypeGenres");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.LogLine", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ArchetypeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ThemeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ArchetypeId");

                    b.HasIndex("ThemeId");

                    b.ToTable("LogLines");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Story", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DevelopmentStage")
                        .HasColumnType("integer");

                    b.Property<Guid?>("LogLineId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("PublishedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("LogLineId")
                        .IsUnique();

                    b.HasIndex("Title", "DevelopmentStage")
                        .IsUnique();

                    b.ToTable("Storys");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.StoryLine", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ArchetypeBeatId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LineText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LineType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Sequence")
                        .HasColumnType("integer");

                    b.Property<Guid>("StoryId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ArchetypeBeatId");

                    b.HasIndex("StoryId");

                    b.ToTable("StoryLines");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Theme", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TagList")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Themes");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ThemeExtension", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Notion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ThemeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ThemeId");

                    b.ToTable("ThemeExtensions");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ArchetypeBeat", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.Archetype", "Archetype")
                        .WithMany("ArchetypeBeats")
                        .HasForeignKey("ArchetypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Archetype");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ArchetypeGenre", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.Archetype", "Archetype")
                        .WithMany("ArchetypeGenres")
                        .HasForeignKey("ArchetypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Archetype");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.LogLine", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.Archetype", "Archetype")
                        .WithMany()
                        .HasForeignKey("ArchetypeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FountainFlow.Api.Entities.Theme", "Theme")
                        .WithMany()
                        .HasForeignKey("ThemeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Archetype");

                    b.Navigation("Theme");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Story", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.LogLine", "LogLine")
                        .WithOne()
                        .HasForeignKey("FountainFlow.Api.Entities.Story", "LogLineId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("LogLine");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.StoryLine", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.ArchetypeBeat", "ArchetypeBeat")
                        .WithMany()
                        .HasForeignKey("ArchetypeBeatId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FountainFlow.Api.Entities.Story", "Story")
                        .WithMany("StoryLines")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ArchetypeBeat");

                    b.Navigation("Story");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.ThemeExtension", b =>
                {
                    b.HasOne("FountainFlow.Api.Entities.Theme", "Theme")
                        .WithMany("ThemeExtensions")
                        .HasForeignKey("ThemeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Theme");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Archetype", b =>
                {
                    b.Navigation("ArchetypeBeats");

                    b.Navigation("ArchetypeGenres");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Story", b =>
                {
                    b.Navigation("StoryLines");
                });

            modelBuilder.Entity("FountainFlow.Api.Entities.Theme", b =>
                {
                    b.Navigation("ThemeExtensions");
                });
#pragma warning restore 612, 618
        }
    }
}
