using System.Xml.Serialization;

namespace FountainFlow.Service.Services;

public class SaveAsXml : ISaveAs
{
    public void Save(object data, string filePath)
    {
        string typeName = data.GetType().Name;
        string subDirectory = Path.Combine(filePath, typeName);

        Directory.CreateDirectory(subDirectory);

        string fileName = $"{typeName}_{DateTime.Now:yyyyMMddHHmmss}.xml";
        string fullFilePath = Path.Combine(subDirectory, fileName);

        XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());
        using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Create))
        {
            xmlSerializer.Serialize(fileStream, data);
        }
    }
}
