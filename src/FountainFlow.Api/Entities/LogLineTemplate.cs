using System;

namespace FountainFlow.Api.Entities;

public class LogLineTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }

}
