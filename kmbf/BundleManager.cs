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
                ResourcesLibrary.LoadedResource resource = new ResourcesLibrary.LoadedResource(asset);
                ResourcesLibrary.s_LoadedResources[MakeAssetId(asset.name)] = resource;
            }
        }

        public static string MakeAssetId(string assetName)
        {
            return $"{AssetPrefix}/{assetName}";
        }
    }
}
