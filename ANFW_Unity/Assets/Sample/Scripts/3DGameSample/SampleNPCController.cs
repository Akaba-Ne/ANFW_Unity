using Cysharp.Threading.Tasks;
using ANFW;
using UnityEngine;

public class SampleNPCController : MonoBehaviour
{
    [SerializeField] private float _angularSpeed = 1f;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private string _footstepAddress = "Footstep";
    [SerializeField] private float _footstepInterval = 0.5f;

    private AudioSource _footstepSource;
    private AudioClip _footstepClip;
    private Vector3 _center;
    private float _angle;
    private float _footstepTimer;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameLauncher.SoundManager != null,
            cancellationToken: destroyCancellationToken);

        _center = transform.position;
        _footstepSource = GameLauncher.SoundManager.CreatePositionalSource(transform);
        _footstepClip = await AddressablesLoader.LoadAsync<AudioClip>(_footstepAddress, destroyCancellationToken);
    }

    private void Update()
    {
        if (GameLauncher.SoundManager == null) return;

        _angle += _angularSpeed * Time.deltaTime;
        var nextPos = new Vector3(
            _center.x + Mathf.Cos(_angle) * _radius,
            _center.y,
            _center.z + Mathf.Sin(_angle) * _radius);

        var direction = (nextPos - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.forward = direction;

        transform.position = nextPos;

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f && _footstepClip != null)
        {
            _footstepSource.PlayOneShot(_footstepClip);
            _footstepTimer = _footstepInterval;
        }
    }

    private void OnDestroy()
    {
        if (_footstepSource != null)
            GameLauncher.SoundManager.ReleasePositionalSource(_footstepSource);
    }
}
