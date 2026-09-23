using UnityEngine;
using UnityEngine.Rendering;

namespace MysticJungle
{
    public class Week4Lighting : MonoBehaviour
    {
        public Light sun;
        public Light[] torches;
        public ReflectionProbe waterProbe;
        [Range(0, 1)] public float timeOfDay = .22f;
        public float cycleSeconds = 180;
        public bool automatic = false;
        float nextReflection;
        public string Period => timeOfDay < .42f ? "DAY" : timeOfDay < .64f ? "SUNSET" : "NIGHT";
        void Start() { automatic = false; Apply(); }
        void Update()
        {
            if (automatic) timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / cycleSeconds, 1);
            Apply();
            if (waterProbe && Time.time >= nextReflection) { waterProbe.RenderProbe(); nextReflection = Time.time + 20; }
        }
        public void SetPeriod(int index) { automatic = false; timeOfDay = index == 0 ? .22f : index == 1 ? .53f : .8f; Apply(); }
        public void Apply()
        {
            float daylight = Mathf.SmoothStep(0, 1, Mathf.Clamp01(Mathf.Cos((timeOfDay - .2f) * Mathf.PI * 2) * .7f + .5f));
            float dusk = Mathf.Clamp01(1 - Mathf.Abs(timeOfDay - .53f) / .16f);
            sun.intensity = Mathf.Lerp(.16f, 1.25f, daylight);
            sun.color = Color.Lerp(Color.Lerp(new Color(.4f,.55f,1), new Color(1,.95f,.8f), daylight), new Color(1,.48f,.2f), dusk);
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(12, 65, daylight), -35 + timeOfDay * 80, 0);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.055f,.09f,.17f), new Color(.48f,.65f,.65f), daylight);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(.04f,.065f,.08f), new Color(.22f,.32f,.22f), daylight);
            RenderSettings.ambientGroundColor = new Color(.035f,.05f,.035f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .012f;
            RenderSettings.fogColor = Color.Lerp(new Color(.025f,.065f,.12f), new Color(.43f,.65f,.63f), daylight);
            if (Camera.main) Camera.main.backgroundColor = RenderSettings.fogColor;
            for (int i = 0; i < torches.Length; i++) if (torches[i])
            {
                torches[i].intensity = Mathf.Lerp(3, .7f, daylight) * (.9f + .15f * Mathf.PerlinNoise(i % 6 * 3, Time.time * 5));
                if (Application.isPlaying && Camera.main) torches[i].enabled = Mathf.Abs(torches[i].transform.position.z - Camera.main.transform.position.z) < 65;
            }
        }
    }
}
