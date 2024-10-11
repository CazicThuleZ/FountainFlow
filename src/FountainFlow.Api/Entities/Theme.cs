using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FountainFlow.Api.Entities;

[Table("Themes")]
public class Theme
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public string TagList { get; set; }
    public ICollection<ThemeExtension> ThemeExtensions { get; set; } = new List<ThemeExtension>();
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
}
