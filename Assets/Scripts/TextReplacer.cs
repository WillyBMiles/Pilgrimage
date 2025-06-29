using System.Collections.Generic;
using UnityEngine;

public static class TextReplacer 
{
    static Dictionary<string, string> Replacements = new()
    {
        { "<tents>", "<sprite name=\"tents\" tint=1>" },
        { "<faith>", "<sprite name=\"faith\" tint=1>" },
        { "<food>", "<sprite name=\"food\" tint=1>" }
    };

    public static string Replace(string text)
    {
        foreach (var kvp in Replacements)
        {
            text = text.Replace(kvp.Key, kvp.Value);
        }
        return text;

    }
}
