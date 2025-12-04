// Assets/Editor/WalletSchemePostprocessor.cs
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class WalletSchemePostprocessor
{
    private static readonly string[] WalletSchemes = { "crossx-stage", "crossx", "wc" };

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        var root = plist.root;
        var schemesArray = root.values.ContainsKey("LSApplicationQueriesSchemes")
            ? root["LSApplicationQueriesSchemes"].AsArray()
            : root.CreateArray("LSApplicationQueriesSchemes");

        foreach (var scheme in WalletSchemes)
        {
            var exists = false;
            foreach (var element in schemesArray.values)
            {
                if (element.AsString() == scheme)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                schemesArray.AddString(scheme);
        }

        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
