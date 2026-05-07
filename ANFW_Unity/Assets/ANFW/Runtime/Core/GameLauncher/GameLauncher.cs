using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ANFW
{
    public class GameLauncher : MonoBehaviour
    {
        private static GameLauncher _instance;

        public static StateMachine<IState> StateMachine { get; private set; }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAsync(destroyCancellationToken).Forget();
        }

        private async UniTask InitializeAsync(CancellationToken ct)
        {
            ANFWLogger.Log("GameLauncher: Initialization started");

            await AddressablesLoader.InitializeAsync(ct);

            // 各 Manager の初期化をここに追加していく
            // await SoundManager.InitializeAsync(ct);
            // await GameSceneManager.InitializeAsync(ct);

            ANFWLogger.Log("GameLauncher: Initialization completed");
        }
    }
}
