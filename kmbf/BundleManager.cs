using Kingmaker.Blueprints;
using kmbf.Patch.BP;
using System.Reflection;
using UnityEngine;

namespace kmbf
{
    static class BundleManager
    {
        // We have some extra assets that we ship in a Unity "AssetBundle"
        // These are all loaded and inserted into ResourcesLibrary for now
        static readonly string BundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(LibraryScriptableObject_LoadDictionary_Patch)).Location), "kmbf.bundle");
        static readonly string AssetPrefix = "kmbf";

        static readonly Dictionary<string, UnityEngine.Object> s_bundleAssets = new Dictionary<string, UnityEngine.Object>();

        public static void LoadBundle()
        {
            if (!File.Exists(BundlePath))
            {
                Main.Log.Error($"No asset bundle found at {BundlePath}");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(BundlePath);

            var assets = bundle.LoadAllAssets();
            foreach (var asset in assets)
            {
                var assetId = MakeAssetId(asset);
                s_bundleAssets[assetId] = asset;
            }
        }

        public static string MakeAssetId<T>(T asset) where T : UnityEngine.Object
        {
            return MakeAssetId(asset.name, asset.GetType());
        }

        public static string MakeAssetId<T>(string assetName) where T : UnityEngine.Object
        {
            return MakeAssetId(assetName, typeof(T));
        }

        public static string MakeAssetId(string assetName, Type type)
        {
            return $"{AssetPrefix}/{assetName}:{type.Name}";
        }

        public static bool IsKmbfAssetId(string assetId)
        {
            return assetId.StartsWith($"{AssetPrefix}/");
        }

        public static T GetResource<T>(string assetId) where T : UnityEngine.Object
        {
            if(IsKmbfAssetId(assetId))
            {
                return s_bundleAssets[assetId] as T;
            }
            else
            {
                return ResourcesLibrary.TryGetResource<T>(assetId);
            }
        }

        public static T GetKmbfResource<T>(string assetName) where T : UnityEngine.Object
        {
            return s_bundleAssets[MakeAssetId<T>(assetName)] as T;
        }
    }
}
