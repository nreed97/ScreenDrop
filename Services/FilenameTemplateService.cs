using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScreenDrop.Services;

public class FilenameTemplateService
{
    public static string Generate(string template, string type = "screenshot")
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            template = "{timestamp}_{random}";
        }

        var now = DateTime.Now;
        var result = template;

        // Replace date/time variables
        result = result.Replace("{date}", now.ToString("yyyyMMdd"));
        result = result.Replace("{time}", now.ToString("HHmmss"));
        result = result.Replace("{timestamp}", now.ToString("yyyyMMdd_HHmmss"));
        
        // Replace random variable (8 character hex)
        result = result.Replace("{random}", Guid.NewGuid().ToString("N")[..8]);
        
        // Replace type variable
        result = result.Replace("{type}", type.ToLower());

        // Sanitize the result to remove invalid filename characters
        result = SanitizeFilename(result);

        return result;
    }

    public static string GenerateExample(string template)
    {
        return Generate(template, "screenshot");
    }

    public static bool IsValidTemplate(string template, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(template))
        {
            error = "Template cannot be empty";
            return false;
        }

        // Check for invalid Windows filename characters (except / for folders)
        var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*' };
        foreach (var c in invalidChars)
        {
            if (template.Contains(c))
            {
                error = $"Template contains invalid character: {c}";
                return false;
            }
        }

        // Try to generate a test filename
        try
        {
            var test = Generate(template, "test");
            if (string.IsNullOrWhiteSpace(test))
            {
                error = "Template generates empty filename";
                return false;
            }
        }
        catch (Exception ex)
        {
            error = $"Template error: {ex.Message}";
            return false;
        }

        return true;
    }

    private static string SanitizeFilename(string filename)
    {
        // Remove invalid characters but preserve forward slashes for folders
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var pattern = "[" + Regex.Escape(new string(invalidChars.Where(c => c != '/').ToArray())) + "]";
        return Regex.Replace(filename, pattern, "_");
    }
}
