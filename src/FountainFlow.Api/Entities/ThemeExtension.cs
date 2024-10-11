using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

// This table extends the theme table.  A theme can have multiple extensions, the idea being that we are storing sentiments such as quotes,
// anecdotes, proverbs or other ideas that support the main theme.

[Table("ThemeExtensions")]
public class ThemeExtension
{
    public Guid Id { get; set; }
    public Guid ThemeId { get; set; }
    public string Notion { get; set; }    
    public Theme Theme { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
