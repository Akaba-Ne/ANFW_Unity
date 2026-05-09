using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ANFW.Input
{
    public class InputManager
    {
        private InputActionAsset _asset;

        /// <summary>
        /// InputActionAsset を受け取り有効化する。asset が null の場合は何もしない
        /// </summary>
        /// <param name="asset">ゲームプロジェクト側で用意した InputActionAsset</param>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask InitializeAsync(InputActionAsset asset, CancellationToken ct)
        {
            _asset = asset;
            _asset?.Enable();

            ANFWLogger.Log("InputManager: Initialized");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 購読をすべて解除してアセットを無効化する
        /// </summary>
        public void Dispose()
        {
            _asset?.Disable();
            _asset = null;
        }

        /// <summary>
        /// 指定した ActionMap を有効化し、他の ActionMap をすべて無効化する
        /// </summary>
        /// <param name="mapName">有効化する ActionMap の名前</param>
        public void EnableActionMap(string mapName)
        {
            if (_asset == null) return;

            foreach (var map in _asset.actionMaps)
                map.Disable();

            var target = _asset.FindActionMap(mapName, throwIfNotFound: true);
            target.Enable();
        }

        /// <summary>
        /// すべての ActionMap を無効化する
        /// </summary>
        public void DisableAllActionMaps()
        {
            if (_asset == null) return;

            foreach (var map in _asset.actionMaps)
                map.Disable();
        }

        /// <summary>
        /// 名前または ID で InputAction を検索して返す
        /// </summary>
        /// <param name="actionNameOrId">アクション名または ID</param>
        /// <param name="throwIfNotFound">見つからない場合に例外をスローするか</param>
        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return _asset?.FindAction(actionNameOrId, throwIfNotFound);
        }
    }
}
