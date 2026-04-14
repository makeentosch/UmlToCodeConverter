using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Presentation.Wpf.Helpers;
using Presentation.Wpf.ViewModels;

namespace Presentation.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    
        var vm = (MainViewModel)DataContext;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.OutputText) || e.PropertyName == nameof(vm.HasError))
                UpdateCodeDisplay(vm.OutputText, vm.HasError);
        };
    }

    private void UpdateCodeDisplay(string text, bool isFailed)
    {
        CodeOutputDisplay.Document.Blocks.Clear();
        var p = new Paragraph(new Run(text));
        CodeOutputDisplay.Document.Blocks.Add(p);

        if (isFailed)
            CodeOutputDisplay.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107));
        else
        {
            CodeOutputDisplay.Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212));
            SyntaxDecorator.Highlight(CodeOutputDisplay.Document);
        }
    }
}