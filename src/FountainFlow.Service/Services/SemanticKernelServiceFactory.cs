using FountainFlow.Service.Config;
using FountainFlow.Service.Interfaces;

namespace FountainFlow.Service.Services;

public class SemanticKernelServiceFactory : ISemanticKernelServiceFactory
{
    private readonly GlobalSettings _globalSettings;

    public SemanticKernelServiceFactory(GlobalSettings globalSettings)
    {
        _globalSettings = globalSettings;
    }

    public ISemanticKernelService ClassifyFountainTextAsync()
    {
        return new SemanticKernelService(_globalSettings);
    }

    public ISemanticKernelService DeriveThemeAsync()
    {
        return new SemanticKernelService(_globalSettings);
    }
}
