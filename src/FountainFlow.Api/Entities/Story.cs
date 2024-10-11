#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

[Table("Storys")]
public class Story
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DevelopmentStage DevelopmentStage { get; set; }
    public DateTime PublishedUTC { get; set; }
    public ICollection<StoryLine> StoryLines { get; set; } = new List<StoryLine>();
    public Guid? LogLineId { get; set; }
    public LogLine? LogLine { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}

