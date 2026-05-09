using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using ANFW.Scene;

namespace ANFW.Tests
{
    public class GameSceneManagerTests
    {
        private const string SCENE_A = "TestSceneA";
        private const string SCENE_B = "TestSceneB";

        private GameSceneManager _manager;
        private CancellationTokenSource _cts;

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            _cts = new CancellationTokenSource();
            _manager = new GameSceneManager();
            await _manager.InitializeAsync(_cts.Token);
        });

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            _cts.Cancel();
            _cts.Dispose();
        }

        /// <summary>
        /// Single ロード後に ActiveSceneName が更新されること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadScene_UpdatesActiveSceneName() => UniTask.ToCoroutine(async () =>
        {
            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);

            Assert.AreEqual(SCENE_A, _manager.ActiveSceneName);
        });

        /// <summary>
        /// Additive ロード後に AdditiveScenes にシーン名が追加されること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadSceneAdditive_AddsToAdditiveScenes() => UniTask.ToCoroutine(async () =>
        {
            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);
            await _manager.LoadSceneAdditiveAsync(SCENE_B, ct: _cts.Token);

            Assert.IsTrue(_manager.AdditiveScenes.Contains(SCENE_B));
        });

        /// <summary>
        /// UnloadSceneAsync 後に AdditiveScenes からシーン名が削除されること
        /// </summary>
        [UnityTest]
        public IEnumerator UnloadScene_RemovesFromAdditiveScenes() => UniTask.ToCoroutine(async () =>
        {
            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);
            await _manager.LoadSceneAdditiveAsync(SCENE_B, ct: _cts.Token);

            await _manager.UnloadSceneAsync(SCENE_B, _cts.Token);

            Assert.IsFalse(_manager.AdditiveScenes.Contains(SCENE_B));
        });

        /// <summary>
        /// Single ロードで Additive シーンがすべてクリアされること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadScene_Single_ClearsAdditiveScenes() => UniTask.ToCoroutine(async () =>
        {
            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);
            await _manager.LoadSceneAdditiveAsync(SCENE_B, ct: _cts.Token);

            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);

            Assert.AreEqual(0, _manager.AdditiveScenes.Count);
        });

        /// <summary>
        /// ロード完了後に SceneLoadedEvent が発火されること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadScene_EmitsSceneLoadedEvent() => UniTask.ToCoroutine(async () =>
        {
            var loaded = false;
            using var sub = EventBus.Subscribe<SceneLoadedEvent>(_ => loaded = true);

            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);

            Assert.IsTrue(loaded);
        });

        /// <summary>
        /// アンロード完了後に SceneUnloadedEvent が発火されること
        /// </summary>
        [UnityTest]
        public IEnumerator UnloadScene_EmitsSceneUnloadedEvent() => UniTask.ToCoroutine(async () =>
        {
            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);
            await _manager.LoadSceneAdditiveAsync(SCENE_B, ct: _cts.Token);

            var unloaded = false;
            using var sub = EventBus.Subscribe<SceneUnloadedEvent>(_ => unloaded = true);

            await _manager.UnloadSceneAsync(SCENE_B, _cts.Token);

            Assert.IsTrue(unloaded);
        });

        /// <summary>
        /// EventBus 経由の LoadSceneEvent でシーンがロードされること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadSceneViaEventBus_LoadsScene() => UniTask.ToCoroutine(async () =>
        {
            var loaded = false;
            using var sub = EventBus.Subscribe<SceneLoadedEvent>(_ => loaded = true);

            EventBus.Emit(new LoadSceneEvent { SceneName = SCENE_A, Additive = false });

            await UniTask.WaitUntil(() => loaded, cancellationToken: _cts.Token);

            Assert.IsTrue(loaded);
        });

        /// <summary>
        /// ロード完了後に IsLoading が false になること、かつ SceneLoadedEvent 受信時には false であること
        /// </summary>
        [UnityTest]
        public IEnumerator LoadScene_IsLoadingFalseOnCompletion() => UniTask.ToCoroutine(async () =>
        {
            var isLoadingDuringEvent = true;
            using var sub = EventBus.Subscribe<SceneLoadedEvent>(_ =>
                isLoadingDuringEvent = _manager.IsLoading);

            await _manager.LoadSceneAsync(SCENE_A, ct: _cts.Token);

            Assert.IsFalse(isLoadingDuringEvent);
            Assert.IsFalse(_manager.IsLoading);
        });
    }
}
