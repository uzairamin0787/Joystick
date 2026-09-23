using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MysticJungle
{
    // Keep two routes behind the player and two ahead. Never move the player/camera
    // at a seam: only recycle chunks outside the visible and playable neighbourhood.
    public class Week4Runner : MonoBehaviour
    {
        public const float Length = 106f;
        public const int PoolSize = 5;
        readonly Transform[] tiles = new Transform[PoolSize];
        readonly int[] indices = new int[PoolSize];
        readonly Week4Pickup[][] coins = new Week4Pickup[PoolSize][];
        readonly HashSet<long> collected = new HashSet<long>();
        Week4Game game;
        float furthest;
        public float Distance => furthest;
        public int ActiveTileCount => tiles.Length;
        public int FirstChunk { get; private set; }

        public void Initialize(Week4Game owner)
        {
            game = owner;
            var root = game.transform.parent;
            var environment = root.Find("Environment");
            var collectibles = root.Find("Gameplay/Collectibles");
            tiles[0] = new GameObject("Endless Route 0").transform;
            tiles[0].SetParent(root, false);
            environment.SetParent(tiles[0], true);
            collectibles.SetParent(tiles[0], true);
            // Keep the original stairs and terrace. Only open the end wall and bounds.
            foreach (Transform part in environment.GetComponentsInChildren<Transform>(true))
            {
                if (part.name == "Start boundary" || part.name == "Level boundary" || part.name == "Sanctuary rear wall")
                    part.gameObject.SetActive(false);
                if (part.name == "Temple terrace" || part.name.StartsWith("Temple stair")) part.gameObject.SetActive(true);
                string label = part.name.ToLowerInvariant();
                if (label.Contains("rock") && part.GetComponent<Collider>() ||
                    label.StartsWith("pillar") || label.StartsWith("bush_") || label.StartsWith("coconut_tree"))
                {
                    if (!part.GetComponent<Week4Hazard>()) part.gameObject.AddComponent<Week4Hazard>();
                    if (part.GetComponentsInChildren<Collider>().Length == 0 && label.StartsWith("bush_"))
                    {
                        var collider = part.gameObject.AddComponent<CapsuleCollider>();
                        collider.center = Vector3.up * .45f; collider.height = .9f; collider.radius = .45f;
                    }
                }
            }
            foreach (var pickup in root.GetComponentsInChildren<Week4Pickup>(true))
                if (pickup.finish) pickup.gameObject.SetActive(false);
            // Torch lights live outside Environment in the authored scene; stream them too.
            foreach (var light in game.lighting.torches)
                if (light) light.transform.SetParent(tiles[0], true);
            var spotlight = root.Find("Lighting/Sanctuary Spot Light");
            if (spotlight) spotlight.SetParent(tiles[0], true);
            var sources = tiles[0].GetComponentsInChildren<Renderer>(true);
            for (int i = 1; i < PoolSize; i++)
            {
                tiles[i] = Instantiate(tiles[0], root);
                tiles[i].name = "Endless Route " + i;
                var copies = tiles[i].GetComponentsInChildren<Renderer>(true);
                // Instantiate does not reliably retain baked lightmap assignments.
                for (int r = 0; r < copies.Length; r++)
                {
                    copies[r].lightmapIndex = sources[r].lightmapIndex;
                    copies[r].lightmapScaleOffset = sources[r].lightmapScaleOffset;
                }
            }
            var torches = new List<Light>();
            for (int i = 0; i < PoolSize; i++)
            {
                coins[i] = tiles[i].GetComponentsInChildren<Week4Pickup>(true);
                foreach (var light in tiles[i].GetComponentsInChildren<Light>())
                    if (light.type == LightType.Point) torches.Add(light);
            }
            game.lighting.torches = torches.ToArray();
            if (Camera.main) Camera.main.useOcclusionCulling = false;
            ResetRoute();
        }

        public void ResetRoute()
        {
            collected.Clear(); furthest = 0; FirstChunk = 0;
            for (int i = 0; i < PoolSize; i++) Assign(i, i);
            Physics.SyncTransforms();
        }

        void Update()
        {
            if (!game.Playing) return;
            furthest = Mathf.Max(furthest, game.player.transform.position.z - game.spawn.z);
            RefreshRoute();
        }

        public void RefreshRoute()
        {
            int current = Mathf.Max(0, Mathf.FloorToInt((game.player.transform.position.z + 4f) / Length));
            FirstChunk = Mathf.Max(0, current - 2);
            for (int desired = FirstChunk; desired < FirstChunk + PoolSize; desired++)
            {
                bool exists = false;
                for (int i = 0; i < PoolSize; i++) if (indices[i] == desired) exists = true;
                if (exists) continue;
                for (int i = 0; i < PoolSize; i++)
                    if (indices[i] < FirstChunk || indices[i] >= FirstChunk + PoolSize) { Assign(i, desired); break; }
            }
        }

        void Assign(int slot, int chunk)
        {
            indices[slot] = chunk;
            tiles[slot].position = Vector3.forward * (chunk * Length);
            var random = new System.Random(41 + chunk * 7919);
            for (int c = 0; c < coins[slot].Length; c++)
            {
                var pickup = coins[slot][c];
                if (pickup.finish) continue;
                float z = pickup.transform.localPosition.z;
                float width = z > 41 && z < 59 ? .85f : 1.9f;
                float x = ((c + random.Next(0, 2)) % 3 - 1) * width;
                float y = z >= 79 && z <= 94 ? 1.6f : .9f;
                if (Mathf.Abs(z - 17) < 1.5f || Mathf.Abs(z - 67) < 1.5f) y = 1.65f;
                pickup.transform.localPosition = new Vector3(x, y, z);
                pickup.route = this; pickup.routeKey = ((long)chunk << 32) | (uint)c;
                pickup.ResetOrigin();
                pickup.gameObject.SetActive(!collected.Contains(pickup.routeKey));
                foreach (var renderer in pickup.GetComponentsInChildren<Renderer>()) renderer.lightProbeUsage = LightProbeUsage.Off;
            }
        }

        public void MarkCollected(long key) { collected.Add(key); }

        public bool IsOnPath(Vector3 position)
        {
            if (position.z < -3.5f) return false;
            float z = Mathf.Repeat(position.z + 4f, Length) - 4f;
            float x = Mathf.Abs(position.x);
            if (z > 42f && z < 57f) return x <= 1.7f;
            if (z >= 75.5f && z <= 94f) return x <= 7.15f;
            if (z >= 17f && z <= 36f && position.x >= 0 && position.x <= 8.8f) return true;
            return x <= 3f;
        }
    }
}
