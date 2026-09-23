# Week 4 — Mystic Jungle Ruins

## Project kholna aur chalana

1. Unity 6000.5.9f1 mein `Assets/Week4/Scenes/Week4_Environment.unity` open karein.
2. Play dabayein. Main menu se **PLAY CHAPTER 01** select karein.
3. PC par WASD/arrow keys se move, Space se jump. Touch par joystick aur JUMP button.
4. Escape ya HUD ka II button pause karta hai. Resume, Restart aur Main Menu wired hain.
5. Path follow karein: jungle → fallen logs → bridge → river crossing → stairs → ruins → golden portal.
6. Settings mein Day, Sunset, Night aur automatic cycle demonstrate karein.

## Aapke purane kaam ka backup

- `Assets/Week4/Scenes/Week4_BeforeDemo.unity`: changes se pehle ka Week 4 scene snapshot.
- Updated scene ke andar `Original_Week4_Backup (inactive)`: purane objects retained hain.
- `Assets/Scenes/SampleScene.unity` aur `Assets/Scripts/` ke original Week 3 scripts is implementation ne edit nahin kiye.
- `SampleScene` se actual Player hierarchy, ninja model, Animator aur jump/collision/collect VFX copy kiye. Week 4 movement ke liye separate component use hua.

## Scene hierarchy

```text
Week4_Environment
├── Original_Week4_Backup (inactive)
└── Week4_Demo
    ├── Environment
    │   ├── Ground
    │   ├── Paths
    │   ├── Rocks
    │   ├── Trees
    │   ├── Ruins
    │   ├── Decorations
    │   └── Bridge
    ├── Gameplay
    │   ├── Ninja_Player
    │   ├── Collectibles
    │   ├── SpawnPoints / Start
    │   └── Final sanctuary trigger
    ├── Main Camera
    ├── Lighting
    │   ├── Sun_DayNight
    │   ├── Torch Point Light (6)
    │   ├── Baked Ambient Fill
    │   ├── Light Probes
    │   ├── Water Reflection Probe
    │   └── Sanctuary Spot Light
    ├── VFX
    ├── GameManager
    ├── UI
    └── EventSystem
```

## Har phase mein kya kiya

