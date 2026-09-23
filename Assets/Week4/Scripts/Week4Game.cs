using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace MysticJungle
{
    public class Week4Game : MonoBehaviour
    {
        public static Week4Game Instance { get; private set; }
        public Week4Player player;
        public Week4Lighting lighting;
        public Transform ui;
        public AudioSource music, sfx;
        public AudioClip collectSound;
        public Vector3 spawn = new Vector3(0,.1f,2);
        public bool Playing { get; private set; }
        int coins, health = 3, difficulty, wallet;
        string current = "MainMenu";
        Week4Pickup[] pickups;
        TMP_Text status;
        readonly Color gold = new Color(.77f,.61f,.31f);
        readonly Color green = new Color(.14f,.4f,.13f);
        readonly Color brown = new Color(.28f,.17f,.085f);
        void Awake() { Instance = this; Time.timeScale = 1; }
        void Start()
        {
            pickups = FindObjectsByType<Week4Pickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            wallet = PlayerPrefs.GetInt("W4.Wallet", 0); difficulty = PlayerPrefs.GetInt("W4.Mode", 0);
            foreach (Button button in ui.GetComponentsInChildren<Button>(true))
            { string action = button.name; button.onClick.AddListener(() => Act(action)); }
            foreach (Slider slider in ui.GetComponentsInChildren<Slider>(true))
            {
                string key = slider.name; slider.value = PlayerPrefs.GetFloat("W4." + key, .6f);
                SetVolume(key, slider.value); slider.onValueChanged.AddListener(v => SetVolume(key, v));
            }
            status = ui.Find("HUD/Status").GetComponent<TMP_Text>();
            Show("MainMenu"); UpdateHUD();
        }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            { if (Playing) Show("Pause"); else if (current == "Pause") Show("HUD"); else Show("MainMenu"); }
            if (Playing) UpdateHUD();
        }
        void SetVolume(string key, float value)
        { PlayerPrefs.SetFloat("W4." + key, value); if (key == "Music" && music) music.volume = value; if (key == "SFX" && sfx) sfx.volume = value; }
        public void Show(string screen)
        {
            current = screen; Playing = screen == "HUD"; player.CanMove = Playing;
            Time.timeScale = Playing || screen == "MainMenu" ? 1 : 0;
            foreach (Transform panel in ui) panel.gameObject.SetActive(panel.name == screen);
            if (player.joystick) { player.joystick.OnPointerUp(null); }
            SetText("MainMenu/Wallet", "YOUR COINS  " + wallet + "     /     " + new[]{"EASY","MEDIUM","HARD"}[difficulty]);
            SetText("Store/Balance", "BALANCE  " + wallet + " coins");
            SetText("Reward/RewardStatus", PlayerPrefs.GetString("W4.RewardDate", "") == DateTime.UtcNow.ToString("yyyy-MM-dd") ? "Today's reward has been claimed" : "100 coins • one reward each UTC day");
        }
        void SetText(string path, string value) { int split=path.IndexOf('/'); var t = ui.Find(path.Insert(split+1,"Content/")); if (t) t.GetComponent<TMP_Text>().text = value; }
        void Act(string action)
        {
            switch(action)
            {
                case "Start": case "Level1": case "Restart": Begin(); break;
                case "Levels": Show("LevelSelect"); break;
                case "Settings": Show("Settings"); break;
                case "Mode": Show("ModeSelect"); break;
                case "Store": Show("Store"); break;
                case "Reward": Show("Reward"); break;
                case "Tutorial": Show("Tutorial"); break;
                case "Back": case "Home": Show("MainMenu"); break;
                case "Pause": Show("Pause"); break;
                case "Resume": Show("HUD"); break;
                case "Jump": player.Jump(); break;
                case "Easy": difficulty = 0; SaveMode(); break;
                case "Medium": difficulty = 1; SaveMode(); break;
                case "Hard": difficulty = 2; SaveMode(); break;
                case "Day": lighting.SetPeriod(0); break;
                case "Sunset": lighting.SetPeriod(1); break;
                case "Night": lighting.SetPeriod(2); break;
                case "Cycle": lighting.automatic = !lighting.automatic; SetText("Settings/LightingStatus", lighting.automatic ? "Automatic cycle: ON" : "Automatic cycle: OFF"); break;
                case "Quality": lighting.sun.shadows = lighting.sun.shadows == LightShadows.None ? LightShadows.Soft : LightShadows.None; SetText("Settings/Quality/Label", lighting.sun.shadows == LightShadows.None ? "SHADOWS: OFF" : "SHADOWS: ON"); break;
                case "Claim":
                    string date = DateTime.UtcNow.ToString("yyyy-MM-dd");
                    if (PlayerPrefs.GetString("W4.RewardDate", "") != date) { wallet += 100; PlayerPrefs.SetString("W4.RewardDate", date); SaveWallet(); }
                    Show("Reward"); break;
                case "BuyTrail":
                    if (PlayerPrefs.GetInt("W4.Trail", 0) == 1) SetText("Store/Balance", "Golden trail already owned");
                    else if (wallet >= 100) { wallet -= 100; PlayerPrefs.SetInt("W4.Trail", 1); SaveWallet(); EnableTrail(); SetText("Store/Balance", "Golden trail equipped • balance " + wallet); }
                    else SetText("Store/Balance", "Need 100 coins • collect coins or claim daily reward");
                    break;
            }
        }
        void SaveMode() { PlayerPrefs.SetInt("W4.Mode", difficulty); PlayerPrefs.Save(); Show("MainMenu"); }
        void SaveWallet() { PlayerPrefs.SetInt("W4.Wallet", wallet); PlayerPrefs.Save(); }
        void EnableTrail() { var trail = player.GetComponent<TrailRenderer>(); if (trail) trail.emitting = PlayerPrefs.GetInt("W4.Trail", 0) == 1; }
        public void Begin()
        {
            coins = 0; health = 3; player.speed = new[]{6f,7f,8.5f}[difficulty];
            foreach (var pickup in pickups) pickup.gameObject.SetActive(true);
            player.Teleport(spawn); EnableTrail(); Show("HUD");
        }
        public void Collect() { coins++; wallet++; SaveWallet(); if (sfx && collectSound) sfx.PlayOneShot(collectSound); UpdateHUD(); }
        public void Damage(bool fall)
        {
            if (!Playing) return; health--; if (fall) player.Teleport(spawn);
            if (health <= 0) { SetText("GameOver/Results", "COINS   " + coins + "\nDISTANCE   " + Mathf.Max(0, Mathf.RoundToInt(player.transform.position.z - spawn.z)) + " m\nTry again. The ruins are waiting."); Show("GameOver"); }
            UpdateHUD();
        }
        public void Finish() { SetText("Victory/Results", "THE SANCTUARY IS FOUND\n\nCollected " + coins + " coins\nBest coins: " + Mathf.Max(coins,PlayerPrefs.GetInt("W4.Best",0))); PlayerPrefs.SetInt("W4.Best",Mathf.Max(coins,PlayerPrefs.GetInt("W4.Best",0))); PlayerPrefs.Save(); Show("Victory"); }
        void UpdateHUD() { if (status) status.text = "LIFE " + health + "/3     COINS " + coins + "\n" + Mathf.Max(0,Mathf.RoundToInt(player.transform.position.z-spawn.z)) + " m    •    " + lighting.Period; }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }

        // Called by the editor builder: all UI objects remain editable in the saved scene.
        public void BuildUI()
        {
            var canvas = new GameObject("UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = 1f;
            ui = canvas.transform;
            var p = Page("MainMenu", "MYSTIC\nJUNGLE RUINS", "EXPLORE  /  COLLECT  /  SURVIVE");
            Label(p,"Wallet","",-280,23); Btn(p,"Start","PLAY CHAPTER 01",-355,green); Btn(p,"Levels","SELECT LEVEL",-455,brown);
            Btn(p,"Mode","DIFFICULTY",-550,brown); Btn(p,"Settings","SETTINGS",-655,brown); Btn(p,"Store","STORE",-760,brown); Btn(p,"Reward","DAILY REWARD",-865,brown); Btn(p,"Tutorial","HOW TO PLAY",-970,brown);
            Label(p,"Chapter","CHAPTER 01\nThe Lost Sanctuary",-1150,29).color=gold;
            p = Page("LevelSelect","SELECT LEVEL","THE LOST SANCTUARY"); Btn(p,"Level1","01   JUNGLE RUINS  •  PLAY",-330,green);
            Label(p,"Locked","02 – 09\n\nLOCKED • Future chapters",-540,34); Back(p);
            p = Page("Settings","SETTINGS","MAKE THE JUNGLE YOURS"); Volume(p,"Music",-300); Volume(p,"SFX",-430);
            Btn(p,"Quality","SHADOWS: ON",-580,brown); Btn(p,"Day","LIGHTING: DAY",-690,brown); Btn(p,"Sunset","LIGHTING: SUNSET",-795,brown); Btn(p,"Night","LIGHTING: NIGHT",-900,brown); Btn(p,"Cycle","TOGGLE DAY / NIGHT CYCLE",-1005,green); Label(p,"LightingStatus","Automatic cycle: ON",-1090,26); Back(p);
            p = Page("ModeSelect","SELECT MODE","CHOOSE YOUR PACE"); Btn(p,"Easy","EASY  •  6 m/s",-360,green); Btn(p,"Medium","MEDIUM  •  7 m/s",-500,new Color(.6f,.36f,.07f)); Btn(p,"Hard","HARD  •  8.5 m/s",-640,new Color(.48f,.12f,.08f)); Label(p,"Note","Difficulty changes movement speed.\nYou have three lives in every mode.",-830,28); Back(p);
            p = Page("Pause","GAME PAUSED","TAKE A BREATH"); Btn(p,"Resume","RESUME",-380,green); Btn(p,"Restart","RESTART",-530,brown); Btn(p,"Home","MAIN MENU",-680,brown);
            foreach (string name in new[]{"GameOver","Victory"}) { p = Page(name,name=="Victory" ? "RUINS DISCOVERED" : "GAME OVER","YOUR JOURNEY"); Label(p,"Results","",-390,34); Btn(p,"Restart","PLAY AGAIN",-700,green); Btn(p,"Home","MAIN MENU",-840,brown); }
            p = Page("Reward","DAILY REWARD","RETURN TO THE JUNGLE"); Label(p,"Amount","100\nGOLD COINS",-370,68); Label(p,"RewardStatus","",-610,28); Btn(p,"Claim","CLAIM REWARD",-790,green); Back(p);
            p = Page("Store","STORE","EARNED COINS • COSMETICS"); Label(p,"Balance","",-300,30); Label(p,"Item","GOLDEN TRAIL\nLeave a golden shimmer behind your ninja.",-490,32); Btn(p,"BuyTrail","EQUIP TRAIL  •  100 COINS",-740,green); Label(p,"Note","Earn coins on the route or claim daily rewards.",-910,26); Back(p);
            p = Page("Tutorial","HOW TO PLAY","FOLLOW THE PATH TO THE RUINS"); Label(p,"Instructions","MOVE\nWASD / arrow keys or the touch joystick\n\nJUMP\nSpace or the JUMP button\n\nCOLLECT\nGold coins add to your wallet\n\nSURVIVE\nAvoid fallen logs and the river\n\nEXPLORE\nCross the bridge and reach the golden portal",-610,33); Back(p);
            var hud = new GameObject("HUD",typeof(RectTransform)); hud.transform.SetParent(ui,false); Stretch(hud.GetComponent<RectTransform>());
            var stat = Label(hud.transform,"Status","",0,32); Anchor(stat.rectTransform,new Vector2(1,1),new Vector2(-255,-125),new Vector2(470,130));
            var pause = Btn(hud.transform,"Pause","II",0,brown); Anchor(pause.GetComponent<RectTransform>(),new Vector2(0,1),new Vector2(110,-120),new Vector2(130,110));
            var jump = Btn(hud.transform,"Jump","JUMP",0,green); Anchor(jump.GetComponent<RectTransform>(),new Vector2(1,0),new Vector2(-165,180),new Vector2(220,160));
            var bg = Box(hud.transform,"Joystick",new Color(.14f,.18f,.12f,.85f)); Anchor(bg.rectTransform,new Vector2(0,0),new Vector2(190,200),new Vector2(260,260));
            var handle = Box(bg.transform,"Handle",gold); Anchor(handle.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(100,100)); handle.raycastTarget = false;
            var joy = bg.gameObject.AddComponent<Joystick>(); joy.background=bg.rectTransform; joy.handle=handle.rectTransform; joy.deadZone=.08f; player.joystick=joy;
            foreach(Transform panel in ui) panel.gameObject.SetActive(panel.name=="MainMenu");
        }
        Transform Page(string name,string title,string subtitle)
        {
            var veil=Box(ui,name,new Color(.02f,.045f,.03f,.66f)); Stretch(veil.rectTransform);
            var panel=Box(veil.transform,"Card",new Color(.065f,.085f,.058f,.96f)); Anchor(panel.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(850,1320));
            // Elements are direct children of the page so runtime paths stay simple.
            var content=new GameObject("Content",typeof(RectTransform)); content.transform.SetParent(veil.transform,false); Anchor(content.GetComponent<RectTransform>(),new Vector2(.5f,.5f),new Vector2(0,660),new Vector2(850,0));
            Label(content.transform,"Title",title,-130,68).color=gold; Label(content.transform,"Subtitle",subtitle,-225,25).color=new Color(.7f,.75f,.61f);
            return content.transform;
        }
        void Back(Transform p) { Btn(p,"Back","BACK",-1200,brown); }
        TMP_Text Label(Transform p,string name,string value,float y,float size)
        { var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(p,false);var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.96f,.91f,.75f);t.raycastTarget=false;t.enableWordWrapping=true;Anchor(t.rectTransform,new Vector2(.5f,1),new Vector2(0,y),new Vector2(730,230));return t; }
        Image Box(Transform p,string name,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(p,false);var img=go.GetComponent<Image>();img.color=color;return img; }
        Button Btn(Transform p,string name,string title,float y,Color color)
        { var img=Box(p,name,color);Anchor(img.rectTransform,new Vector2(.5f,1),new Vector2(0,y),new Vector2(690,85));var outline=img.gameObject.AddComponent<Outline>();outline.effectColor=gold;outline.effectDistance=new Vector2(2,-2);var b=img.gameObject.AddComponent<Button>();var colors=b.colors;colors.highlightedColor=new Color(1,.9f,.6f);colors.pressedColor=new Color(.7f,.7f,.7f);b.colors=colors;var t=Label(img.transform,"Label",title,0,34);Stretch(t.rectTransform);return b; }
        void Volume(Transform p,string name,float y)
        {
            Label(p,name+"Label",name.ToUpperInvariant(),y-5,30);
            var bg=Box(p,name,new Color(.2f,.24f,.16f));Anchor(bg.rectTransform,new Vector2(.5f,1),new Vector2(0,y-55),new Vector2(640,24));
            var handle=Box(bg.transform,"Handle",gold);Anchor(handle.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(44,44));
            var slider=bg.gameObject.AddComponent<Slider>();slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.value=.6f;
        }
        static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero; }
        static void Anchor(RectTransform r,Vector2 anchor,Vector2 position,Vector2 size) { r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size; }
    }
}
