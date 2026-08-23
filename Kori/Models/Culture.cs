using System.Collections.Generic;

namespace Kori.Models;

public sealed class Culture
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    
    public override string ToString(){
        return Name;
    }
    
    public static readonly List<Culture> GetSupportedLanguages = new List<Culture>
    {
        new Culture { Name = "English", Code = "en" },
        new Culture { Name = "Français", Code = "fr" },
        new Culture { Name = "Deutsch", Code = "de" },
        new Culture { Name = "日本語", Code = "ja" },
        new Culture { Name = "中文", Code = "zh" },
        new Culture { Name = "Italiano", Code = "it" },
        new Culture { Name = "Português", Code = "pt" }
    };
}