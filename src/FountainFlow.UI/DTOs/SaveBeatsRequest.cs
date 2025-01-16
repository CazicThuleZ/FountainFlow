using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FountainFlowUI.Models;

namespace FountainFlowUI.DTOs
{
    public class SaveBeatsRequest
    {
        [JsonPropertyName("archetypeId")]
        public Guid ArchetypeId { get; set; }

        [JsonPropertyName("beats")]
        public List<BeatUpdateModel> Beats { get; set; } = new();
    }

    public class BeatUpdateModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }  // Using string to handle both GUIDs and temp_ ids

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }        

        [JsonPropertyName("parentSequence")]
        public int ParentSequence { get; set; }

        [JsonPropertyName("childSequence")]
        public int? ChildSequence { get; set; }

        [JsonPropertyName("grandchildSequence")]
        public int? GrandchildSequence { get; set; }

        [JsonPropertyName("percentOfStory")]
        public int PercentOfStory { get; set; }

        [JsonPropertyName("archetypeId")]
        public Guid ArchetypeId { get; set; }
    }
}