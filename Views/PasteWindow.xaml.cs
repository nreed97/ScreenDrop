using System.Text.RegularExpressions;
using System.Windows;

namespace ScreenDrop.Views;

public partial class PasteWindow : Window
{
    public string PasteTitle { get; private set; } = string.Empty;
    public string PasteContent { get; private set; } = string.Empty;
    public string UrlSlug { get; private set; } = string.Empty;
    public bool EnableSyntaxHighlighting { get; private set; }
    public bool ShowLineNumbers { get; private set; }
    public bool WasUploaded { get; private set; }

    public PasteWindow()
    {
        InitializeComponent();
        ContentTextBox.TextChanged += ContentTextBox_TextChanged;
        
        // Try to paste from clipboard automatically
        if (Clipboard.ContainsText())
        {
            ContentTextBox.Text = Clipboard.GetText();
        }
    }

    private void ContentTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        CharCountTextBlock.Text = $"{ContentTextBox.Text.Length:N0} characters";
    }

    private void TitleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Auto-generate URL slug from title if slug field is empty
        if (string.IsNullOrWhiteSpace(UrlSlugTextBox.Text))
        {
            var suggestedSlug = GenerateSlugFromTitle(TitleTextBox.Text);
            UrlSlugTextBox.Text = suggestedSlug;
        }
    }

    private static string GenerateSlugFromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Convert to lowercase
        var slug = title.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove invalid characters (keep only letters, numbers, hyphens)
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit length to 50 characters
        if (slug.Length > 50)
            slug = slug.Substring(0, 50).TrimEnd('-');

        return slug;
    }

    private void Upload_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ContentTextBox.Text))
        {
            MessageBox.Show("Please enter some text content to upload.", "No Content", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PasteTitle = TitleTextBox.Text.Trim();
        PasteContent = ContentTextBox.Text;
        UrlSlug = GenerateSlugFromTitle(UrlSlugTextBox.Text.Trim());
        EnableSyntaxHighlighting = SyntaxHighlightCheckBox.IsChecked == true;
        ShowLineNumbers = LineNumbersCheckBox.IsChecked == true;
        WasUploaded = true;
        
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        WasUploaded = false;
        DialogResult = false;
        Close();
    }
}