| Plan phase | Scene/object aur implementation | Aap kaise dekhein / samjhayen |
|---|---|---|
| 1. Setup | Existing Week4 scene mein organized `Week4_Demo` hierarchy aur original backup. Scene build list mein added. | Hierarchy expand karein. Week 3 alag retained hai. |
| 2. Ground | Do mesh banks, main path, raised overlook aur river ka lower area. Ground collision layer 6. | Ground aur Paths folders select karein. Mesh blocks modular placement ko simple rakhte hain. |
| 3. Modular design | `Generated/Modules` mein Road Piece, Road Corner, Rock Small/Large, Wall Straight/Corner/Broken, Pillar, Temple Floor reusable prefabs. | Prefab inspect karein; repeated road/pillar objects prefab instances hain. Corner/broken modules future layout edits ke liye library mein bhi hain. |
| 4. Jungle | Existing Trees Package Lite ke coconut trees/bushes reused; roadside rocks aur moss patches. | Trees outside route, bushes middle scale, moss smallest detail. Seed 41 se reproducible placement. |
| 5. Level route | Start z=2; jungle z=2–42; bridge z=43–57; ruins z=76–94. Side path x=6 aur overlook x=10. | Center route clear hai. Three fallen logs par jump karein; river mein girne se life kam hoti hai. |
| 6. Ruins | Terrace, four steps, pillars, capitals, walls, lintel, broken masonry aur golden goal portal. | `Environment/Ruins` expand karein. Final landmark z=92 hai. |
| 7. Water | Smooth metallic URP water material, river surface y=-1. Collider removed; bridge par chalna zaroori. | Ye stylized reflective surface hai, fluid simulation nahin. |
| 8. Basic lights | Real-time directional sun, six torch point lights, sanctuary spot, trilight ambient aur fog. | `Lighting` folder mein light types inspect karein. |
| 9. Day | Sun intensity/color, ambient aur fog ka daytime preset. | Settings → LIGHTING: DAY. |
| 10. Baked lights | Neutral low-intensity fill lights Baked; ground/ruins static geometry Contribute GI. Imported foliage light probes use karti hai kyun ke us mein valid lightmap UVs nahin. Dynamic sun bake mein include nahin. | Lighting window → generated Week4 lighting settings. Bake result ke liye verification report dekhein. |
| 11. Lightmap optimization | Resolution 8 texels/unit, atlas max 1024, per-renderer scale .35, CPU lightmapper. | Ye demo baseline hai; target-device profiling ka substitute nahin. |
| 12. Light probes | Route par 3 x lanes, 2 heights aur 7 m spacing; Ninja renderers Blend Probes. | Light Probes select karke yellow probe positions dekhein. Baked data moving Ninja par interpolate hota hai. |
| 13. Reflection probe | Water center (0,2,50), bounds 32×14×28, resolution128, box projection, time-sliced refresh. | `Water Reflection Probe` inspect karein; play mein 20 seconds per refresh. |
| 14. Dynamic lights | Sun aur torches real-time; static fill separately baked. | Night mein sun dim hota hai aur torches stronger. |
| 15. Torches | Six posts, point lights aur bounded flame particle systems. | Ruins ke dono sides z=78,87,93. Har fire max32 particles,22/sec emission. |
| 16. Day/night | `Week4Lighting.cs` cycle default180seconds; timeOfDay exposed. | Automatic cycle toggle ya manual presets. |
| 17. Ambient variation | Sky/equator/ground ambient, camera background aur fog colors time ke sath change. | Day, sunset aur night compare karein. |
| 18. Lighting optimization | Torch shadows disabled, sun shadow distance35, reflection128 and periodic, neutral bake. | Har point light ke expensive real-time shadows avoid kiye. |
| 19. Ninja | Actual Week3 player/model/Animator copy; Rigidbody + capsule retained, Week4 movement component attached. | `Ninja_Player` inspect karein. Original controller ko new scene copy par replace kiya. |
| 20. Collectibles | 29 rotating/bobbing gold coins; trigger → wallet increment → existing collect VFX → deactivate. | Restart par sab coins active ho jate hain. |
| 21. VFX | Week3 JumpVFX, CollisionVFX, CollectVFX copied; torch flames added. | VFX group aur player child JumpVFX inspect karein. |
| 22. Polish | Authored clear center route, seeded vegetation, moss, portal, carved UI sprite borders. | Imported low-poly assets ka look reference image ki painted illustration se different hai. |
| 23. Camera | Smooth third-person camera offset(0,5.5,-8), look-ahead3m, FOV60, far150. | `Week4Camera` LateUpdate mein follow karta hai. |
| 24. Optimization | Static batching flags, restricted particles, short shadow distance, probe refresh budget. | Mobile FPS/thermal measurement actual device par abhi separately karna hai. |
| 25. Testing | Editor Play Mode verification: coins, jump, pause, bridge, victory, restart, game over, menu screens. | `VERIFICATION.md` mein actual result aur limitations. |
| 26. Presentation | Saved scene, functional menus/HUD, this guide, script map aur verification report. | Main menu → gameplay → lighting presets → final portal ka demonstration dein. |

## Kis file mein kaunsa code hai

### `Assets/Week4/Scripts/Week4Player.cs`

- `Awake`: Rigidbody/Animator references cache.
- `FixedUpdate`: Input System keyboard aur existing joystick mein stronger input choose; velocity aur facing update; Animator `Speed` blend.
- Is tarah connected joystick keyboard ko zero input se overwrite nahin karta.
- `Jump`: ground raycast + cooldown, vertical velocity aur Animator `Jump` trigger, original jump particles.
- `OnCollisionEnter`: Obstacle tag par collision VFX aur one-life damage, cooldown prevents repeated hits.
- `Teleport`: restart/fall recovery par position aur velocity reset.
- `CanMove`: menu/pause/gameover ke waqt input disable.

### `Assets/Week4/Scripts/Week4Game.cs`

- `BuildUI`: scene mein editable Canvas, panels, labels, buttons, sliders, joystick create karta hai; editor builder/polish isay call karta hai.
- `Start`: saved controls par button listeners attach; PlayerPrefs settings load.
- `Show`: sirf requested screen active; pause/gameplay timescale aur movement control.
- `Act`: button name ke mutabiq action dispatch.
- `Begin`: score/lives reset, all coins reactivate, player spawn, selected speed.
- `Collect/Damage/Finish`: wallet, lives, gameover aur victory logic.
- Music/SFX sliders AudioSources change karte hain. Included sound is a generated ambient tone/chime, recorded jungle ambience nahin.
- Keys `W4.Wallet`, `W4.Mode`, `W4.Music`, `W4.SFX`, `W4.RewardDate`, `W4.Trail`, `W4.Best` local PlayerPrefs mein save hote hain.
- Daily reward UTC calendar date check karta hai. Ye local demo hai; server-verified rewards nahin.

