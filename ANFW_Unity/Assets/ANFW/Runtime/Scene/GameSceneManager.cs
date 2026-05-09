using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace ANFW.Scene
{
    public class GameSceneManager
    {
        private readonly HashSet<string> _additiveScenes = new();
        private readonly List<IDisposable> _subscriptions = new();
        private CancellationToken _ct;

        /// <summary>
        /// 現在アクティブなシーン名
        /// </summary>
        public string ActiveSceneName { get; private set; }

        /// <summary>
        /// シーンのロード・アンロード処理が進行中かどうか
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// 現在ロード中の Additive シーン名の一覧
        /// </summary>
        public IReadOnlyCollection<string> AdditiveScenes => _additiveScenes;

        /// <summary>
        /// GameSceneManager を初期化して EventBus の購読を開始する
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask InitializeAsync(CancellationToken ct)
        {
            _ct = ct;
            ActiveSceneName = SceneManager.GetActiveScene().name;

            _subscriptions.Add(EventBus.Subscribe<LoadSceneEvent>(e =>
                LoadInternalAsync(e.SceneName, e.Additive, null, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<UnloadSceneEvent>(e =>
                UnloadSceneAsync(e.SceneName, _ct).Forget()));

            ANFWLogger.Log("GameSceneManager: Initialized");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 購読をすべて解除する
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();
        }

        /// <summary>
        /// 指定したシーンを Single モードで非同期ロードする。現在ロード中のシーンはすべて破棄される
        /// </summary>
        /// <param name="sceneName">ロードするシーン名</param>
        /// <param name="progress">進捗コールバック（0.0 〜 1.0）</param>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask LoadSceneAsync(string sceneName, IProgress<float> progress = null, CancellationToken ct = default)
            => LoadInternalAsync(sceneName, false, progress, ct);

        /// <summary>
        /// 指定したシーンを Additive モードで非同期ロードする。現在のシーンはそのまま残る
        /// </summary>
        /// <param name="sceneName">ロードするシーン名</param>
        /// <param name="progress">進捗コールバック（0.0 〜 1.0）</param>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask LoadSceneAdditiveAsync(string sceneName, IProgress<float> progress = null, CancellationToken ct = default)
            => LoadInternalAsync(sceneName, true, progress, ct);

        /// <summary>
        /// Additive でロードしたシーンを非同期でアンロードする
        /// </summary>
        /// <param name="sceneName">アンロードするシーン名</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            if (!_additiveScenes.Contains(sceneName))
            {
                ANFWLogger.LogWarning($"GameSceneManager: '{sceneName}' is not loaded additively");
                return;
            }

            IsLoading = true;
            try
            {
                await SceneManager.UnloadSceneAsync(sceneName).ToUniTask(cancellationToken: ct);
                _additiveScenes.Remove(sceneName);
            }
            finally
            {
                IsLoading = false;
            }

            EventBus.Emit(new SceneUnloadedEvent { SceneName = sceneName });
            ANFWLogger.Log($"GameSceneManager: Unloaded '{sceneName}'");
        }

        private async UniTask LoadInternalAsync(string sceneName, bool additive, IProgress<float> progress, CancellationToken ct)
        {
            IsLoading = true;
            try
            {
                var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
                await SceneManager.LoadSceneAsync(sceneName, mode).ToUniTask(progress: progress, cancellationToken: ct);

                if (additive)
                    _additiveScenes.Add(sceneName);
                else
                {
                    _additiveScenes.Clear();
                    ActiveSceneName = sceneName;
                }
            }
            finally
            {
                IsLoading = false;
            }

            EventBus.Emit(new SceneLoadedEvent { SceneName = sceneName });
            ANFWLogger.Log($"GameSceneManager: Loaded '{sceneName}' (additive: {additive})");
        }
    }
}
