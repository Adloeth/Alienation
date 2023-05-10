using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

public partial class SettingsManager : Manager<SettingsManager>
{
    private Settings settings;

    public override void Ready()
    {
        
    }

    public override void AllManagersReady()
    {
        settings = GetSettings();
        ApplySettings(settings);
    }

    public static Settings GetSettings()
    {
        if(!Directory.Exists(PathManager.settingsPath))
            throw new Exception("Settings path doesn't exist.");

        string jsonPath = string.Concat(PathManager.settingsPath, "settings.json");
        if(!File.Exists(jsonPath))
        {
            Settings result = Settings.Default;
            using(FileStream file = File.Open(jsonPath, FileMode.CreateNew))
                JsonSerializer.Serialize(file, result, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true });

            return result;
        }

        string json = null;
        using(FileStream file = File.Open(jsonPath, FileMode.Open))
            using(StreamReader reader = new StreamReader(file))
                json = reader.ReadToEnd();

        GD.Print(json);

        return JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true });
    }

    public static void ApplySettings(Settings settings)
    {
        GD.Print("settings.graphics.fullscreenMode '" + settings.graphics.FullScreenMode.ToSettingValue() + "' (" + settings.graphics.fullscreenMode + ")");
        GD.Print("Window mode : '" + settings + "'");

        DisplayServer.WindowSetMode(settings.graphics.FullScreenMode.ToSettingValue());
        //ProjectSettings.SetSetting("display/window/size/borderless", settings.graphics.FullScreenMode == FullScreenMode.Borderless);

        DisplayServer.WindowSetVsyncMode(settings.graphics.vSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        Engine.MaxFps = settings.graphics.maxFPS;
        
        RenderingServer.GISetUseHalfResolution(settings.graphics.halfResolutionGI);

        RenderingServer.EnvironmentSetSsaoQuality(
            settings.graphics.SSAOQuality.ToSettingValue(), 
            settings.graphics.SSAOQuality <= SSAOQuality.Low,
            (float)ProjectSettings.GetSetting("rendering/environment/ssao/adaptive_target"),
            (int)ProjectSettings.GetSetting("rendering/environment/ssao/blur_passes"),
            (float)ProjectSettings.GetSetting("rendering/environment/ssao/fadeout_from"),
            (float)ProjectSettings.GetSetting("rendering/environment/ssao/fadeout_to")
        );

        RenderingServer.EnvironmentGlowSetUseBicubicUpscale(settings.graphics.glowQuality);

        RenderingServer.EnvironmentSetVolumetricFogFilterActive(settings.graphics.volumetricFogQuality);

        RenderingServer.SubSurfaceScatteringSetQuality(settings.graphics.SubsurfaceScatteringQuality.ToSettingValue());

        int positionalShadowAtlasSize = (int)ProjectSettings.GetSetting("rendering/lights_and_shadows/positional_shadow/atlas_size");

        // Viewports need to be registered in a list to apply this setting !
        // RenderingServer.ViewportSetScreenSpaceAA(rid, settings.graphics.AntialiasingQuality.ToSettingValue());
        // RenderingServer.ViewportSetMsaa3D(rid, settings.graphics.AntialiasingQuality == AntialiasingQuality.Disabled);
        // RenderingServer.ViewportSetUseDebanding(rid, settings.graphics.AntialiasingQuality != AntialiasingQuality.Disabled);
        //
        // RenderingServer.ViewportSetPositionalShadowAtlasSize(
        //     rid,
        //     positionalShadowAtlasSize,
        //     settings.graphics.SoftShadowQuality <= SoftShadowQuality.Low
        // );

        RenderingServer.DirectionalSoftShadowFilterSetQuality(settings.graphics.SoftShadowQuality.ToSettingValue());
        RenderingServer.PositionalSoftShadowFilterSetQuality(settings.graphics.SoftShadowQuality.ToSettingValue());
        RenderingServer.DirectionalShadowAtlasSetSize(
            (int)ProjectSettings.GetSetting("rendering/lights_and_shadows/directional_shadow/size"),
            settings.graphics.SoftShadowQuality <= SoftShadowQuality.Low
        );

        if(!Directory.Exists(string.Concat(PathManager.localePath, settings.currentLocale)))
            settings.currentLocale = "EnglishOfficial";

        GD.Print(string.Concat("Got locale '", PathManager.localePath, settings.currentLocale, "'"));
    }
}

