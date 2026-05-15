using System.Threading;
using Cysharp.Threading.Tasks;
using ANFW.Input;
using ANFW.SaveData;
using ANFW.Scene;
using ANFW.Sound;
using ANFW.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ANFW
{
    public class GameLauncher : MonoBehaviour
    {
        private static GameLauncher _instance;

        [SerializeField] private int _maxSECount = 8;
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private Canvas _uiCanvas;

        public static SaveDataManager SaveDataManager { get; private set; }
        public static StateMachine<IState> StateMachine { get; private set; }
        public static SoundManager SoundManager { get; private set; }
        public static GameSceneManager GameSceneManager { get; private set; }
        public static InputManager InputManager { get; private set; }
        public static UIManager UIManager { get; private set; }

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

            SaveDataManager = new SaveDataManager();
            await SaveDataManager.InitializeAsync(ct);

            await AddressablesLoader.InitializeAsync(ct);

            var soundRoot = new GameObject("Sound");
            soundRoot.transform.SetParent(transform);

            var seSources = new AudioSource[_maxSECount];
            for (var i = 0; i < _maxSECount; i++)
                seSources[i] = CreateAudioSource($"se_{i}", soundRoot.transform);

            SoundManager = new SoundManager();
            await SoundManager.InitializeAsync(CreateAudioSource("bgm", soundRoot.transform), seSources, ct);

            StateMachine = new StateMachine<IState>();

            GameSceneManager = new GameSceneManager();
            await GameSceneManager.InitializeAsync(ct);

            InputManager = new InputManager();
            await InputManager.InitializeAsync(_inputActionAsset, ct);

            UIManager = new UIManager();
            await UIManager.InitializeAsync(_uiCanvas != null ? _uiCanvas.transform : null, ct);

            ANFWLogger.Log("GameLauncher: Initialization completed");
        }

        private AudioSource CreateAudioSource(string sourceName, Transform parent = null)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(parent ?? transform);
            return go.AddComponent<AudioSource>();
        }
    }
}
