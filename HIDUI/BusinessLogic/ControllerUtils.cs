using System;
using System.Linq;

namespace HIDUI.BusinessLogic;

public static class ControllerUtils
{
    public static string GetMatchingPath(string[] filePaths, string currentValue, Func<string, bool> isMatchDelegate)
    {
        var matches = filePaths.Where(isMatchDelegate).ToList();
        if (matches.Count == 1)
            return matches.Single();

        return matches.Count > 1 ? default : currentValue;
    }
}