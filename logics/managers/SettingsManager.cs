using Godot;
using System;

public partial class SettingsManager : Manager<SettingsManager>
{
    [Export] private Settings settings;

    public override void Ready()
    {
        
    }

    public override void AllManagersReady()
    {

    }

    public static void ApplySettings(Settings settings)
    {
        ProjectSettings.SetSetting("display/window/size/mode", settings.graphics.fullscreenMode.ToSettingValue());
        ProjectSettings.SetSetting("display/window/size/borderless", settings.graphics.fullscreenMode == FullScreenMode.Borderless);

        ProjectSettings.SetSetting("display/window/vsync/vsync_mode", settings.graphics.vSync);
        ProjectSettings.SetSetting("application/run/max_fps", settings.graphics.maxFPS);

        ProjectSettings.SetSetting("rendering/textures/default_filters/anisotropic_filtering_level", settings.graphics.anisotropicFiltering.ToSettingValue());

        ProjectSettings.SetSetting("rendering/global_illumination/gi/use_half_resolution", settings.graphics.halfResolutionGI);

        ProjectSettings.SetSetting("rendering/environment/ssao/quality", settings.graphics.ssaoQuality.ToSettingValue());

        ProjectSettings.SetSetting("rendering/environment/ssao/quality", settings.graphics.ssaoQuality.ToSettingValue());

        ProjectSettings.SetSetting("rendering/environment/glow/upscale_mode", settings.graphics.glowQuality ? 1 : 0);

        ProjectSettings.SetSetting("rendering/environment/volumetric_fog/use_filter", settings.graphics.volumetricFogQuality ? 1 : 0);

        ProjectSettings.SetSetting("rendering/environment/subsurface_scattering/subsurface_scattering_quality", settings.graphics.subsurfaceScatteringQuality.ToSettingValue());

        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", settings.graphics.antialiasingQuality.ToSettingValue());
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/screen_space_aa", settings.graphics.antialiasingQuality == AntialiasingQuality.Disabled ? 0 : 1);

        ProjectSettings.SetSetting("rendering/lights_and_shadows/directional_shadow/soft_shadow_filter_quality", settings.graphics.softShadowQuality.ToSettingValue());
        ProjectSettings.SetSetting("rendering/lights_and_shadows/positional_shadow/soft_shadow_filter_quality", settings.graphics.softShadowQuality.ToSettingValue());

        ProjectSettings.SetSetting("rendering/shading/overrides/force_lambert_over_burley", settings.graphics.fastShading);
        // Maybe ?
        //ProjectSettings.SetSetting("rendering/shading/overrides/force_vertex_shading", settings.graphics.fastShading);
    }
}

public enum FullScreenMode : byte { Windowed, Fullscreen, Borderless }
public enum AnisotropicFiltering : byte { Disabled, x2, x4, x8, x16 }
public enum SSAOQuality : byte { VeryLow, Low, Medium, High, Ultra }
public enum SubsurfaceScatteringQuality : byte { Disabled, Low, Medium, High }
public enum AntialiasingQuality : byte { Disabled, x2, x4, x8 }
public enum SoftShadowQuality : byte { Disabled, VeryLow, Low, Medium, High, Ultra }

public static class SettingsHelper
{
    public static int ToSettingValue(this FullScreenMode mode)
    {
        switch(mode)
        {
            case FullScreenMode.Windowed:
                return 0;
            case FullScreenMode.Fullscreen:
                return 3;
            case FullScreenMode.Borderless:
                return 3;
            default:
                return 0;
        }
    }

    public static int ToSettingValue(this AnisotropicFiltering filter) => (int)filter > 4 ? 0 : (int)filter;
    public static int ToSettingValue(this SSAOQuality quality) => (int)quality > 5 ? 0 : (int)quality;
    public static int ToSettingValue(this SubsurfaceScatteringQuality quality) => (int)quality > 4 ? 0 : (int)quality;
    public static int ToSettingValue(this AntialiasingQuality quality) => (int)quality > 4 ? 0 : (int)quality;
    public static int ToSettingValue(this SoftShadowQuality quality) => (int)quality > 6 ? 0 : (int)quality;
}

