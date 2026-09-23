# Week 4 verification

Environment: Unity 6000.5.9f1, existing URP project; Windows Editor, Android selected as project target.

## Completed checks

- Unity compiled and executed the new runtime/editor scripts. Existing API deprecation warnings are non-blocking; no unresolved C# compile error was present when the demo ran.
- The generated scene was saved at `Assets/Week4/Scenes/Week4_Environment.unity`.
- 18 automated Play Mode checks passed; raw results are in `PlayModeChecks.txt`.
- Checks cover: source ninja reference, start, standing on path, physical coin trigger/persistent wallet, physical jump, pause, resume, night sun intensity, bridge support, physical final trigger/victory, restoring29coins, three-hit game over, and six menu screens.
- A real mouse click on PLAY CHAPTER01 opened the gameplay HUD.
- Scene and Game views were visually inspected in portrait preview.
- CPU lighting bake completed successfully. Two directional lightmap atlases were generated, plus baked spherical-harmonic data for90light probes.
- Final bake used dim neutral ambient and baked fill lights. The daytime sun and torch lights remain real-time.
- Imported palm/bush meshes had unusable lightmap UVs. Their scene renderers now receive GI through Light Probes; original package meshes were not edited. Ground/ruins retain lightmaps.

## Evidence files

- `PlayModeChecks.txt`: raw PASS results.
- `HUD.png`: captured Unity gameplay preview.
- `MainMenu.png`: captured final menu preview, when present.
- `Assets/Week4/Scenes/Week4_Environment/Lightmap-0_comp_light.exr`
- `Assets/Week4/Scenes/Week4_Environment/Lightmap-1_comp_light.exr`
- Matching `_comp_dir.png` lightmap textures and `LightingData.asset`.

## Practical limits

- Play Mode smoke checks use teleports to exercise physical checkpoints; they are not an automated uninterrupted traversal of the whole route.
- Touch joystick/button components are integrated, but actual Android hardware, multi-touch, notch safe areas, FPS, thermals and APK installation were not tested.
- No production IAP, account service, anti-cheat or server clock exists. Rewards/store are local PlayerPrefs features using earned coins.
- Levels2–9 are intentionally locked future content. Only Chapter01 has a playable route.
- The provided reference is painted artwork. This implementation uses the project's existing low-poly foliage/ninja and generated UI textures, not an exact reproduction of the illustration.
- Reflection probe configuration is implemented and rendering runs; no planar-water simulation or quantitative reflection-quality test is claimed.
- Static batching/probe/particle budgets are configured. No measured target-device performance result or occlusion bake is claimed.

## Repeat manually

1. Open the saved Week4 scene and press Play.
2. Start, move with WASD/arrows, collect coins, and jump with Space.
3. Drag the joystick and press JUMP in Game view; then test both together on an Android device.
4. Pause/resume, restart, and verify all coins return.
5. Lose three lives against logs/falls and verify Game Over.
6. Cross the bridge, climb the stairs and enter the gold portal; verify victory.
7. Compare Day/Sunset/Night in Settings; toggle shadow button and automatic cycle.
8. Change audio sliders; claim the daily reward twice (second claim must not add coins); buy the100-coin trail and restart to confirm it remains equipped.
9. Before grading mobile performance, create an Android development build and inspect the Unity Profiler on the actual target device.