public enum FullScreenMode : byte { Windowed, Fullscreen, Borderless }
public enum SSAOQuality : byte { VeryLow, Low, Medium, High, Ultra }
public enum SubsurfaceScatteringQuality : byte { Disabled, Low, Medium, High }
public enum AntialiasingQuality : byte { Disabled, x2, x4, x8 }
public enum SoftShadowQuality : byte { Disabled, VeryLow, Low, Medium, High, Ultra }

public static class SettingsHelper
{
    public static DisplayServer.WindowMode ToSettingValue(this FullScreenMode mode)
    {
        switch(mode)
        {
            case FullScreenMode.Windowed:   return DisplayServer.WindowMode.Windowed;
            case FullScreenMode.Fullscreen: return DisplayServer.WindowMode.Fullscreen;
            case FullScreenMode.Borderless: return DisplayServer.WindowMode.Fullscreen;
            default:                        return DisplayServer.WindowMode.Windowed;
        }
    }

    public static RenderingServer.EnvironmentSsaoQuality ToSettingValue(this SSAOQuality quality) => (int)quality > 5 ? 0 : (RenderingServer.EnvironmentSsaoQuality)quality;
    public static RenderingServer.SubSurfaceScatteringQuality ToSettingValue(this SubsurfaceScatteringQuality quality) => (int)quality > 4 ? 0 : (RenderingServer.SubSurfaceScatteringQuality)quality;
    public static int ToSettingValue(this AntialiasingQuality quality) => (int)quality > 4 ? 0 : (int)quality;
    public static RenderingServer.ShadowQuality ToSettingValue(this SoftShadowQuality quality) 
    {
        switch(quality)
        {
            case SoftShadowQuality.Disabled: return RenderingServer.ShadowQuality.Hard;
            case SoftShadowQuality.VeryLow:  return RenderingServer.ShadowQuality.SoftVeryLow;
            case SoftShadowQuality.Low:      return RenderingServer.ShadowQuality.SoftLow;
            case SoftShadowQuality.Medium:   return RenderingServer.ShadowQuality.SoftMedium;
            case SoftShadowQuality.High:     return RenderingServer.ShadowQuality.SoftHigh;
            case SoftShadowQuality.Ultra:    return RenderingServer.ShadowQuality.SoftUltra;
            default:                         return RenderingServer.ShadowQuality.Hard;
        }
    }
}

[Serializable]
public class Settings
{
    public static readonly Settings Default = new Settings();

    //Internal
    /// <summary>
    /// When the player first joins the game, they will be sent to the settings panel so they can choose everything direcly. 
    /// Once the player reached the main menu, we consider that they have done their choice and they will be sent to the main menu on startup instead.
    /// </summary>
    public bool playerReachedMainMenu = false;

    /*
	 *	Gameplay
	 */
    public bool tips = true;

    /*
	 *	Controls
	 */
    public float mouseXSensitivity = 1;
    public float mouseYSensitivity = 1;
    public bool invertX = false;
    public bool invertY = false;

    /*
	 *	Localisation
	 */
    /// <summary>The name of the directory where the localization files can be found. Meaning the community can do it's own translations.</summary>
    public string currentLocale = "EnglishOfficial";

    /*
	 *	Audio
	 */
    //General volume is 0 by default to avoid exploding the ears of players launching the game for the first time.
    public float general = 0;
    public float music = 1;
    public float environment = 1;
    public float effects = 1;
    public float ui = 1;

    /*
	 *	Graphics
	 */
    public GraphicSettings graphics = GraphicSettings.UltraPreset;

    public override string ToString()
    {
        return string.Concat(
            "playerReachedMainMenu: '", playerReachedMainMenu, "'\n",
            "mouseXSensitivity: '", mouseXSensitivity, "'\n",
            "mouseYSensitivity: '", mouseYSensitivity, "'\n",
            "invertX: '", invertX, "'\n",
            "invertY: '", invertY, "'\n",
            "currentLocale: '", currentLocale, "'\n",
            "music: '", music, "'\n",
            "environment: '", environment, "'\n",
            "effects: '", effects, "'\n",
            "ui: '", ui, "'\n",
            "graphics: {\n", graphics, "}\n"
            );
    }
}

