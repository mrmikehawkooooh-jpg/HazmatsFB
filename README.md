# Hazmats FB for standalone Quest

A BONELAB code mod intended to run on a standalone Meta Quest through LemonLoader. It has no PCVR-only APIs or Fusion integration.

- BoneMenu switch for fullbright.
- Brightness from 1 to 10. Level 1 is flat white ambient lighting; higher values multiply that light up to 10x.
- Fullbright disables Unity's global shadow quality while active.
- Optional best-effort fog removal.
- No Fusion hooks or network messages. Other players do not receive these visual changes.

BoneLib is required for BoneMenu. The mod has no Fusion dependency. It does not use asset bundles or desktop-native libraries.

## Build

The project includes a GitHub Actions workflow that builds the mod on GitHub. It downloads the BoneLib compile reference during the run and restores the MelonLoader and Unity references from NuGet, so your Mac does not need the .NET SDK or BONELAB's assemblies. `NuGet.Config` includes both nuget.org and the BepInEx Unity package feed required for the matching Unity 2021.3.16 assemblies.

1. Put the contents of this folder at the root of a GitHub repository.
2. Push the repository to GitHub.
3. Open the repository's **Actions** tab and select **Build Hazmats FB**. It also runs automatically on pushes and pull requests.
4. Open a successful run and download the `HazmatsFB-Quest` artifact.

The workflow targets .NET 6 for LemonLoader compatibility. It pins BoneLib 3.2.2 for its BoneMenu compile API. If that package version changes, update the download URL in `.github/workflows/build.yml`.

## Install on Quest

Install `bin/Release/net6.0/HazmatsFB.dll` and BoneLib's `BoneLib.dll` under LemonLoader's `Mods` directory on the Quest. For the LemonLoader 0.6.5 line, that directory is `/sdcard/MelonLoader/com.StressLevelZero.BONELAB/Mods`. Follow LemonLoader's current install guide if your loader version uses a different data location.

## Notes

Ambient lighting is a game-wide Unity setting, so shaders that ignore ambient light may not brighten. Fog removal only switches off Unity's built-in `RenderSettings.fog`; a level or shader that draws fog another way may keep showing it. Level scripts that change these settings are countered while the mod is enabled, and their new ambient/shadow/fog values are remembered for restoration where possible.
