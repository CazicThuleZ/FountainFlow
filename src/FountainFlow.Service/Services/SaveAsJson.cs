using System.Text.Json;

namespace FountainFlow.Service.Services;

public class SaveAsJson : ISaveAs
{
    public void Save(object data, string filePath)
    {
        string typeName = data.GetType().Name;
        string subDirectory = Path.Combine(filePath, typeName);

        Directory.CreateDirectory(subDirectory);

        string fileName = $"{typeName}_{DateTime.Now:yyyyMMddHHmmss}.json";
        string savePath = Path.Combine(subDirectory, fileName);

        string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        File.WriteAllText(savePath, jsonString);
    }
}
