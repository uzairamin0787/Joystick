using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace MysticJungle.Editor
{
    [InitializeOnLoad]
    public static class Week4RunnerSetup
    {
        static Week4RunnerSetup() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying) return;
            if (!File.Exists("Temp/Week4RunnerSetup.request")) return;
            File.Delete("Temp/Week4RunnerSetup.request");
            try { Prepare(); File.WriteAllText("Temp/Week4RunnerSetup.done", "Runner scene and mixed lighting prepared"); }
            catch (Exception e) { File.WriteAllText("Temp/Week4RunnerSetup.error", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("Week 4/Prepare Continuous Runner")]
        public static void Prepare()
        {
            var game = UnityEngine.Object.FindAnyObjectByType<Week4Game>();
            if (!game) throw new InvalidOperationException("Open Week4_Environment first");
            game.ConfigureRunnerUI();
            var environment = game.transform.parent.Find("Environment");
            var paths = environment.Find("Paths");
            foreach (Transform part in environment.GetComponentsInChildren<Transform>(true))
                if (part.name == "Start boundary" || part.name == "Level boundary" || part.name == "Sanctuary rear wall")
                    part.gameObject.SetActive(false);
            if (!paths.Find("Seam path connector"))
            {
                var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                road.name = "Seam path connector"; road.layer = 6;
                road.transform.SetParent(paths, false);
                road.transform.localPosition = new Vector3(0, 0, 101.5f);
                road.transform.localScale = new Vector3(6, .2f, 7);
                road.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Week4/Generated/Sandstone Path.mat");
            }
            foreach (var renderer in environment.GetComponentsInChildren<MeshRenderer>(true))
            {
                var filter = renderer.GetComponent<MeshFilter>();
                bool foliage = filter && filter.sharedMesh && AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith("Assets/Trees Package Lite/");
                bool dynamic = foliage || renderer.name.Contains("River") || renderer.GetComponent<ParticleSystemRenderer>();
                // Static batching and baked occlusion contain absolute positions: do not use
                // them on recycled chunks. Lightmap UVs remain reusable at every position.
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, dynamic ? 0 : StaticEditorFlags.ContributeGI);
                renderer.receiveGI = dynamic ? ReceiveGI.LightProbes : ReceiveGI.Lightmaps;
                renderer.lightProbeUsage = LightProbeUsage.Off;
            }
            foreach (var renderer in game.player.GetComponentsInChildren<Renderer>())
            {
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
            }
            var lighting = game.lighting;
            lighting.automatic = false; lighting.SetPeriod(0);
            lighting.sun.lightmapBakeType = LightmapBakeType.Mixed;
            lighting.sun.bounceIntensity = 0; // Neutral baked fill, so night is not baked daylight.
            lighting.sun.shadows = LightShadows.Soft;
            foreach (var light in lighting.GetComponentsInChildren<Light>())
            {
                if (light == lighting.sun) continue;
                if (light.name == "Baked Ambient Fill") { light.lightmapBakeType = LightmapBakeType.Baked; light.intensity = .5f; }
                else light.lightmapBakeType = LightmapBakeType.Realtime;
            }
            var settings = Lightmapping.lightingSettings;
            settings.bakedGI = true; settings.realtimeGI = false;
            settings.mixedBakeMode = MixedLightingMode.IndirectOnly;
            settings.ao = true; settings.aoMaxDistance = 1.25f;
            settings.lightmapResolution = 8; settings.lightmapMaxSize = 1024;
            settings.directSampleCount = 32; settings.indirectSampleCount = 128; settings.environmentSampleCount = 64;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.055f, .065f, .065f);
            QualitySettings.shadowDistance = 55;
            EditorUtility.SetDirty(settings);
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            Lightmapping.BakeAsync();
        }
    }
}
