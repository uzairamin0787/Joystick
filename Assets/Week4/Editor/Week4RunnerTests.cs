using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MysticJungle.Editor
{
    [InitializeOnLoad]
    public static class Week4RunnerTests
    {
        static IEnumerator test;
        static double next;
        static Keyboard keyboard, originalKeyboard;
        static Week4Game game;
        static int wallet, best;
        static bool background;
        static readonly List<string> checks = new List<string>();
        static Week4RunnerTests() { EditorApplication.update += Tick; }
        static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (EditorApplication.isPlaying && test == null && File.Exists("Temp/Week4Test.request"))
            {
                File.Delete("Temp/Week4Test.request");
                game = Object.FindAnyObjectByType<Week4Game>();
                wallet = PlayerPrefs.GetInt("W4.Wallet", 0); best = PlayerPrefs.GetInt("W4.BestDistance", 0);
                background = Application.runInBackground; Application.runInBackground = true;
                originalKeyboard = Keyboard.current; keyboard = InputSystem.AddDevice<Keyboard>();
                checks.Clear(); test = Run(); next = 0;
            }
            if (test == null || EditorApplication.timeSinceStartup < next) return;
            try
            {
                if (!test.MoveNext()) { Finish(); return; }
                next = EditorApplication.timeSinceStartup + Convert.ToDouble(test.Current);
            }
            catch (Exception e) { checks.Add("FAIL: " + e); Finish(); }
        }
        static void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        static void Check(bool result, string label) { checks.Add((result ? "PASS: " : "FAIL: ") + label); File.WriteAllLines("Temp/Week4Tests.txt", checks); }
        static void Place(float x, float y, float z)
        {
            Keys(); game.player.Teleport(new Vector3(x,y,z)); game.Route.RefreshRoute(); Physics.SyncTransforms();
        }
        static IEnumerator Run()
        {
            yield return .5;
            game.Begin(); yield return .4;
            Check(game.Playing && Mathf.Abs(game.player.transform.position.z-game.spawn.z)<.1f,"No forced forward movement while idle");
            Keys(Key.W); yield return .5;
            Check(game.player.transform.position.z>game.spawn.z+1,"Forward input moves player");
            float before=game.player.transform.position.z; Keys(Key.S); yield return .35;
            Check(game.player.transform.position.z<before-1,"Backward input moves player"); Keys();
            Place(0,.2f,14); yield return .2;
            Keys(Key.W); yield return .9;
            Check(game.Playing && game.player.transform.position.z<16.3f,"Log blocks movement without teleport or game over");
            yield return .5;
            Check(game.Playing && game.player.transform.position.z<16.3f,"Holding forward against log remains blocked");
            Keys(Key.W,Key.Space); yield return .12; Keys(Key.W); yield return .8;
            Check(game.Playing && game.player.transform.position.z>18,"Jump crosses the log"); Keys();
            game.Show("Pause"); Vector3 paused=game.player.transform.position; yield return .2;
            Check(Time.timeScale==0 && game.player.transform.position==paused,"Pause freezes player"); game.Show("HUD");
            for(int repeat=1;repeat<=7;repeat++)
            {
                float seam=repeat*Week4Runner.Length-4;
                Place(0,.25f,seam-1); yield return .25;
                Keys(Key.W); yield return .8;
                Check(game.Playing && game.player.transform.position.z>seam+1 && game.player.transform.position.y>-.3f,"Continuous ground and movement at repeat "+repeat);
                Check(game.Route.ActiveTileCount==5,"Bounded five-chunk pool at repeat "+repeat);
                Keys();
            }
            float backSeam=4*Week4Runner.Length-4;
            Place(0,.2f,backSeam+1); yield return .25; Keys(Key.S); yield return .7;
            Check(game.Playing && game.player.transform.position.z<backSeam-1 && game.player.transform.position.y>-.3f,"Backward traversal across recycled route"); Keys();
            game.Begin(); Place(0,.2f,49); yield return .3;
            Check(game.Playing && game.player.transform.position.y>-.3f,"Original bridge still supports player");
            Place(0,1,85); yield return .4;
            Check(game.Playing && game.player.transform.position.y>.6f,"Original raised temple terrace restored");
            var all=Object.FindObjectsByType<Week4Pickup>(FindObjectsInactive.Include);
            var coin=all.First(p=>!p.finish && p.gameObject.activeInHierarchy && p.transform.position.z<10);
            Place(coin.transform.position.x,.2f,coin.transform.position.z); yield return .3;
            Check(!coin.gameObject.activeSelf,"Scattered coin collected");
            long coinKey=coin.routeKey;
            Place(0,.2f,650); yield return .2; Place(0,.2f,4); yield return .2;
            Check(Object.FindObjectsByType<Week4Pickup>(FindObjectsInactive.Include).Any(p=>p.routeKey==coinKey&&!p.gameObject.activeSelf),"Backtracking does not respawn collected coins");
            var meshes=Object.FindObjectsByType<MeshRenderer>().Where(r=>r.transform.parent && r.transform.IsChildOf(game.transform.parent) && r.lightmapIndex>=0 && r.lightmapIndex<LightmapSettings.lightmaps.Length).ToArray();
            Check(LightmapSettings.lightmaps.Length>0 && meshes.Any(r=>r.transform.position.z>106),"Repeated static scenery retains baked lightmaps");
            Check(!meshes.Any(r=>r.isPartOfStaticBatch),"Recycled scenery is not locked in static batches");
            Check(game.lighting.sun.lightmapBakeType==LightmapBakeType.Mixed && !game.lighting.automatic,"Stable mixed sunlight");
            Check(game.player.GetComponentsInChildren<Renderer>().All(r=>r.lightProbeUsage==LightProbeUsage.Off),"Player uses realtime lighting on every chunk");
            game.Show("MainMenu");
            string[] names={"Start","Mode","Settings","Store","Reward","Tutorial"}; bool layout=true;
            for(int i=0;i<names.Length;i++) layout &= Mathf.Abs(((RectTransform)game.ui.Find("MainMenu/Content/"+names[i])).anchoredPosition.y-(-355-i*110))<1;
            Check(layout&&!game.ui.Find("MainMenu/Content/Levels").gameObject.activeSelf,"Menu reflows with no missing level button gap");
            game.Show("Settings");var toggle=game.ui.Find("Settings/Content/MusicToggle").GetComponent<Button>();bool muted=game.music.mute;toggle.onClick.Invoke();Check(game.music.mute!=muted,"Music on/off works");toggle.onClick.Invoke();
            game.Begin();Place(3.3f,.2f,10);yield return .2;
            Check(!game.Playing&&game.ui.Find("GameOver").gameObject.activeSelf,"Leaving path ends run");
            game.Begin();Place(0,.2f,-3.7f);yield return .2;
            Check(!game.Playing,"Leaving the start behind ends run");
            game.Begin();Place(4,.9f,84);yield return .2;Keys(Key.D);yield return .35;
            Check(!game.Playing && game.ui.Find("GameOver/Content/Results").GetComponent<TMPro.TMP_Text>().text.Contains("Pillar"),"Actual pillar collision ends run");Keys();
            game.Begin(); yield return .2;
            Check(game.Playing&&game.player.transform.position.z<3,"Restart restores spawn and route");
            // Walk the complete original route three times: this catches missed stairs,
            // bridge entry edges, later logs and the temple exit, not just seam teleports.
            double deadline = EditorApplication.timeSinceStartup + 100;
            var jumped = new HashSet<int>(); Keys(Key.W);
            while (game.Playing && game.player.transform.position.z < 320 && EditorApplication.timeSinceStartup < deadline)
            {
                float z=game.player.transform.position.z;
                int chunk=Mathf.FloorToInt(z/Week4Runner.Length); float local=z-chunk*Week4Runner.Length;
                float[] approach={13.5f,63.5f,73.8f};
                for(int i=0;i<approach.Length;i++)
                    if(local>=approach[i] && local<approach[i]+2 && jumped.Add(chunk*3+i)) game.player.Jump();
                yield return .03;
            }
            Check(game.Playing && game.player.transform.position.z>=320,"Three complete routes: logs, bridge, stairs, terrace and seams");
            checks.Add("INFO: full-route finish position " + game.player.transform.position);
            Keys();
            game.Show("MainMenu");yield return .2;
            ScreenCapture.CaptureScreenshot("Temp/RunnerMenu.png");yield return .3;
            game.Show("Settings");yield return .2;ScreenCapture.CaptureScreenshot("Temp/RunnerSettings.png");yield return .3;
            game.Begin();Place(0,.2f,9);yield return .5;ScreenCapture.CaptureScreenshot("Temp/RunnerRoute.png");yield return .3;
        }
        static void Finish()
        {
            Keys();InputSystem.RemoveDevice(keyboard);if(originalKeyboard!=null) originalKeyboard.MakeCurrent();
            game.Show("MainMenu");
            typeof(Week4Game).GetField("wallet",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,wallet);
            PlayerPrefs.SetInt("W4.Wallet",wallet);PlayerPrefs.SetInt("W4.BestDistance",best);PlayerPrefs.Save();
            Application.runInBackground=background;File.WriteAllLines("Temp/Week4Tests.txt",checks);
            test=null;
        }
    }
}
