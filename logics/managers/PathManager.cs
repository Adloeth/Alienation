using Godot;
using System;
using System.IO;

public partial class PathManager : Manager<PathManager>
{
    #if GODOT_LINUXBSD
        public static readonly string userDataPath = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "/.local/share/alienation/");
    #elif GODOT_WINDOWS
        public static string  userDataPath = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "/Alienation/");
    #elif GODOT_OSX
        public static string  userDataPath = string.Concat(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "/Library/Application Support/Alienation");
    #else
        #error This platform is not supported !
    #endif

    public static readonly string savePath = string.Concat(userDataPath, "save/");
    public static readonly string structurePath = string.Concat(userDataPath, "structures/");
    public static readonly string exportedStructurePath = string.Concat(structurePath, "exported/");
    public static readonly string editorStructurePath = string.Concat(structurePath, "edited/");
    public static readonly string settingsPath = string.Concat(userDataPath, "settings/");
    public static readonly string localePath = string.Concat(userDataPath, "locale/");

    public override void Ready()
    {
        GD.Print("");
        GD.Print("Paths :");
        GD.Print(string.Concat("\t- Data:              ", userDataPath));
        GD.Print(string.Concat("\t- Save:              ", savePath));
        GD.Print(string.Concat("\t- Structure:         ", structurePath));
        GD.Print(string.Concat("\t- ExportedStructure: ", exportedStructurePath));
        GD.Print(string.Concat("\t- EditorStructure:   ", editorStructurePath));
        GD.Print(string.Concat("\t- Settings:          ", settingsPath));
        GD.Print(string.Concat("\t- Locale:            ", localePath));
        GD.Print("");

        GeneratePaths();
    }

    public override void AllManagersReady()
    {
        
    }

    public void GeneratePaths()
    {
        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory(structurePath);
        Directory.CreateDirectory(exportedStructurePath);
        Directory.CreateDirectory(editorStructurePath);
        Directory.CreateDirectory(settingsPath);
        Directory.CreateDirectory(localePath);
    }
}