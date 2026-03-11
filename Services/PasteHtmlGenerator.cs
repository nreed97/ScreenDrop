using System;
using System.Net;
using System.Text;

namespace ScreenDrop.Services;

public class PasteHtmlGenerator
{
    public static string GenerateHtml(string content, string title, bool enableSyntaxHighlighting, bool showLineNumbers)
    {
        var encodedTitle = !string.IsNullOrEmpty(title) ? WebUtility.HtmlEncode(title) : "Untitled Paste";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss UTC");
        
        // HTML encode content first
        var encodedContent = WebUtility.HtmlEncode(content);
        
        // Split content into lines for line numbers
        var lines = encodedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html lang=\"en\">");
        htmlBuilder.AppendLine("<head>");
        htmlBuilder.AppendLine("    <meta charset=\"UTF-8\">");
        htmlBuilder.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        htmlBuilder.AppendLine($"    <title>{encodedTitle}</title>");
        
        if (enableSyntaxHighlighting)
        {
            // Include Prism.js for syntax highlighting with line numbers support
            htmlBuilder.AppendLine("    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism-tomorrow.min.css\">");
            htmlBuilder.AppendLine("    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.css\">");
        }
        
        htmlBuilder.AppendLine("    <style>");
        htmlBuilder.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
        htmlBuilder.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background: #0d1117; color: #c9d1d9; min-height: 100vh; }");
        htmlBuilder.AppendLine("        .container { max-width: 100%; width: 100%; margin: 0; padding: 20px; }");
        htmlBuilder.AppendLine("        .header { background: #161b22; border: 1px solid #30363d; border-radius: 6px; padding: 16px 20px; margin-bottom: 20px; width: 100%; }");
        htmlBuilder.AppendLine("        .header h1 { font-size: 24px; margin-bottom: 8px; color: #f0f6fc; word-wrap: break-word; }");
        htmlBuilder.AppendLine("        .header .meta { font-size: 14px; color: #8b949e; }");
        htmlBuilder.AppendLine("        .actions { display: flex; gap: 10px; margin-bottom: 20px; flex-wrap: wrap; }");
        htmlBuilder.AppendLine("        .btn { background: #238636; color: white; border: none; padding: 8px 16px; border-radius: 6px; cursor: pointer; font-size: 14px; text-decoration: none; display: inline-block; }");
        htmlBuilder.AppendLine("        .btn:hover { background: #2ea043; }");
        htmlBuilder.AppendLine("        .btn-secondary { background: #21262d; }");
        htmlBuilder.AppendLine("        .btn-secondary:hover { background: #30363d; }");
        htmlBuilder.AppendLine("        .code-container { background: #161b22; border: 1px solid #30363d; border-radius: 6px; overflow: auto; width: 100%; }");
        
        if (showLineNumbers && !enableSyntaxHighlighting)
        {
            // Custom line numbers (no syntax highlighting)
            htmlBuilder.AppendLine("        .code-wrapper { display: flex; }");
            htmlBuilder.AppendLine("        .line-numbers { background: #0d1117; color: #6e7681; text-align: right; user-select: none; border-right: 1px solid #30363d; padding: 0 12px 0 16px; min-width: 50px; }");
            htmlBuilder.AppendLine("        .line-numbers span { font-family: 'Consolas', 'Monaco', 'Courier New', monospace; font-size: 12px; line-height: 1.5; display: block; }");
            htmlBuilder.AppendLine("        .code-content { flex: 1; overflow-x: auto; padding: 0 16px 0 16px; min-width: 0; }");
            htmlBuilder.AppendLine("        pre { margin: 0; padding: 0; }");
        }
        else
        {
            // Prism.js line numbers or no line numbers
            htmlBuilder.AppendLine("        pre { margin: 0 !important; overflow: auto; }");
            htmlBuilder.AppendLine("        pre[class*=\"language-\"] { margin: 0; padding: 16px; background: #161b22; }");
        }
        
