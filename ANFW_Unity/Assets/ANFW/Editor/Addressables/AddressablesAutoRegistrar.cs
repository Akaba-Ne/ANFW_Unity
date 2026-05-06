using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace ANFW.Editor
{
    public class AddressablesAutoRegistrar : AssetPostprocessor
    {
        private const string WATCHED_FOLDER = "Assets/AddressablesResources";
        private const string GROUP_NAME = "AddressablesResources";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            foreach (var assetPath in importedAssets)
                TryRegister(settings, assetPath);

            foreach (var assetPath in deletedAssets)
                TryUnregister(settings, assetPath);

            for (int i = 0; i < movedAssets.Length; i++)
            {
                TryUnregister(settings, movedFromAssetPaths[i]);
                TryRegister(settings, movedAssets[i]);
            }
        }

        private static void TryRegister(AddressableAssetSettings settings, string assetPath)
        {
            if (!assetPath.StartsWith(WATCHED_FOLDER)) return;
            if (assetPath.EndsWith(".meta")) return;
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            var group = GetOrCreateGroup(settings);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var address = GetAddress(assetPath);

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address;

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }

        private static void TryUnregister(AddressableAssetSettings settings, string assetPath)
        {
            if (!assetPath.StartsWith(WATCHED_FOLDER)) return;
            if (assetPath.EndsWith(".meta")) return;

            var group = settings.FindGroup(GROUP_NAME);
            if (group == null) return;

            // 削除済みアセットは AssetPathToGUID が空を返すため、アドレスで逆引きして削除する
            var address = GetAddress(assetPath);
            var entry = group.entries.FirstOrDefault(e => e.address == address);
            if (entry == null) return;

            settings.RemoveAssetEntry(entry.guid);
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings)
        {
            var group = settings.FindGroup(GROUP_NAME);
            if (group != null) return group;

            return settings.CreateGroup(
                GROUP_NAME,
                setAsDefaultGroup: false,
                readOnly: false,
                postEvent: false,
                schemasToCopy: settings.DefaultGroup.Schemas);
        }

        /// <summary>
        /// 相対パスでのアドレスを取得
        /// </summary>
        /// <param name="assetPath">ルートディレクトリからの相対パス</param>
        private static string GetAddress(string assetPath)
        {
            var relativePath = assetPath.Substring(WATCHED_FOLDER.Length).TrimStart('/');
            return Path.ChangeExtension(relativePath, null);
        }
    }
}