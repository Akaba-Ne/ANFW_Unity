using UnityEngine;

public class SampleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -8f);
    [SerializeField] private float _smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (_target == null) return;

        var targetPosition = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
        transform.LookAt(_target);
    }
}
