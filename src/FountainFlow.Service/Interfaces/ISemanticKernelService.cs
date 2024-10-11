using Microsoft.SemanticKernel;

namespace FountainFlow.Service.Interfaces;

public interface ISemanticKernelService
{
    Kernel Kernel { get; }
    Task<string> ClassifyFountainTextAsync(Dictionary<string, string> arguments);
    Task<string> DeriveThemeAsync(Dictionary<string, string> arguments);
    
}
