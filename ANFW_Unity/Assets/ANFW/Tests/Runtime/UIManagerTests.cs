// 事前準備: 以下のアドレスで Prefab を AddressablesResources/UI/ に配置すること
//   UI/TestPanel               … 空の GameObject Prefab（IUIPanel 未実装）
//   UI/TestPanelWithLifecycle  … TestUIPanel コンポーネントをアタッチした Prefab
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ANFW.UI;

namespace ANFW.Tests
{
    public class UIManagerTests
    {
        private const string PANEL_SIMPLE = "Test/UI/TestPanel";
        private const string PANEL_WITH_LIFECYCLE = "Test/UI/TestPanelWithLifecycle";

        private UIManager _manager;
        private GameObject _uiRoot;
        private CancellationTokenSource _cts;

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            _cts = new CancellationTokenSource();
            _uiRoot = new GameObject("UIRoot");
            _manager = new UIManager();
            await _manager.InitializeAsync(_uiRoot.transform, _cts.Token);
        });

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            _cts.Cancel();
            _cts.Dispose();
            Object.Destroy(_uiRoot);
        }

        [UnityTest]
        public IEnumerator PopPanel_WhenEmpty_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PopPanelAsync(_cts.Token);
        });

        [UnityTest]
        public IEnumerator PushPanel_StackDepthIsOne() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PushPanelAsync(PANEL_SIMPLE, _cts.Token);

            Assert.AreEqual(1, _manager.PanelCount);
        });

        [UnityTest]
        public IEnumerator PopPanel_StackDepthIsZero() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PushPanelAsync(PANEL_SIMPLE, _cts.Token);
            await _manager.PopPanelAsync(_cts.Token);

            Assert.AreEqual(0, _manager.PanelCount);
        });

        [UnityTest]
        public IEnumerator PushTwice_SuspendCalledOnFirst() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PushPanelAsync(PANEL_WITH_LIFECYCLE, _cts.Token);
            var panel1 = Object.FindFirstObjectByType<TestUIPanel>();

            await _manager.PushPanelAsync(PANEL_SIMPLE, _cts.Token);

            Assert.IsTrue(panel1.SuspendedCalled);
        });

        [UnityTest]
        public IEnumerator Pop_ResumeCalledOnPrevious() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PushPanelAsync(PANEL_WITH_LIFECYCLE, _cts.Token);
            var panel1 = Object.FindFirstObjectByType<TestUIPanel>();

            await _manager.PushPanelAsync(PANEL_SIMPLE, _cts.Token);
            await _manager.PopPanelAsync(_cts.Token);

            Assert.IsTrue(panel1.ResumedCalled);
        });

        [UnityTest]
        public IEnumerator OpenPanelEvent_CallsPushPanel() => UniTask.ToCoroutine(async () =>
        {
            EventBus.Emit(new OpenPanelEvent { Address = PANEL_SIMPLE });
            await UniTask.WaitUntil(() => _manager.PanelCount == 1, cancellationToken: _cts.Token);

            Assert.AreEqual(1, _manager.PanelCount);
        });

        [UnityTest]
        public IEnumerator ClosePanelEvent_CallsPopPanel() => UniTask.ToCoroutine(async () =>
        {
            await _manager.PushPanelAsync(PANEL_SIMPLE, _cts.Token);

            EventBus.Emit(new ClosePanelEvent());
            await UniTask.WaitUntil(() => _manager.PanelCount == 0, cancellationToken: _cts.Token);

            Assert.AreEqual(0, _manager.PanelCount);
        });

        [Test]
        public void Dispose_StopsEventBusHandlers()
        {
            _manager.Dispose();

            EventBus.Emit(new OpenPanelEvent { Address = PANEL_SIMPLE });

            Assert.AreEqual(0, _manager.PanelCount);
        }
    }
}