### `Assets/Week4/Scripts/Week4Lighting.cs`

- `Update` cycle progress karta hai aur limited-frequency reflection refresh.
- `Apply` light intensity/color/rotation, fog aur ambient set karta hai.
- Perlin noise torch intensity ko subtle flicker deta hai.
- `SetPeriod` manual preset choose karke automatic cycle stop karta hai.
- Important: static baked daylight ko night par dark nahin kiya ja sakta; isliye sun real-time aur baked fill neutral/dim rakha hai.

### `Assets/Week4/Scripts/Week4Pickup.cs`

- Coin bob/rotate visual, player-only trigger, collect effect; coins destroy karne ki bajaye inactive hote hain taake restart reusable ho.
- Same component `finish=true` ke sath final trigger par victory kholta hai.

### `Assets/Week4/Scripts/Week4Camera.cs`

- `LateUpdate` mein SmoothDamp follow aur look-ahead. Unscaled time se paused/menu camera transition stable rehta hai.

### `Assets/Week4/Editor/Week4Builder.cs`

- One-time authored demo generator: backup, materials, modular prefabs, route, reused ninja/VFX, UI, audio, lights and probes.
- `Week 4 → Build Mystic Jungle Demo` menu. Existing `Week4_Demo` detect hone par duplicate build nahin karta.
- `CopyEffect` source scene VFX clone karta hai; source scene save nahin karta.
- `Tone` local WAV audio generate karta hai.
- Normal edits Inspector mein karein; har edit ke baad builder run karna zaroori nahin.

### `Assets/Week4/Editor/Week4Verification.cs`

- Play Mode smoke checks aur editor polish/bake helpers.
- `Week 4 → Polish Saved Demo` UI ko generated layout se rebuild karta hai. Agar aap UI manually customize kar chuke hain to is menu ko dobara run karne se pehle scene copy save karein.
- `FrameSprite` carved border/round control PNG sprite assets generate karta hai.
- `Temp/Week4*.request` flags sirf local development/test automation ke liye hain; player build mein Editor folder include nahin hota.

## UI ko Inspector mein edit karna

1. `Week4_Demo/UI/MainMenu/Content` kholein.
2. `Start/Label` ka TMP Text button label change karta hai.
3. Button Image color/sprite appearance control karta hai; RectTransform size/position layout control karta hai.
4. Runtime button dispatch object **name** se hota hai; `Start`, `Resume`, `Jump` jaise button names retain karein, displayed Label text freely change karein.
5. Settings sliders Music/SFX naam use karte hain. Canvas scaler reference1080×1920 aur height match use karta hai.
6. UI panels scene mein saved hain; runtime par buttons code se bind hote hain, isliye Inspector OnClick empty nazar aana expected hai.

## Reference image se deliberate differences

- Main menu, level selection, settings, HUD, pause, game over, reward, store, mode aur tutorial functional hain; victory added hai.
- 01 actual playable chapter hai; 02–09 future chapters hain, fake playable levels nahin.
- Store mein golden trail100 earned coins ka hai; reference ke dollar purchases/IAP backend implemented nahin.
- Same scene instant reset hota hai; fake loading percentage show nahin kiya.
- Reference ka hand-painted foliage/frame artwork supplied individual sprites nahin tha; scene uses existing low-poly assets aur generated beveled UI sprites. Exact pixel-for-pixel recreation claim nahin.
- Vibration, multiple skins/characters, seven-day reward streak, production purchase backend aur Android device profiling is version mein implemented/verified nahin.

## Senior ko explain karne ka short script

“Main ne existing Week3 Ninja aur VFX ko separate Week4 environment mein reuse kiya. Environment reusable road, wall aur pillar prefabs se bana hai. Player bridge cross karke ruins tak jata hai; coins, obstacles, pause/restart aur final trigger functional hain. Neutral static fill baked lighting ke liye hai, jabke sun aur torch lights real-time hain taake day/night transition mein baked daylight stuck na rahe. Light probes moving Ninja ko baked light dete hain aur water reflection probe environment capture karta hai. Performance ke liye torch shadows off, reflection resolution limited aur particle counts bounded hain.”
