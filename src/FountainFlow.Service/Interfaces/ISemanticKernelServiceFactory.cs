using FountainFlow.Service.Interfaces;

namespace FountainFlow.Service;

public interface ISemanticKernelServiceFactory
{
    ISemanticKernelService ClassifyFountainTextAsync();
    ISemanticKernelService DeriveThemeAsync();

}
