using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace CatosTents
{
    internal static class TentPiece
    {
        private const string BundleResourceSuffix = ".Assets.AssetBundles.catostents";
        private static bool _registered;
        private static AssetBundle _bundle;

        private static readonly TentDefinition[] Tents =
        {
            new TentDefinition("catostent_market_01", "CatosTent_Market_01_Visual", "Market Tent 01"),
            new TentDefinition("catostent_market_02", "CatosTent_Market_02_Visual", "Market Tent 02"),
            new TentDefinition("catostent_market_props_01", "CatosTent_Market_Props_01_Visual", "Market Tent Props 01"),
            new TentDefinition("catostent_market_props_02", "CatosTent_Market_Props_02_Visual", "Market Tent Props 02")
        };

        public static void Register()
        {
            if (_registered) return;
            _registered = true;

            GameObject buildSource = FindBuildPiecePrefab();
            if (buildSource == null || !LoadBundle()) return;

            foreach (TentDefinition tent in Tents) RegisterTent(buildSource, tent);
        }

        private static void RegisterTent(GameObject buildSource, TentDefinition tent)
        {
            GameObject visualSource = _bundle.LoadAsset<GameObject>(tent.VisualPrefabName);
            Sprite icon = _bundle.LoadAsset<Sprite>($"{tent.VisualPrefabName}_Icon");
            if (visualSource == null || icon == null)
            {
                Plugin.Log.LogError($"CatosTents: missing visual or icon for {tent.PieceId}.");
                return;
            }

            GameObject clone = UnityEngine.Object.Instantiate(buildSource);
            clone.name = tent.PieceId;
            AttachVisual(clone, visualSource);

            var piece = new CustomPiece(clone, true, new PieceConfig
            {
                PieceTable = "Hammer",
                Category = "CatosTents",
                Name = tent.DisplayName,
                Description = "A sturdy placeable tent for camps and market scenes.",
                Icon = icon,
                Requirements = new[]
                {
                    new RequirementConfig { Item = "Wood", Amount = 20 },
                    new RequirementConfig { Item = "DeerHide", Amount = 6 },
                    new RequirementConfig { Item = "FineWood", Amount = 4 }
                }
            });

            piece.PiecePrefab.GetComponent<Piece>().m_canBeRemoved = true;
            PieceManager.Instance.AddPiece(piece);
            Plugin.Log.LogInfo($"Registered functional tent bed piece '{tent.PieceId}'");
        }

        private static GameObject FindBuildPiecePrefab()
        {
            string[] candidates = { "piece_wood_floor", "wood_floor", "piece_stone_floor", "stone_floor" };
            GameObject buildPiece = candidates.Select(PrefabManager.Cache.GetPrefab<GameObject>).FirstOrDefault(prefab => prefab != null);
            if (buildPiece == null)
            {
                Plugin.Log.LogError($"CatosTents: none of the neutral build-piece candidates were found: {string.Join(", ", candidates)}");
            }
            else
            {
                Plugin.Log.LogInfo($"CatosTents: using neutral vanilla build-piece prefab '{buildPiece.name}'");
            }
            return buildPiece;
        }

        private static void AttachVisual(GameObject functionalPrefab, GameObject source)
        {
            GameObject visual = UnityEngine.Object.Instantiate(source);
            visual.name = source.name;
            visual.transform.SetParent(functionalPrefab.transform, false);
            foreach (Renderer renderer in functionalPrefab.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.transform.IsChildOf(visual.transform)) renderer.enabled = false;
            }
        }

        private static bool LoadBundle()
        {
            if (_bundle != null) return true;
            Assembly assembly = typeof(TentPiece).Assembly;
            string resource = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(BundleResourceSuffix, StringComparison.OrdinalIgnoreCase));
            if (resource == null)
            {
                Plugin.Log.LogError($"CatosTents: embedded resource ending with '{BundleResourceSuffix}' was not found.");
                return false;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            using (var buffer = new MemoryStream())
            {
                if (stream == null)
                {
                    Plugin.Log.LogError($"CatosTents: embedded bundle stream '{resource}' was empty.");
                    return false;
                }
                stream.CopyTo(buffer);
                _bundle = AssetBundle.LoadFromMemory(buffer.ToArray());
            }

            if (_bundle == null)
            {
                Plugin.Log.LogError("CatosTents: AssetBundle.LoadFromMemory returned null.");
                return false;
            }
            return true;
        }

        private sealed class TentDefinition
        {
            public TentDefinition(string pieceId, string visualPrefabName, string displayName)
            {
                PieceId = pieceId;
                VisualPrefabName = visualPrefabName;
                DisplayName = displayName;
            }
            public string PieceId { get; }
            public string VisualPrefabName { get; }
            public string DisplayName { get; }
        }
    }
}
