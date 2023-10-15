using System.Text.RegularExpressions;

namespace DragonAttacker.Utils;

public static partial class Regexes
{
    public static readonly Regex FloatNumber = FloatNumberRegex();
    
    [GeneratedRegex(@"\d+\.?\d+")]
    private static partial Regex FloatNumberRegex();
}