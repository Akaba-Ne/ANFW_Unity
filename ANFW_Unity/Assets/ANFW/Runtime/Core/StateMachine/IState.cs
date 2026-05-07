using System.Threading;
using Cysharp.Threading.Tasks;

namespace ANFW
{
    public interface IState
    {
        /// <summary>
        /// 状態がスタックの先頭に積まれたときに呼ばれる
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        UniTask OnEnterAsync(CancellationToken ct);

        /// <summary>
        /// 状態がスタックから完全に取り除かれたときに呼ばれる
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        UniTask OnExitAsync(CancellationToken ct);

        /// <summary>
        /// Push により自分の上に別の状態が積まれたときに呼ばれる
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        UniTask OnSuspendAsync(CancellationToken ct);

        /// <summary>
        /// Pop により自分が再びスタックの先頭に戻ったときに呼ばれる
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        UniTask OnResumeAsync(CancellationToken ct);
    }
}