        htmlBuilder.AppendLine("        code { font-family: 'Consolas', 'Monaco', 'Courier New', monospace; font-size: 12px; line-height: 1.5; display: block; white-space: pre; }");
        
        if (enableSyntaxHighlighting)
        {
            // Prism customizations for dark theme
            htmlBuilder.AppendLine("        .line-numbers .line-numbers-rows { border-right: 1px solid #30363d !important; }");
            htmlBuilder.AppendLine("        .line-numbers .line-numbers-rows > span:before { color: #6e7681 !important; font-family: 'Consolas', 'Monaco', 'Courier New', monospace !important; font-size: 12px !important; }");
            htmlBuilder.AppendLine("        pre[class*=\"language-\"] { background: #161b22 !important; font-size: 12px !important; }");
            htmlBuilder.AppendLine("        code[class*=\"language-\"] { color: #c9d1d9; font-size: 12px !important; line-height: 1.5 !important; }");
        }
        
        htmlBuilder.AppendLine("        .stats { margin-top: 20px; padding: 12px; background: #161b22; border: 1px solid #30363d; border-radius: 6px; font-size: 13px; color: #8b949e; width: 100%; box-sizing: border-box; }");
        
        // Responsive design for smaller screens
        htmlBuilder.AppendLine("        @media (max-width: 768px) {");
        htmlBuilder.AppendLine("            .container { padding: 10px; }");
        htmlBuilder.AppendLine("            .header { padding: 12px 16px; }");
        htmlBuilder.AppendLine("            .header h1 { font-size: 20px; }");
        htmlBuilder.AppendLine("            code { font-size: 11px; }");
        htmlBuilder.AppendLine("            .line-numbers span { font-size: 11px; }");
        htmlBuilder.AppendLine("        }");
        
        htmlBuilder.AppendLine("    </style>");
        htmlBuilder.AppendLine("</head>");
        htmlBuilder.AppendLine("<body>");
        htmlBuilder.AppendLine("    <div class=\"container\">");
        htmlBuilder.AppendLine("        <div class=\"header\">");
        htmlBuilder.AppendLine($"            <h1>{encodedTitle}</h1>");
        htmlBuilder.AppendLine($"            <div class=\"meta\">Uploaded on {timestamp}</div>");
        htmlBuilder.AppendLine("        </div>");
        
        htmlBuilder.AppendLine("        <div class=\"actions\">");
        htmlBuilder.AppendLine("            <button class=\"btn\" onclick=\"copyToClipboard(event)\">Copy to Clipboard</button>");
        htmlBuilder.AppendLine("            <button class=\"btn btn-secondary\" onclick=\"downloadRaw()\">Download Raw</button>");
        htmlBuilder.AppendLine("        </div>");
        
        // Count lines accurately - don't count trailing empty lines from trailing newlines
        var lineCount = lines.Length;
        if (lineCount > 0 && string.IsNullOrEmpty(lines[lineCount - 1]))
        {
            lineCount--; // Don't count the trailing empty line from a trailing newline
        }
        
        htmlBuilder.AppendLine("        <div class=\"code-container\">");
        
