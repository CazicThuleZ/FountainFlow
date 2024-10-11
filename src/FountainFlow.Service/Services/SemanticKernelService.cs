using Azure.AI.OpenAI;
using FountainFlow.Service.Config;
using FountainFlow.Service.Interfaces;
using FountainFlow.Service.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

namespace FountainFlow.Service.Services;
public class SemanticKernelService : ISemanticKernelService
{
    public Kernel Kernel { get; }

    private readonly string _kernel_model;
        private readonly string _token_usage_log_path;

    public SemanticKernelService(GlobalSettings globalSettings)
    {
        Kernel = InitializeKernel(globalSettings);
        _kernel_model = globalSettings.OpenApiModel;
        _token_usage_log_path = globalSettings.DashboardLogLocation;
    }

    private Kernel InitializeKernel(GlobalSettings globalSettings)
    {

        var endpoint = new Uri("http://localhost:11434");
        var builder = Kernel.CreateBuilder();                            
        # pragma warning disable SKEXP0070
        //var builder = Kernel.CreateBuilder()
        //    .AddOllamaChatCompletion(endpoint: new Uri("http://localhost:11434"), modelId: "llama3.1")
        //    .Build();

        // builder.Plugins.AddFromType<ConversationSummaryPlugin>();
        builder.Plugins.AddFromPromptDirectory(Path.Combine(globalSettings.SemanticKernelPluginsPath, "ParseFountain"));
        // builder.Plugins.AddFromPromptDirectory(Path.Combine(globalSettings.SemanticKernelPluginsPath, "DeriveTheme"));
        // builder.Plugins.AddFromPromptDirectory(Path.Combine(globalSettings.SemanticKernelPluginsPath, "IdentifyProtagonist"));
        // builder.Plugins.AddFromPromptDirectory(Path.Combine(globalSettings.SemanticKernelPluginsPath, "IdentifyAntagonist"));
        
        //builder.Services.AddOpenAIChatCompletion(globalSettings.OpenApiModel, globalSettings.OpenApiKey, endpoint.ToString());
        builder.Services.AddOllamaChatCompletion(endpoint: new Uri("http://localhost:11434"), modelId: "llama3.1");
        //builder.Services.AddOpenAIChatCompletion(globalSettings.OpenApiModel,globalSettings.OpenApiKey, endpoint);

        return builder.Build();
    }

    public async Task<string> ClassifyFountainTextAsync(Dictionary<string, string> arguments)
    {
        KernelPlugin parseFountainPlugin = Kernel.Plugins.FirstOrDefault(plugin => plugin.Name.Equals("ParseFountain", StringComparison.OrdinalIgnoreCase));

        var kernelArguments = new KernelArguments();
        foreach (var arg in arguments)
        {
            kernelArguments.Add(arg.Key, arg.Value);
        }

        var fountainElementType = await Kernel.InvokeAsync(parseFountainPlugin["InterpretMarkdown"], kernelArguments);
        ProcessTokenUsage(fountainElementType.Metadata);
        return fountainElementType.ToString();       

    }

    public Task<string> DeriveThemeAsync(Dictionary<string, string> arguments)
    {
        throw new NotImplementedException();
    }

    private void ProcessTokenUsage(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata == null) // probably locally hosted if true
            return;

        if (metadata.ContainsKey("Usage"))
        {
            var usage = (CompletionsUsage)metadata["Usage"];

            LogTokenUsage logTokenUsage = new()
            {
                SnapshotDateUTC = DateTime.UtcNow,
                Model = _kernel_model,
                PromptTokens = usage.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                TotalTokens = usage.TotalTokens
            };

            var documentSaver = new DocumentSaver(new SaveAsJson());
            documentSaver.SaveDocument(logTokenUsage, _token_usage_log_path);
        }
    }
}