[Serializable]
public class GraphicSettings
{
    public static readonly GraphicSettings LowPreset = new GraphicSettings() 
    { 
        FullScreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = false,
        maxFPS = 60,
        halfResolutionGI = true,
        SSAOQuality = SSAOQuality.VeryLow,
        glowQuality = false,
        volumetricFogQuality = false,
        SubsurfaceScatteringQuality = SubsurfaceScatteringQuality.Disabled,
        AntialiasingQuality = AntialiasingQuality.Disabled,
        SoftShadowQuality = SoftShadowQuality.Disabled
    };

    public static readonly GraphicSettings MediumPreset = new GraphicSettings() 
    { 
        FullScreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = true,
        maxFPS = 0,
        halfResolutionGI = false,
        SSAOQuality = SSAOQuality.Medium,
        glowQuality = true,
        volumetricFogQuality = true,
        SubsurfaceScatteringQuality = SubsurfaceScatteringQuality.Low,
        AntialiasingQuality = AntialiasingQuality.x4,
        SoftShadowQuality = SoftShadowQuality.High
    };

    public static readonly GraphicSettings UltraPreset = new GraphicSettings() 
    { 
        FullScreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = true,
        maxFPS = 0,
        halfResolutionGI = false,
        SSAOQuality = SSAOQuality.Ultra,
        glowQuality = true,
        volumetricFogQuality = true,
        SubsurfaceScatteringQuality = SubsurfaceScatteringQuality.High,
        AntialiasingQuality = AntialiasingQuality.x8,
        SoftShadowQuality = SoftShadowQuality.Ultra
    };

    public byte fullscreenMode = 0;
    public float gamma = 1;
    public bool vSync = false;
    public ushort maxFPS = 0;
    public bool halfResolutionGI = true;
    public byte ssaoQuality = 0;
    public bool glowQuality = false;
    public bool volumetricFogQuality = false;
    public byte subsurfaceScatteringQuality = 0;
    public byte antialiasingQuality = 0;
    public byte softShadowQuality = 0;

    [JsonIgnore] public FullScreenMode FullScreenMode 
    { get => (FullScreenMode)fullscreenMode; set => fullscreenMode = (byte)value; }
    [JsonIgnore] public SSAOQuality SSAOQuality 
    { get => (SSAOQuality)ssaoQuality; set => ssaoQuality = (byte)value; }
    [JsonIgnore] public SubsurfaceScatteringQuality SubsurfaceScatteringQuality 
    { get => (SubsurfaceScatteringQuality)subsurfaceScatteringQuality; set => subsurfaceScatteringQuality = (byte)value; }
    [JsonIgnore] public AntialiasingQuality AntialiasingQuality 
    { get => (AntialiasingQuality)antialiasingQuality; set => antialiasingQuality = (byte)value; }
    [JsonIgnore] public SoftShadowQuality SoftShadowQuality 
    { get => (SoftShadowQuality)softShadowQuality; set => softShadowQuality = (byte)value; }

    public override string ToString()
    {
        return string.Concat(
            "\tfullscreenMode: '", fullscreenMode, "'\n",
            "\tgamma: '", gamma, "'\n",
            "\tvSync: '", vSync, "'\n",
            "\tmaxFPS: '", maxFPS, "'\n",
            "\thalfResolutionGI: '", halfResolutionGI, "'\n",
            "\tssaoQuality: '", ssaoQuality, "'\n",
            "\tglowQuality: '", glowQuality, "'\n",
            "\tvolumetricFogQuality: '", volumetricFogQuality, "'\n",
            "\tsubsurfaceScatteringQuality: '", subsurfaceScatteringQuality, "'\n",
            "\tantialiasingQuality: '", antialiasingQuality, "'\n",
            "\tsoftShadowQuality: '", softShadowQuality, "'\n"
        );
    }
}