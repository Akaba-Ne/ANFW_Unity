using System.Threading;
using Cysharp.Threading.Tasks;

namespace ANFW.UI
{
    public interface IUIPanel
    {
        UniTask OnEnterAsync(CancellationToken ct);
        UniTask OnExitAsync(CancellationToken ct);
        UniTask OnSuspendAsync(CancellationToken ct);
        UniTask OnResumeAsync(CancellationToken ct);
    }
}
