using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

namespace MysticJungle.Editor
{
    [InitializeOnLoad]
    public static class Week4Builder
    {
        const string ScenePath="Assets/Week4/Scenes/Week4_Environment.unity";
        const string Generated="Assets/Week4/Generated";
        static Material stone,path,earth,wood,gold,water,leaf;
        static Transform env;
        static Week4Builder() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (File.Exists("Temp/Week4Build.request") && !EditorApplication.isPlaying)
            { File.Delete("Temp/Week4Build.request"); try { Build(); } catch(Exception e) { Debug.LogException(e); File.WriteAllText("Temp/Week4Build.error",e.ToString()); } }
        }
        [MenuItem("Week 4/Build Mystic Jungle Demo")]
        public static void Build()
        {
            Directory.CreateDirectory(Generated); Directory.CreateDirectory(Generated+"/Modules");
            Scene scene=SceneManager.GetActiveScene();
            if(scene.path!=ScenePath) scene=EditorSceneManager.OpenScene(ScenePath);
            if(GameObject.Find("Week4_Demo")) { Debug.LogWarning("Week4 demo already exists. Restore the BeforeDemo backup to regenerate without duplicates.");return; }
            EditorSceneManager.SaveScene(scene,"Assets/Week4/Scenes/Week4_BeforeDemo.unity",true);
            var originals=scene.GetRootGameObjects();var legacy=new GameObject("Original_Week4_Backup (inactive)");
            foreach(var original in originals) original.transform.SetParent(legacy.transform,true); legacy.SetActive(false);
            var root=new GameObject("Week4_Demo");
            env=Group("Environment",root.transform);
            stone=Mat("Weathered Stone",new Color(.34f,.38f,.28f));path=Mat("Sandstone Path",new Color(.52f,.37f,.18f));earth=Mat("Jungle Soil",new Color(.13f,.22f,.10f));wood=Mat("Ancient Timber",new Color(.23f,.12f,.055f));gold=Mat("Relic Gold",new Color(1,.66f,.08f),.65f,.65f);water=Mat("River Water",new Color(.025f,.38f,.42f),.7f,.92f);leaf=Mat("Moss",new Color(.17f,.30f,.065f));
            var ground=Group("Ground",env);var paths=Group("Paths",env);var ruins=Group("Ruins",env);var trees=Group("Trees",env);var rocks=Group("Rocks",env);var details=Group("Decorations",env);
            Module("Road Piece",new Vector3(6,.2f,6),path);Module("Wall Straight",new Vector3(4,3,.65f),stone);Module("Pillar",new Vector3(1,4,1),stone);Module("Temple Floor",new Vector3(6,.4f,6),stone);Module("Rock Small",new Vector3(1,.8f,1),stone,PrimitiveType.Sphere);Module("Rock Large",new Vector3(2.5f,2,2),stone,PrimitiveType.Sphere);Module("Road Corner",new Vector3(6,.2f,6),path);Module("Wall Corner",new Vector3(1.3f,3,1.3f),stone);Module("Wall Broken",new Vector3(3,1.3f,.7f),stone);
            Cube("Start and jungle bank",ground,new Vector3(0,-.6f,20),new Vector3(32,1,48),earth);
            Cube("Ruins bank",ground,new Vector3(0,-.6f,79),new Vector3(32,1,46),earth);
            for(int z=2;z<42;z+=6) Place("Road Piece",paths,new Vector3(0,0,z));
            for(int z=59;z<99;z+=6) Place("Road Piece",paths,new Vector3(0,0,z));
            for(int z=20;z<38;z+=6) Place("Road Corner",paths,new Vector3(6,0,z));
            Cube("Optional raised overlook",ground,new Vector3(10,.25f,33),new Vector3(5,.7f,8),stone);
            Cube("Overlook step",ground,new Vector3(7,.1f,30),new Vector3(1,.3f,3),stone);
            var river=Cube("Reflective River",ground,new Vector3(0,-1,50),new Vector3(32,.18f,12),water); Object.DestroyImmediate(river.GetComponent<Collider>());
            var bridge=Group("Bridge",env);
            for(int i=0;i<22;i++) Cube("Bridge plank "+i,bridge,new Vector3(0,.02f,43+i*.62f),new Vector3(3.7f,.22f,.56f),wood);
            for(int side=-1;side<=1;side+=2)
            { Cube("Handrail",bridge,new Vector3(side*1.85f,1,49.5f),new Vector3(.15f,.15f,14),wood);for(int z=43;z<=57;z+=2)Cube("Bridge post",bridge,new Vector3(side*1.85f,.5f,z),new Vector3(.18f,1.2f,.18f),wood); }
            for(int i=0;i<4;i++) Cube("Temple stair "+i,ruins,new Vector3(0,i*.16f,76+i*.8f),new Vector3(7,.32f,.85f),stone);
            Cube("Temple terrace",ruins,new Vector3(0,.22f,86),new Vector3(15,1,15),stone);
            for(int side=-1;side<=1;side+=2)
            { for(int z=80;z<=92;z+=4) { var pillar=Place("Pillar",ruins,new Vector3(side*5,2.7f,z)); Cube("Pillar capital",pillar.transform,new Vector3(side*5,4.85f,z),new Vector3(1.5f,.35f,1.5f),stone); } for(int z=83;z<95;z+=4) {var wall=Place("Wall Straight",ruins,new Vector3(side*7,2.1f,z));wall.transform.rotation=Quaternion.Euler(0,90,0);} }
            Cube("Temple entrance lintel",ruins,new Vector3(0,5,80),new Vector3(11,.8f,1.3f),stone);
            Cube("Sanctuary rear wall",ruins,new Vector3(0,2.5f,94),new Vector3(15,4,.8f),stone);
            for(int i=0;i<7;i++) Cube("Broken masonry",ruins,new Vector3(8+i%2,.4f,73+i*2),new Vector3(1.4f,.8f,1),stone).transform.rotation=Quaternion.Euler(0,i*31,0);
            // Composition is deterministic: tall canopy outside the route, medium rocks, small bushes.
            var random=new System.Random(41);
            for(int side=-1;side<=1;side+=2) for(int z=0;z<100;z+=5)
            {
                if(z>42&&z<59)continue;
                float x=side*(7+(float)random.NextDouble()*6);
                Vegetation("coconut_tree_2",trees,new Vector3(x,0,z),1.2f+(float)random.NextDouble()*.5f,random.Next(360));
                Vegetation("bush_01",details,new Vector3(side*(4+(float)random.NextDouble()*2),0,z+1),.75f,random.Next(360));
                var rock=Place(z%2==0?"Rock Large":"Rock Small",rocks,new Vector3(side*5,.25f,z+2));rock.transform.rotation=Quaternion.Euler(0,random.Next(360),15);
                Cube("Moss patch",details,new Vector3(side*4.5f,.08f,z+3),new Vector3(1.5f,.1f,.8f),leaf);
            }
            foreach(int z in new[]{17,33,67}) {var log=Cube("Fallen log obstacle",details,new Vector3(z==33?4:0,.42f,z),new Vector3(3,.75f,.8f),wood);log.tag="Obstacle";}
            // Invisible side bounds prevent leaving the composed level; the river remains a falling hazard.
            foreach(int side in new[]{-1,1}) {var bound=Cube("Level boundary",ground,new Vector3(side*15,2,49),new Vector3(.4f,5,106),earth);bound.GetComponent<Renderer>().enabled=false;}
            var back=Cube("Start boundary",ground,new Vector3(0,2,-4),new Vector3(30,5,.4f),earth);back.GetComponent<Renderer>().enabled=false;
            var gameplay=Group("Gameplay",root.transform);var spawnPoints=Group("SpawnPoints",gameplay);Group("Start",spawnPoints).position=new Vector3(0,.15f,2);
            var source=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity",OpenSceneMode.Additive);
            var oldPlayer=source.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PlayerController>(true)).First();
            var clone=Object.Instantiate(oldPlayer.gameObject); clone.name="Ninja_Player"; SceneManager.MoveGameObjectToScene(clone,scene);clone.transform.SetParent(gameplay);clone.transform.position=new Vector3(0,.15f,2);
            var oldController=clone.GetComponent<PlayerController>();var player=clone.AddComponent<Week4Player>();
            var vfx=Group("VFX",root.transform);
            player.jumpVFX=CopyEffect(oldPlayer.jumpParticles,"JumpVFX",player.transform,scene);
            player.collisionVFX=CopyEffect(oldPlayer.collisionParticles,"CollisionVFX",vfx,scene);
            var originalPickup=source.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collectible>(true)).FirstOrDefault();
            ParticleSystem collection=originalPickup?CopyEffect(originalPickup.collectParticles,"CollectVFX",vfx,scene):null;
            Object.DestroyImmediate(oldController);EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);
            var rb=clone.GetComponent<Rigidbody>();rb.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.Continuous;
            foreach(var renderer in clone.GetComponentsInChildren<Renderer>()) renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
            var trail=clone.AddComponent<TrailRenderer>();trail.sharedMaterial=gold;trail.time=.45f;trail.startWidth=.14f;trail.endWidth=0;trail.emitting=false;
            var collectibles=Group("Collectibles",gameplay);
            for(int z=7;z<94;z+=3) {float y=z>=79?1.5f:.9f;var coin=Primitive("Relic Coin",collectibles,new Vector3(0,y,z),new Vector3(.48f,.09f,.48f),gold,PrimitiveType.Cylinder);coin.transform.rotation=Quaternion.Euler(90,0,0);coin.GetComponent<Collider>().isTrigger=true;var pickup=coin.AddComponent<Week4Pickup>();pickup.effect=collection;coin.isStatic=false;}
            var goal=Cube("Final sanctuary trigger",gameplay,new Vector3(0,2,92),new Vector3(5,3,1),gold);goal.GetComponent<Renderer>().enabled=false;goal.GetComponent<Collider>().isTrigger=true;goal.AddComponent<Week4Pickup>().finish=true;goal.isStatic=false;
            for(int side=-1;side<=1;side+=2) Cube("Golden portal pillar",ruins,new Vector3(side*2.5f,2.2f,92),new Vector3(.35f,3,.35f),gold);
            Cube("Golden portal arch",ruins,new Vector3(0,3.7f,92),new Vector3(5.35f,.35f,.35f),gold);
            var cameraGO=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(Week4Camera));cameraGO.tag="MainCamera";cameraGO.transform.SetParent(root.transform);cameraGO.transform.position=new Vector3(0,5.65f,-6);cameraGO.GetComponent<Week4Camera>().player=clone.transform;var camera=cameraGO.GetComponent<Camera>();camera.fieldOfView=60;camera.farClipPlane=150;camera.clearFlags=CameraClearFlags.SolidColor;
            var lightingRoot=Group("Lighting",root.transform);var dayNight=lightingRoot.gameObject.AddComponent<Week4Lighting>();
            var sunGO=new GameObject("Sun_DayNight",typeof(Light));sunGO.transform.SetParent(lightingRoot);dayNight.sun=sunGO.GetComponent<Light>();dayNight.sun.type=LightType.Directional;dayNight.sun.shadows=LightShadows.Soft;dayNight.sun.lightmapBakeType=LightmapBakeType.Realtime;RenderSettings.sun=dayNight.sun;
            var torches=new List<Light>();foreach(int z in new[]{78,87,93})foreach(int side in new[]{-1,1})
            {
                var torch=Cube("Torch",ruins,new Vector3(side*4,1.5f,z),new Vector3(.3f,2,.3f),wood);
                var lightGO=new GameObject("Torch Point Light",typeof(Light));lightGO.transform.SetParent(lightingRoot);lightGO.transform.position=new Vector3(side*4,2.8f,z);var light=lightGO.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.42f,.08f);light.range=7;light.shadows=LightShadows.None;light.lightmapBakeType=LightmapBakeType.Realtime;torches.Add(light);
                var fire=new GameObject("Flame",typeof(ParticleSystem));fire.transform.SetParent(torch.transform,true);fire.transform.position=lightGO.transform.position;var ps=fire.GetComponent<ParticleSystem>();var main=ps.main;main.startLifetime=.5f;main.startSpeed=.6f;main.startSize=.3f;main.startColor=new Color(1,.42f,.03f);main.maxParticles=32;var emission=ps.emission;emission.rateOverTime=22;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.09f;shape.angle=10;fire.transform.rotation=Quaternion.Euler(-90,0,0);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=ParticleMaterial();
            }
            dayNight.torches=torches.ToArray();
            var probeGO=new GameObject("Water Reflection Probe",typeof(ReflectionProbe));probeGO.transform.SetParent(lightingRoot);probeGO.transform.position=new Vector3(0,2,50);var probe=probeGO.GetComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=128;probe.size=new Vector3(32,14,28);probe.boxProjection=true;dayNight.waterProbe=probe;
            var probes=new GameObject("Light Probes",typeof(LightProbeGroup));probes.transform.SetParent(lightingRoot);var positions=new List<Vector3>();for(int z=0;z<100;z+=7)foreach(float x in new[]{-4f,0f,4f})foreach(float y in new[]{.5f,3f})positions.Add(new Vector3(x,y,z));probes.GetComponent<LightProbeGroup>().probePositions=positions.ToArray();
            // Neutral indirect fill is baked; sun/torches stay real-time so night does not retain baked daylight.
            for(int z=8;z<98;z+=18) {var go=new GameObject("Baked Ambient Fill",typeof(Light));go.transform.SetParent(lightingRoot);go.transform.position=new Vector3(0,6,z);var l=go.GetComponent<Light>();l.type=LightType.Point;l.intensity=.35f;l.range=15;l.color=new Color(.42f,.53f,.46f);l.lightmapBakeType=LightmapBakeType.Baked;}
            var spotGO=new GameObject("Sanctuary Spot Light",typeof(Light));spotGO.transform.SetParent(lightingRoot);spotGO.transform.position=new Vector3(0,7,89);spotGO.transform.rotation=Quaternion.Euler(65,0,0);var spot=spotGO.GetComponent<Light>();spot.type=LightType.Spot;spot.range=12;spot.spotAngle=65;spot.intensity=2;spot.color=new Color(1,.74f,.3f);spot.shadows=LightShadows.None;
            var game=new GameObject("GameManager",typeof(Week4Game)).GetComponent<Week4Game>();game.transform.SetParent(root.transform);game.player=player;game.lighting=dayNight;game.music=game.gameObject.AddComponent<AudioSource>();game.sfx=game.gameObject.AddComponent<AudioSource>();game.music.loop=true;game.music.clip=Tone("JungleAmbient",true);game.music.playOnAwake=true;game.collectSound=Tone("CollectChime",false);game.BuildUI();game.ui.SetParent(root.transform,true);
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)).transform.SetParent(root.transform);
            dayNight.Apply();QualitySettings.shadowDistance=35;
            var settings=new LightingSettings();settings.name="Week4 Balanced Bake";settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapResolution=8;settings.lightmapMaxSize=1024;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;AssetDatabase.CreateAsset(settings,Generated+"/Week4Lighting.lighting");Lightmapping.lightingSettings=settings;
            foreach(var renderer in env.GetComponentsInChildren<MeshRenderer>()) {if(renderer.gameObject.name.Contains("River"))continue;GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);var serialized=new SerializedObject(renderer);var scale=serialized.FindProperty("m_ScaleInLightmap");if(scale!=null){scale.floatValue=.35f;serialized.ApplyModifiedPropertiesWithoutUndo();}}
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/Week4Build.done","Scene built: "+ScenePath+"\nCoins: "+Object.FindObjectsByType<Week4Pickup>(FindObjectsSortMode.None).Length+"\nNinja: "+player.name);
            Debug.Log("WEEK4_BUILD_OK: scene, ninja, route, UI, probes and lighting generated.");
        }
        static Transform Group(string name,Transform parent) {var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        static Material Mat(string name,Color color,float metallic=0,float smooth=.15f) {var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);AssetDatabase.CreateAsset(m,Generated+"/"+name+".mat");return m;}
        static Material ParticleMaterial() {string p=Generated+"/Flame.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m)return m;m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));m.SetColor("_BaseColor",new Color(1,.55f,.05f));AssetDatabase.CreateAsset(m,p);return m;}
        static GameObject Primitive(string name,Transform parent,Vector3 pos,Vector3 scale,Material material,PrimitiveType type) {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,true);go.transform.position=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;go.layer=6;return go;}
        static GameObject Cube(string n,Transform p,Vector3 v,Vector3 s,Material m)=>Primitive(n,p,v,s,m,PrimitiveType.Cube);
        static void Module(string name,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube) {var go=Primitive(name,null,Vector3.zero,scale,material,type);PrefabUtility.SaveAsPrefabAsset(go,Generated+"/Modules/"+name+".prefab");Object.DestroyImmediate(go);}
        static GameObject Place(string name,Transform parent,Vector3 position) {var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Generated+"/Modules/"+name+".prefab"));go.transform.SetParent(parent,true);go.transform.position=position;return go;}
        static void Vegetation(string name,Transform parent,Vector3 position,float scale,float angle) {var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Trees Package Lite/Prefabs/"+name+".prefab");if(!prefab)return;var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent,true);go.transform.position=position;go.transform.localScale*=scale;go.transform.rotation=Quaternion.Euler(0,angle,0);}
        static ParticleSystem CopyEffect(ParticleSystem source,string name,Transform parent,Scene scene) {if(!source)return null;var ps=Object.Instantiate(source);ps.name=name;SceneManager.MoveGameObjectToScene(ps.gameObject,scene);ps.transform.SetParent(parent,false);ps.transform.localPosition=Vector3.zero;var main=ps.main;main.playOnAwake=false;main.loop=false;return ps;}
        static AudioClip Tone(string name,bool ambient)
        {
            int rate=22050,count=ambient?rate*8:rate/4;string file=Generated+"/"+name+".wav";
            using(var w=new BinaryWriter(File.Create(file))) {w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);for(int i=0;i<count;i++){double t=(double)i/rate;double sample=ambient?.045*(Math.Sin(2*Math.PI*110*t)+.4*Math.Sin(2*Math.PI*165*t))*Math.Pow(Math.Sin(Math.PI*i/count),2):.2*Math.Sin(2*Math.PI*880*t)*Math.Exp(-t*18);w.Write((short)(sample*32767));}}
            AssetDatabase.ImportAsset(file);return AssetDatabase.LoadAssetAtPath<AudioClip>(file);
        }
    }
}