public class Settings
{
    //Internal
    /// <summary>
    /// When the player first joins the game, they will be sent to the settings panel so they can choose everything direcly. 
    /// Once the player reached the main menu, we consider that they have done their choice and they will be sent to the main menu on startup instead.
    /// </summary>
    public bool playerReachedMainMenu = false;

    [ExportGroup("Gameplay")]
    [Export] public bool tips = true;

    [ExportGroup("Controls")]
    [Export] public float mouseXSensitivity = 1;
    [Export] public float mouseYSensitivity = 1;
    [Export] public bool invertX = false;
    [Export] public bool invertY = false;

    [ExportGroup("Localisation")]
    /// <summary>The name of the directory where the localization files can be found. Meaning the community can do it's own translations.</summary>
    [Export] public string currentLocal = "EnglishOfficial";

    [ExportGroup("Audio")]
    //General volume is 0 by default to avoid exploding the ears of players launching the game for the first time.
    [Export] public float general = 0;
    [Export] public float music = 1;
    [Export] public float environment = 1;
    [Export] public float effects = 1;
    [Export] public float ui = 1;

    [ExportGroup("Graphics")]
    [Export] public GraphicSettings graphics;
}

public class GraphicSettings
{
    public static readonly GraphicSettings LowPreset = new GraphicSettings() 
    { 
        fullscreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = false,
        maxFPS = 60,
        anisotropicFiltering = AnisotropicFiltering.Disabled,
        halfResolutionGI = true,
        ssaoQuality = SSAOQuality.VeryLow,
        glowQuality = false,
        volumetricFogQuality = false,
        subsurfaceScatteringQuality = SubsurfaceScatteringQuality.Disabled,
        antialiasingQuality = AntialiasingQuality.Disabled,
        softShadowQuality = SoftShadowQuality.Disabled,
        fastShading = true
    };

    public static readonly GraphicSettings MediumPreset = new GraphicSettings() 
    { 
        fullscreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = true,
        maxFPS = 0,
        anisotropicFiltering = AnisotropicFiltering.x4,
        halfResolutionGI = false,
        ssaoQuality = SSAOQuality.Medium,
        glowQuality = true,
        volumetricFogQuality = true,
        subsurfaceScatteringQuality = SubsurfaceScatteringQuality.Low,
        antialiasingQuality = AntialiasingQuality.x4,
        softShadowQuality = SoftShadowQuality.High,
        fastShading = false
    };

    public static readonly GraphicSettings UltraPreset = new GraphicSettings() 
    { 
        fullscreenMode = FullScreenMode.Windowed,
        gamma = 1,
        vSync = true,
        maxFPS = 0,
        anisotropicFiltering = AnisotropicFiltering.x16,
        halfResolutionGI = false,
        ssaoQuality = SSAOQuality.Ultra,
        glowQuality = true,
        volumetricFogQuality = true,
        subsurfaceScatteringQuality = SubsurfaceScatteringQuality.High,
        antialiasingQuality = AntialiasingQuality.x8,
        softShadowQuality = SoftShadowQuality.Ultra,
        fastShading = false
    };

    [Export] public FullScreenMode fullscreenMode = FullScreenMode.Windowed;
    [Export] public float gamma = 1;
    [Export] public bool vSync = false;
    [Export] public ushort maxFPS = 0;
    [Export] public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disabled;
    [Export] public bool halfResolutionGI = true;
    [Export] public SSAOQuality ssaoQuality = SSAOQuality.VeryLow;
    [Export] public bool glowQuality = false;
    [Export] public bool volumetricFogQuality = false;
    [Export] public SubsurfaceScatteringQuality subsurfaceScatteringQuality = SubsurfaceScatteringQuality.Disabled;
    [Export] public AntialiasingQuality antialiasingQuality = AntialiasingQuality.Disabled;
    [Export] public SoftShadowQuality softShadowQuality = SoftShadowQuality.Disabled;
    [Export] public bool fastShading = true;
}