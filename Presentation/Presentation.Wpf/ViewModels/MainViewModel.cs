using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Infrastructure.CodeGenerators;
using Presentation.Wpf.MVVM;

namespace Presentation.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IUmlParser _plantUmlParser;
    private readonly IUmlParser _xmlUmlParser;

    private readonly Dictionary<string, ICodeGenerator> _availableGenerators;

    private string _inputText = string.Empty;
    private string _outputText = string.Empty;
    private string _statusMessage = "Ready";
    private string _selectedInputFormat = "PlantUML";
    private string _selectedOutputLanguage = "C#";

    public MainViewModel()
    {
        _plantUmlParser = new PlantUmlParser();
        _xmlUmlParser = new XmlUmlParser();

        _availableGenerators = new List<ICodeGenerator>
        {
            new CSharpCodeGenerator(),
            new JavaCodeGenerator(),
            new GoCodeGenerator()
        }.ToDictionary(g => g.LanguageName);

        InputFormats = new ObservableCollection<string> { "PlantUML", "XML" };
        OutputLanguages = new ObservableCollection<string>(_availableGenerators.Keys);

        GenerateCommand = new RelayCommand(ExecuteGenerate);
    }

    public ObservableCollection<string> InputFormats { get; }
    public ObservableCollection<string> OutputLanguages { get; }

    public string SelectedInputFormat
    {
        get => _selectedInputFormat;
        set => SetProperty(ref _selectedInputFormat, value);
    }

    public string SelectedOutputLanguage
    {
        get => _selectedOutputLanguage;
        set => SetProperty(ref _selectedOutputLanguage, value);
    }

    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public string OutputText
    {
        get => _outputText;
        set => SetProperty(ref _outputText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand GenerateCommand { get; }

    private void ExecuteGenerate(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            StatusMessage = "Error: Input text is empty.";
            return;
        }

        try
        {
            var parser = SelectedInputFormat == "PlantUML" ? _plantUmlParser : _xmlUmlParser;

            var diagram = parser.Parse(InputText);

            if (_availableGenerators.TryGetValue(SelectedOutputLanguage, out var generator))
            {
                OutputText = generator.Generate(diagram);

                if (OutputText.Contains("TODO:"))
                    StatusMessage = $"Warning: Generation for {SelectedOutputLanguage} is not implemented.";
                else
                    StatusMessage = "Success: Code generated.";
            }
            else
                StatusMessage = $"Error: Generator for {SelectedOutputLanguage} not found.";
        }
        catch (Exception ex)
        {
            OutputText = string.Empty;
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}