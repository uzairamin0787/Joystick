using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace MysticJungle.Editor
{
    [InitializeOnLoad]
    public static class Week4Verification
    {
        static Week4Game game;
        static Week4Verification() {EditorApplication.update+=Tick;Lightmapping.bakeCompleted+=BakeDone;}
        static void BakeDone() {var cycle=Object.FindFirstObjectByType<Week4Lighting>();if(cycle)cycle.Apply();EditorSceneManager.SaveOpenScenes();File.WriteAllText("Temp/Week4Bake.done","Lightmaps: "+LightmapSettings.lightmaps.Length);}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlaying && File.Exists("Temp/Week4ProbeFix.request"))
            {
                File.Delete("Temp/Week4ProbeFix.request");if(Lightmapping.isRunning)Lightmapping.Cancel();
                foreach(var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    var filter=renderer.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh||!AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith("Assets/Trees Package Lite/"))continue;
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);renderer.receiveGI=ReceiveGI.LightProbes;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
                }
                EditorSceneManager.SaveOpenScenes();File.WriteAllText("Temp/Week4ProbeFix.done","Foliage uses probes; no imported model changes");
            }
            if(File.Exists("Temp/Week4Play.request")) {File.Delete("Temp/Week4Play.request");EditorApplication.isPlaying=true;return;}
            if(File.Exists("Temp/Week4Stop.request")) {File.Delete("Temp/Week4Stop.request");EditorApplication.isPlaying=false;return;}
            if(EditorApplication.isPlaying && File.Exists("Temp/Week4Capture.request")) {string page=File.ReadAllText("Temp/Week4Capture.request").Trim();File.Delete("Temp/Week4Capture.request");var g=Object.FindFirstObjectByType<Week4Game>();g.Show(page);Directory.CreateDirectory("Documentation/Week4");ScreenCapture.CaptureScreenshot("Documentation/Week4/"+page+".png",2);}
            if(!EditorApplication.isPlaying && File.Exists("Temp/Week4Polish.request"))
            {File.Delete("Temp/Week4Polish.request");Polish();}
            if(!EditorApplication.isPlaying && File.Exists("Temp/Week4Bake.request"))
            {File.Delete("Temp/Week4Bake.request");RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.025f,.035f,.04f);RenderSettings.ambientIntensity=.1f;Lightmapping.BakeAsync();}
        }
        [MenuItem("Week 4/Polish Saved Demo")]
        static void Polish()
        {
            game=Object.FindFirstObjectByType<Week4Game>();if(!game)return;
            var oldUI=game.ui.gameObject;Object.DestroyImmediate(oldUI);game.BuildUI();game.ui.SetParent(game.transform.parent,true);
            var buttonSprite=FrameSprite("CarvedButton",false);var circleSprite=FrameSprite("RoundControl",true);
            foreach(var image in game.ui.GetComponentsInChildren<Image>(true))
            {
                if(image.GetComponent<Button>()||image.name=="Card") {image.sprite=buttonSprite;image.type=Image.Type.Sliced;}
                if(image.name=="Joystick"||image.name=="Handle") {image.sprite=circleSprite;image.type=Image.Type.Simple;}
            }
            foreach(var animator in game.player.GetComponentsInChildren<Animator>()) {animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;}
            // Coin triggers must not participate in the player's ground raycast.
            foreach(var pickup in Object.FindObjectsByType<Week4Pickup>(FindObjectsSortMode.None))pickup.gameObject.layer=0;
            foreach(var renderer in game.player.GetComponentsInChildren<Renderer>())renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
            game.lighting.SetPeriod(0);game.lighting.automatic=true;
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();File.WriteAllText("Temp/Week4Polish.done","UI and ninja polished");
        }
        static Sprite FrameSprite(string name,bool circle)
        {
            const int size=128;string path="Assets/Week4/Generated/"+name+".png";
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float edge=circle?63-Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f)):Mathf.Min(x,y,size-1-x,size-1-y);
                float grain=Mathf.PerlinNoise(x*.15f,y*.6f)*.09f;
                float value=edge<2?.25f:edge<5?.95f:edge<8?.46f:.65f+.22f*y/size+grain;
                texture.SetPixel(x,y,new Color(value,value,value,edge<0?0:1));
            }
            texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(12,12,12,12);importer.alphaIsTransparency=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
