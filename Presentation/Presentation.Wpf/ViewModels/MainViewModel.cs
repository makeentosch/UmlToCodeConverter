using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Infrastructure.CodeGenerators;
using Presentation.Wpf.MVVM;

namespace Presentation.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly CodeGenerationService _generationService;

    private string _inputText = string.Empty;
    private string _outputText = string.Empty;
    private string _statusMessage = "Ready";
    private string _selectedInputFormat = "PlantUML";
    private string _selectedOutputLanguage = "C#";
    private bool _hasError;

    public MainViewModel()
    {
        _generationService = new CodeGenerationService();

        InputFormats = new ObservableCollection<string> { "PlantUML", "XML" };
        OutputLanguages = new ObservableCollection<string> { "C#", "Java", "Go" };

        GenerateCommand = new RelayCommand(ExecuteGenerate);
    }

    public ObservableCollection<string> InputFormats { get; }
    public ObservableCollection<string> OutputLanguages { get; }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

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
        var result = _generationService.Process(InputText, SelectedInputFormat, SelectedOutputLanguage);

        HasError = !result.IsSuccess;
        StatusMessage = result.IsSuccess ? "Code generated successfully." : "Ready";
        OutputText = result.Content;
    }
}