using System;
using Cysharp.Threading.Tasks;
using ANFW;
using UnityEngine;
using UnityEngine.InputSystem;

public class SamplePlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private string _footstepAddress = "Footstep";
    [SerializeField] private float _footstepInterval = 0.4f;

    private AudioSource _footstepSource;
    private AudioClip _footstepClip;
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private float _footstepTimer;

    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameLauncher.InputManager != null,
            cancellationToken: destroyCancellationToken);

        GameLauncher.InputManager.EnableActionMap("Player");

        _moveAction = GameLauncher.InputManager.FindAction("Move");
        _onMovePerformed = ctx => _moveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = _ => _moveInput = Vector2.zero;
        _moveAction.performed += _onMovePerformed;
        _moveAction.canceled += _onMoveCanceled;

        _footstepSource = GameLauncher.SoundManager.CreatePositionalSource(transform);
        _footstepClip = await AddressablesLoader.LoadAsync<AudioClip>(_footstepAddress, destroyCancellationToken);
    }

    private void Update()
    {
        if (_moveInput == Vector2.zero) return;

        var move = new Vector3(_moveInput.x, 0f, _moveInput.y) * (_speed * Time.deltaTime);
        transform.Translate(move, Space.World);

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f && _footstepClip != null)
        {
            _footstepSource.PlayOneShot(_footstepClip);
            _footstepTimer = _footstepInterval;
        }
    }

    private void OnDestroy()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= _onMovePerformed;
            _moveAction.canceled -= _onMoveCanceled;
        }

        if (_footstepSource != null)
            GameLauncher.SoundManager.ReleasePositionalSource(_footstepSource);
    }
}
