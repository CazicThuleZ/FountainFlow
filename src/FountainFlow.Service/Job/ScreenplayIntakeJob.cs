using FountainFlow.Service.Config;
using FountainFlow.Service.Interfaces;
using Microsoft.Extensions.Options;
using Quartz;
using Serilog;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class ScreenplayIntakeJob : IJob
{
    private readonly ILogger<ScreenplayIntakeJob> _logger;
    private readonly IntakeJobSettings _settings;
    private readonly ISemanticKernelService _semanticKernelService;
    private bool _titlePageZone = true;

    public ScreenplayIntakeJob(ILogger<ScreenplayIntakeJob> logger, IOptions<IntakeJobSettings> settings, ISemanticKernelService semanticKernelService)
    {
        _logger = logger;
        _settings = settings.Value;
        _semanticKernelService = semanticKernelService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Screenplay Intake Job started at {Time}", DateTimeOffset.Now);

        if (!Directory.Exists(_settings.IntakeDirectory))
        {
            _logger.LogError("Intake directory does not exist: {Directory}", _settings.IntakeDirectory);
            return;
        }

        foreach (var file in GetFiles(_settings.IntakeDirectory))
        {
            _titlePageZone = true;
            if (Path.GetExtension(file).Equals(".fountain", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing fountain file: {FileName}", Path.GetFileName(file));

                try
                {
                    HashSet<string> uniqueCharacters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    string fileContent;
                    try
                    {
                        fileContent = await File.ReadAllTextAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading file: {FileName}", Path.GetFileName(file));
                        continue; // Skip to the next file
                    }

                    var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var classifications = new List<string>();

                    var previousLineClassification = "BlankLine";
                    for (int i = 0; i < lines.Length; i++)
                    {
                        // string previousLine = i > 0 ? lines[i - 1] : string.Empty;
                        string lineUnderReview = lines[i];
                        string nextLine = i < lines.Length - 1 ? lines[i + 1] : string.Empty;

                        Dictionary<string, string> arguments = new Dictionary<string, string>
                        {
                            { "previousLineClassification", previousLineClassification },
                            { "lineUnderReview", lineUnderReview },
                            { "nextLine", nextLine }
                        };

                        string classification;
                        classification = ClassifyLine(lineUnderReview, i, lines, classifications);
                        classifications.Add(classification);
                        _logger.LogInformation($"{classification} : {lineUnderReview}");

                        if (classification == "Character")
                        {
                            string characterName = lineUnderReview.Split('(')[0].Trim();

                            // string characterName = Regex.Match(lineUnderReview, @"^([^(]+)").Groups[1].Value.Trim();

                            uniqueCharacters.Add(characterName);
                        }

                        // string classification;
                        // if (string.IsNullOrWhiteSpace(lineUnderReview))
                        // {
                        //     previousLineClassification = "BlankLine";
                        //     classification = "BlankLine";
                        // }
                        // else
                        // {
                        //     //_logger.Information($"Previous Classification : {previousLineClassification} " );
                        //     //classification = await _semanticKernelService.ClassifyFountainTextAsync(arguments);
                        //     classification = ClassifyLine(lineUnderReview, i, lines, classifications);
                        //     classifications.Add(classification);
                        //     _logger.Information($"{classification} : {lineUnderReview}");
                        //     //previousLineClassification = classification.Replace("Fountain Element:", string.Empty).Trim();

                        //     if (classification == "Character")
                        //     {
                        //         string characterName = Regex.Match(lineUnderReview, @"^([^(]+)").Groups[1].Value.Trim();

                        //         uniqueCharacters.Add(characterName);
                        //     }
                        // }

                    }

                    _logger.LogInformation("All unique characters found across all processed files:");
                    foreach (var character in uniqueCharacters)
                    {
                        _logger.LogInformation("Unique Character: {CharacterName}", character);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing fountain file: {FileName}", Path.GetFileName(file));
                }
            }
            else
            {
                _logger.LogWarning("Skipping non-fountain file: {FileName}", Path.GetFileName(file));
            }
        }

        _logger.LogInformation("Screenplay Intake Job completed at {Time}", DateTimeOffset.Now);
    }

    private IEnumerable<string> GetFiles(string directory)
    {
        return Directory.EnumerateFiles(directory);
    }

    private string ClassifyLine(string line, int index, string[] lines, List<string> classifications)
    {
        string trimmedLine = line.Trim();

        if (string.IsNullOrWhiteSpace(line) || string.IsNullOrEmpty(line))
            return "BlankLine";

        if (IsTitlePageLine(trimmedLine) && _titlePageZone)
            return "TitlePage";

        _titlePageZone = false;

        if (trimmedLine.StartsWith("~"))
            return "Lyrics";

        if (IsSceneHeading(line, index, lines))
            return "SceneHeading";

        if (IsTransition(line, index, lines))
            return "Transition";

        string characterClassification = GetCharacterClassification(line, index, lines, classifications);
        if (characterClassification != null)
            return characterClassification;

        if (IsParenthetical(line, classifications))
            return "Parenthetical";

        if (IsDialogue(line, classifications))
            return "Dialogue";

        if (IsAction(line))
            return "Action";

        return "Action";
    }

    private bool IsTitlePageLine(string line)
    {
        int colonIndex = line.IndexOf(':');

        return colonIndex > 0 && colonIndex < line.Length - 1;
    }

    private bool IsSceneHeading(string line, int index, string[] lines)
    {
        string trimmedLine = line.Trim();

        if (trimmedLine.StartsWith(".") && !trimmedLine.StartsWith("..."))
        {
            return HasBlankLinesAround(index, lines);
        }

        string[] prefixes = {
            "INT", "INT.", "EXT", "EXT.", "EST", "EST.",
            "INT/EXT", "INT./EXT", "I/E"
        };

        if (prefixes.Any(prefix => trimmedLine.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return HasBlankLinesAround(index, lines);

        return false;
    }

    private bool IsTransition(string line, int index, string[] lines)
    {
        string trimmedLine = line.Trim();

        if (trimmedLine == trimmedLine.ToUpper() && trimmedLine.Contains("CUT TO:"))
            return true;

        return false;
    }

    private string GetCharacterClassification(string line, int index, string[] lines, List<string> classifications)
    {
        string trimmedLine = line.Trim();

        if (trimmedLine.StartsWith("@"))
        {
            return "Character";
        }

        bool endsWithCaret = trimmedLine.EndsWith("^");

        if (endsWithCaret)
        {
            trimmedLine = trimmedLine.Substring(0, trimmedLine.Length - 1).TrimEnd();
        }

        string characterName = trimmedLine;
        int parenIndex = trimmedLine.IndexOf('(');
        if (parenIndex > 0)
        {
            characterName = trimmedLine.Substring(0, parenIndex).TrimEnd();
        }

        if (characterName == characterName.ToUpper() && characterName.Any(char.IsLetter))
        {
            if (!IsSceneHeading(line, index, lines) && HasBlankLineBefore(index, lines) && !HasBlankLineAfter(index, lines))
            {
                return endsWithCaret ? "DualDialogue" : "Character";
            }
        }

        return null;
    }


    private bool IsParenthetical(string line, List<string> classifications)
    {
        string trimmedLine = line.Trim();

        if (trimmedLine.StartsWith("(") && trimmedLine.EndsWith(")"))
        {
            if (classifications.Count > 0)
            {
                string prevClassification = classifications[classifications.Count - 1];
                return prevClassification == "Character" || prevClassification == "Dialogue";
            }
        }

        return false;
    }

    private bool IsDialogue(string line, List<string> classifications)
    {
        if (classifications.Count > 0)
        {
            string prevClassification = classifications[classifications.Count - 1];
            if (prevClassification == "BlankLine")
                return false;

            if ((prevClassification == "Character" || prevClassification == "Parenthetical" || prevClassification == "Dialogue") &&
                !string.IsNullOrWhiteSpace(line))
                return true;

        }

        return false;
    }

    private bool IsAction(string line)
    {
        return line.TrimStart().StartsWith("!");
    }

    private bool HasBlankLinesAround(int index, string[] lines)
    {
        return HasBlankLineBefore(index, lines) && HasBlankLineAfter(index, lines);
    }

    private bool HasBlankLineBefore(int index, string[] lines)
    {
        if (index == 0)
            return true;

        return string.IsNullOrWhiteSpace(lines[index - 1]);
    }

    private bool HasBlankLineAfter(int index, string[] lines)
    {
        if (index >= lines.Length - 1)
            return true;

        return string.IsNullOrWhiteSpace(lines[index + 1]);
    }
}