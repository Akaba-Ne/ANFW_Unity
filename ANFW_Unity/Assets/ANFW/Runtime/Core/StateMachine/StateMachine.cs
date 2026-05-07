using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ANFW
{
    public class StateMachine<TState> where TState : IState
    {
        private readonly Stack<TState> _stateStack = new();

        public TState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : default;

        /// <summary>
        /// スタックをすべてクリアして指定した状態に遷移する
        /// </summary>
        /// <param name="nextState">遷移先の状態</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask ChangeStateAsync(TState nextState, CancellationToken ct)
        {
            while (_stateStack.Count > 0)
                await _stateStack.Pop().OnExitAsync(ct);

            _stateStack.Push(nextState);
            await nextState.OnEnterAsync(ct);
        }

        /// <summary>
        /// 現在の状態をスタックに残したまま新しい状態をプッシュする
        /// </summary>
        /// <param name="nextState">プッシュする状態</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PushStateAsync(TState nextState, CancellationToken ct)
        {
            if (_stateStack.Count > 0)
                await _stateStack.Peek().OnSuspendAsync(ct);

            _stateStack.Push(nextState);
            await nextState.OnEnterAsync(ct);
        }

        /// <summary>
        /// 現在の状態をポップして前の状態に戻る
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PopStateAsync(CancellationToken ct)
        {
            if (_stateStack.Count == 0) return;

            await _stateStack.Pop().OnExitAsync(ct);

            if (_stateStack.Count > 0)
                await _stateStack.Peek().OnResumeAsync(ct);
        }
    }
}