        if (enableSyntaxHighlighting && showLineNumbers)
        {
            // Use Prism.js with built-in line numbers
            // Use language-markup as default which provides basic highlighting
            htmlBuilder.Append("            <pre class=\"line-numbers\"><code id=\"paste-content\" class=\"language-clike\">");
            htmlBuilder.Append(encodedContent.TrimEnd('\r', '\n'));
            htmlBuilder.AppendLine("</code></pre>");
        }
        else if (showLineNumbers)
        {
            // Custom line numbers (no syntax highlighting)
            htmlBuilder.AppendLine("            <div class=\"code-wrapper\">");
            htmlBuilder.AppendLine("                <div class=\"line-numbers\">");
            for (int i = 1; i <= lineCount; i++)
            {
                htmlBuilder.AppendLine($"                    <span>{i}</span>");
            }
            htmlBuilder.AppendLine("                </div>");
            htmlBuilder.AppendLine("                <div class=\"code-content\">");
            htmlBuilder.Append("                    <pre><code id=\"paste-content\">");
            for (int i = 0; i < lineCount; i++)
            {
                htmlBuilder.Append(lines[i]);
                if (i < lineCount - 1) htmlBuilder.AppendLine("");
            }
            htmlBuilder.AppendLine("</code></pre>");
            htmlBuilder.AppendLine("                </div>");
            htmlBuilder.AppendLine("            </div>");
        }
        else
        {
            // No line numbers
            htmlBuilder.Append("            <pre><code id=\"paste-content\">");
            htmlBuilder.Append(encodedContent.TrimEnd('\r', '\n'));
            htmlBuilder.AppendLine("</code></pre>");
        }
        
        htmlBuilder.AppendLine("        </div>");
        
        var charCount = content.Length;
        var wordCount = content.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        htmlBuilder.AppendLine("        <div class=\"stats\">");
        htmlBuilder.AppendLine($"            {lineCount:N0} lines · {charCount:N0} characters · {wordCount:N0} words");
        htmlBuilder.AppendLine("        </div>");
        htmlBuilder.AppendLine("    </div>");
        
        if (enableSyntaxHighlighting)
        {
            // Include Prism.js scripts
            htmlBuilder.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/prism.min.js\"></script>");
            htmlBuilder.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/autoloader/prism-autoloader.min.js\"></script>");
            htmlBuilder.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.js\"></script>");
        }
        
        htmlBuilder.AppendLine("    <script>");
        htmlBuilder.AppendLine("        function copyToClipboard(event) {");
        htmlBuilder.AppendLine("            const text = document.getElementById('paste-content').textContent;");
        htmlBuilder.AppendLine("            navigator.clipboard.writeText(text).then(() => {");
        htmlBuilder.AppendLine("                const btn = event.target;");
        htmlBuilder.AppendLine("                const originalText = btn.textContent;");
        htmlBuilder.AppendLine("                btn.textContent = 'Copied!';");
        htmlBuilder.AppendLine("                btn.style.background = '#2ea043';");
        htmlBuilder.AppendLine("                setTimeout(() => {");
        htmlBuilder.AppendLine("                    btn.textContent = originalText;");
        htmlBuilder.AppendLine("                    btn.style.background = '#238636';");
        htmlBuilder.AppendLine("                }, 2000);");
        htmlBuilder.AppendLine("            }).catch(err => {");
        htmlBuilder.AppendLine("                alert('Failed to copy to clipboard');");
        htmlBuilder.AppendLine("            });");
        htmlBuilder.AppendLine("        }");
        htmlBuilder.AppendLine("");
        htmlBuilder.AppendLine("        function downloadRaw() {");
        htmlBuilder.AppendLine("            const text = document.getElementById('paste-content').textContent;");
        htmlBuilder.AppendLine("            const blob = new Blob([text], { type: 'text/plain' });");
        htmlBuilder.AppendLine("            const url = window.URL.createObjectURL(blob);");
        htmlBuilder.AppendLine("            const a = document.createElement('a');");
        htmlBuilder.AppendLine("            a.href = url;");
        htmlBuilder.AppendLine("            a.download = 'paste.txt';");
        htmlBuilder.AppendLine("            document.body.appendChild(a);");
        htmlBuilder.AppendLine("            a.click();");
        htmlBuilder.AppendLine("            document.body.removeChild(a);");
        htmlBuilder.AppendLine("            window.URL.revokeObjectURL(url);");
        htmlBuilder.AppendLine("        }");
        htmlBuilder.AppendLine("    </script>");
        htmlBuilder.AppendLine("</body>");
        htmlBuilder.AppendLine("</html>");
        
        return htmlBuilder.ToString();
    }
}
