using BoneLib.BoneMenu;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: MelonInfo(typeof(HazmatsFB.Main), "Hazmats FB", "1.0.0", "Codex")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace HazmatsFB;

public sealed class Main : MelonMod
{
    private const int MinBrightness = 1;
    private const int MaxBrightness = 10;

    private MelonPreferences_Category _preferences;
    private MelonPreferences_Entry<bool> _enabledPreference;
    private MelonPreferences_Entry<int> _brightnessPreference;
    private MelonPreferences_Entry<bool> _removeFogPreference;

    private bool _enabled;
    private int _brightness = 1;
    private bool _removeFog;

    private bool _hasAmbientSnapshot;
    private AmbientMode _originalAmbientMode;
    private Color _originalAmbientLight;
    private AmbientMode _lastAppliedAmbientMode;
    private Color _lastAppliedAmbientLight;

    private bool _hasShadowSnapshot;
    private ShadowQuality _originalShadowQuality;

    private bool _hasFogSnapshot;
    private bool _originalFog;

    public override void OnInitializeMelon()
    {
        SetupPreferences();
        SetupBoneMenu();
        ApplySettings();

        LoggerInstance.Msg("Loaded. Rendering changes are local to this client.");
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // BONELAB and Fusion can load different scenes with different render settings.
        // Re-apply after scene initialization; the late update below also catches later
        // scene scripts that reset ambient light, shadows, or fog.
        ApplySettings();
    }

    public override void OnLateUpdate()
    {
        if (_enabled || _removeFog)
            ApplySettings();
    }

    public override void OnDeinitializeMelon()
    {
        RestoreAllCapturedSettings();
    }

    private void SetupPreferences()
    {
        _preferences = MelonPreferences.CreateCategory("HazmatsFB");
        _enabledPreference = _preferences.CreateEntry("Enabled", false);
        _brightnessPreference = _preferences.CreateEntry("Brightness", 1);
        _removeFogPreference = _preferences.CreateEntry("RemoveFog", false);

        _enabled = _enabledPreference.Value;
        _brightness = Mathf.Clamp(_brightnessPreference.Value, MinBrightness, MaxBrightness);
        _removeFog = _removeFogPreference.Value;
    }

    private void SetupBoneMenu()
    {
        Page page = Page.Root.CreatePage("Hazmats FB", Color.cyan, 0, true);

        page.CreateBool("Fullbright", Color.white, _enabled, value =>
        {
            _enabled = value;
            _enabledPreference.Value = value;
            SavePreferences();
            ApplySettings();
        });

        page.CreateInt("Brightness", Color.white, _brightness, 1, MinBrightness, MaxBrightness, value =>
        {
            _brightness = Mathf.Clamp(value, MinBrightness, MaxBrightness);
            _brightnessPreference.Value = _brightness;
            SavePreferences();
            ApplySettings();
        });

        page.CreateBool("Remove Fog (try)", Color.white, _removeFog, value =>
        {
            _removeFog = value;
            _removeFogPreference.Value = value;
            SavePreferences();
            ApplySettings();
        });
    }

    private void SavePreferences()
    {
        _preferences.SaveToFile(false);
    }

    private void ApplySettings()
    {
        if (_enabled)
        {
            CaptureAmbientAndShadowsIfNeeded();
            RefreshAmbientSnapshotIfGameChangedIt();

            RenderSettings.ambientMode = AmbientMode.Flat;
            _lastAppliedAmbientMode = AmbientMode.Flat;
            _lastAppliedAmbientLight = Color.white * _brightness;
            RenderSettings.ambientLight = _lastAppliedAmbientLight;

            // Keep the level's shadows from darkening the otherwise evenly lit scene.
            if (QualitySettings.shadows != ShadowQuality.Disable)
                _originalShadowQuality = QualitySettings.shadows;
            QualitySettings.shadows = ShadowQuality.Disable;
        }
        else
        {
            RestoreAmbientAndShadows();
        }

        if (_removeFog)
        {
            if (!_hasFogSnapshot)
            {
                _originalFog = RenderSettings.fog;
                _hasFogSnapshot = true;
            }
            else if (RenderSettings.fog)
            {
                // A scene script may have turned fog back on; remember that scene state
                // before turning it off again.
                _originalFog = true;
            }

            RenderSettings.fog = false;
        }
        else
        {
            RestoreFog();
        }
    }

    private void CaptureAmbientAndShadowsIfNeeded()
    {
        if (!_hasAmbientSnapshot)
        {
            _originalAmbientMode = RenderSettings.ambientMode;
            _originalAmbientLight = RenderSettings.ambientLight;
            _hasAmbientSnapshot = true;
        }

        if (!_hasShadowSnapshot)
        {
            _originalShadowQuality = QualitySettings.shadows;
            _hasShadowSnapshot = true;
        }
    }

    private void RefreshAmbientSnapshotIfGameChangedIt()
    {
        if (!_hasAmbientSnapshot)
            return;

        if (RenderSettings.ambientMode != _lastAppliedAmbientMode ||
            !Approximately(RenderSettings.ambientLight, _lastAppliedAmbientLight))
        {
            _originalAmbientMode = RenderSettings.ambientMode;
            _originalAmbientLight = RenderSettings.ambientLight;
        }
    }

    private void RestoreAmbientAndShadows()
    {
        if (_hasAmbientSnapshot)
        {
            RenderSettings.ambientMode = _originalAmbientMode;
            RenderSettings.ambientLight = _originalAmbientLight;
            _hasAmbientSnapshot = false;
        }

        if (_hasShadowSnapshot)
        {
            QualitySettings.shadows = _originalShadowQuality;
            _hasShadowSnapshot = false;
        }
    }

    private void RestoreFog()
    {
        if (!_hasFogSnapshot)
            return;

        RenderSettings.fog = _originalFog;
        _hasFogSnapshot = false;
    }

    private void RestoreAllCapturedSettings()
    {
        RestoreAmbientAndShadows();
        RestoreFog();
    }

    private static bool Approximately(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.001f &&
               Mathf.Abs(a.g - b.g) < 0.001f &&
               Mathf.Abs(a.b - b.b) < 0.001f &&
               Mathf.Abs(a.a - b.a) < 0.001f;
    }
}
