using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScreenDrop.Models;
using ScreenDrop.Services;
using ScreenDrop.ViewModels;

namespace ScreenDrop.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsService settingsService, AppSettings settings, DatabaseService? databaseService = null)
    {
        InitializeComponent();
        
        _viewModel = new SettingsViewModel(settingsService, settings, databaseService);
        _viewModel.StatusMessage = "Configure your DigitalOcean Spaces credentials";
        
        DataContext = _viewModel;
        
        SecretKeyBox.Password = settings.SecretKey;
        
        Loaded += (_, _) =>
        {
            if (!settings.IsConfigured)
            {
                AccessKeyBox.Focus();
            }
        };
    }

    private void SecretKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.SecretKey = SecretKeyBox.Password;
        }
    }

    private void SaveAndClose_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCommand.Execute(null);
        Close();
    }

    private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        // Ignore modifier keys by themselves
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
            e.Key == Key.LWin || e.Key == Key.RWin ||
            e.Key == Key.System)
        {
            return;
        }

        // Build hotkey string
        var sb = new StringBuilder();
        
        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            sb.Append("Ctrl+");
        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            sb.Append("Shift+");
        if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            sb.Append("Alt+");
        if ((Keyboard.Modifiers & ModifierKeys.Windows) != 0)
            sb.Append("Win+");

        // Add the key
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        sb.Append(key.ToString());

        var hotkeyString = sb.ToString();

        // Update the appropriate property
        if (sender == FullScreenHotkeyBox)
        {
            _viewModel.FullScreenHotkey = hotkeyString;
        }
        else if (sender == RegionHotkeyBox)
        {
            _viewModel.RegionHotkey = hotkeyString;
        }
    }

    public bool ClipboardMonitoringEnabled
    {
        get => _viewModel.ClipboardMonitoringEnabled;
        set => _viewModel.ClipboardMonitoringEnabled = value;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // The binding handles this automatically via OnSearchTextChanged in ViewModel
    }

    private void TemplateBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        string template = textBox.Text;
        string extension = "";
        TextBlock? exampleLabel = null;

        // Determine which template and example label to update
        if (textBox.Name == "ScreenshotTemplateBox")
        {
            exampleLabel = ScreenshotTemplateExample;
            extension = ".png";
        }
        else if (textBox.Name == "FileTemplateBox")
        {
            exampleLabel = FileTemplateExample;
            extension = ".pdf";
        }

        if (exampleLabel == null) return;

        // Validate and generate example
        if (FilenameTemplateService.IsValidTemplate(template, out string error))
        {
            var exampleFilename = FilenameTemplateService.GenerateExample(template) + extension;
            exampleLabel.Text = $"Example: {exampleFilename}";
            exampleLabel.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)); // Gray
        }
        else
        {
            exampleLabel.Text = $"Error: {error}";
            exampleLabel.Foreground = new SolidColorBrush(Colors.Red);
        }
    }
}
