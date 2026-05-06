using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ANFW
{
    public static class AddressablesLoader
    {
        /// <summary>
        /// Addressables システムを初期化する
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public static async UniTask InitializeAsync(CancellationToken ct)
        {
            var handle = Addressables.InitializeAsync();
            await handle.ToUniTask(cancellationToken: ct);
            ANFWLogger.Log("AddressablesLoader: Initialized");
        }

        /// <summary>
        /// 指定したアドレスのアセットを非同期でロードする
        /// </summary>
        /// <param name="address">アセットのアドレスキー</param>
        /// <param name="ct">キャンセルトークン</param>
        public static async UniTask<T> LoadAsync<T>(string address, CancellationToken ct) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                ANFWLogger.LogError($"AddressablesLoader: Failed to load '{address}'");
                return null;
            }

            return handle.Result;
        }

        /// <summary>
        /// ロード済みアセットを解放する
        /// </summary>
        /// <param name="asset">解放するアセット</param>
        public static void Release<T>(T asset) where T : Object
        {
            Addressables.Release(asset);
        }
    }
}
