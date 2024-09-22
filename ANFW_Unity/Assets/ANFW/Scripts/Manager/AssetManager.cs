using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetManager : SingletonMonoBehaviour<AssetManager>
{
    /// <summary>
    /// リソースの取得
    /// </summary>
    /// <typeparam name="T">リソースの型</typeparam>
    /// <param name="key">Addressables のアドレス</param>
    /// <returns>リソース</returns>
    public async Task<T> getResource<T>(string key)
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            T asset = handle.Result;
            Addressables.Release(handle);
            return asset;
        }
        // TODO:もっと良い方法がないか探る
        return default(T);
    }
}
