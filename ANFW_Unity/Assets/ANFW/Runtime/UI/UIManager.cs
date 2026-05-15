using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ANFW.UI
{
    public class UIManager
    {
        private Transform _canvasRoot;
        private readonly Stack<(string address, GameObject instance, GameObject prefab, IUIPanel panel)> _panelStack = new();
        private readonly List<IDisposable> _subscriptions = new();
        private CancellationToken _ct;

        public int PanelCount => _panelStack.Count;

        /// <summary>
        /// UIManager を初期化して EventBus の購読を開始する
        /// </summary>
        /// <param name="canvasRoot">パネルを生成する親 Transform（Canvas の Transform）</param>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask InitializeAsync(Transform canvasRoot, CancellationToken ct)
        {
            _canvasRoot = canvasRoot;
            _ct = ct;

            _subscriptions.Add(EventBus.Subscribe<OpenPanelEvent>(e => PushPanelAsync(e.Address, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<ClosePanelEvent>(_ => PopPanelAsync(_ct).Forget()));

            ANFWLogger.Log("UIManager: Initialized");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 購読をすべて解除してすべてのパネルを破棄・解放する
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();

            while (_panelStack.Count > 0)
            {
                var entry = _panelStack.Pop();
                UnityEngine.Object.Destroy(entry.instance);
                AddressablesLoader.Release(entry.prefab);
            }
        }

        /// <summary>
        /// 指定したアドレスのパネル Prefab を非同期でロードしてインスタンス化し、スタックに積む。
        /// 現在のパネルがあれば OnSuspendAsync を呼んでから新しいパネルを表示する
        /// </summary>
        /// <param name="address">パネル Prefab のアドレスキー</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PushPanelAsync(string address, CancellationToken ct)
        {
            if (_canvasRoot == null) return;

            if (_panelStack.TryPeek(out var current) && current.panel != null)
                await current.panel.OnSuspendAsync(ct);

            var prefab = await AddressablesLoader.LoadAsync<GameObject>(address, ct);
            if (prefab == null) return;

            var instance = UnityEngine.Object.Instantiate(prefab, _canvasRoot);
            instance.TryGetComponent<IUIPanel>(out var panel);

            try
            {
                if (panel != null) await panel.OnEnterAsync(ct);
            }
            catch
            {
                UnityEngine.Object.Destroy(instance);
                AddressablesLoader.Release(prefab);
                throw;
            }

            _panelStack.Push((address, instance, prefab, panel));
            ANFWLogger.Log($"UIManager: Pushed panel '{address}' (stack depth: {_panelStack.Count})");
        }

        /// <summary>
        /// スタック最上位のパネルを取り出して破棄・解放する。
        /// 下のパネルがあれば OnResumeAsync を呼ぶ
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PopPanelAsync(CancellationToken ct)
        {
            if (_panelStack.Count == 0) return;

            var (address, instance, prefab, panel) = _panelStack.Pop();

            if (panel != null) await panel.OnExitAsync(ct);

            UnityEngine.Object.Destroy(instance);
            AddressablesLoader.Release(prefab);

            ANFWLogger.Log($"UIManager: Popped panel '{address}' (stack depth: {_panelStack.Count})");

            if (_panelStack.TryPeek(out var previous) && previous.panel != null)
                await previous.panel.OnResumeAsync(ct);
        }
    }
}
