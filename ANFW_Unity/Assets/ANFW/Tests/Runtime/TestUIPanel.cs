using System.Threading;
using Cysharp.Threading.Tasks;
using ANFW.UI;
using UnityEngine;

namespace ANFW.Tests
{
    public class TestUIPanel : MonoBehaviour, IUIPanel
    {
        public bool EnteredCalled { get; private set; }
        public bool ExitedCalled { get; private set; }
        public bool SuspendedCalled { get; private set; }
        public bool ResumedCalled { get; private set; }

        public UniTask OnEnterAsync(CancellationToken ct) { EnteredCalled = true; return UniTask.CompletedTask; }
        public UniTask OnExitAsync(CancellationToken ct) { ExitedCalled = true; return UniTask.CompletedTask; }
        public UniTask OnSuspendAsync(CancellationToken ct) { SuspendedCalled = true; return UniTask.CompletedTask; }
        public UniTask OnResumeAsync(CancellationToken ct) { ResumedCalled = true; return UniTask.CompletedTask; }
    }
}
