namespace FountainFlow.Service.Services;

public class DocumentSaver
{
    private ISaveAs _saveStrategy;

    public DocumentSaver(ISaveAs saveStrategy)
    {
        _saveStrategy = saveStrategy;
    }

    public void SetSaveStrategy(ISaveAs saveStrategy)
    {
        _saveStrategy = saveStrategy;
    }

    public void SaveDocument(object data, string filePath)
    {
        _saveStrategy.Save(data, filePath);
    }
}
