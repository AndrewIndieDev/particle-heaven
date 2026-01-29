using System.Collections;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    public static PlayerObject Instance;
    private void Awake() => Instance = this;

    [SerializeField] private Transform _visual;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GameObject _deathVFX;

    [Header("Movement")]
    [SerializeField] private float _forwardSpeed = 6f;
    [SerializeField] private float _acceleration = 8f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private float _stopDistance = 1.0f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 180f;

    [Header("Shooting")]
    [SerializeField] private float _shotDelay = 3f;
    [SerializeField] private Gun[] _guns;

    [Header("Audio")]
    [SerializeField] private AudioClip deathClip;

    private float currentForwardSpeed;
    private float shotTimer;
    private bool isEnabled;

    public void Enable()
    {
        StartCoroutine(DelayedEnable());
    }

    public void Disable()
    {
        StartCoroutine(DelayedDisable());
    }

    private void Update()
    {
        if (!isEnabled)
            return;

        #region Forward Movement
        Transform cursor = CursorScript.Instance.transform;
        Vector2 toTarget = cursor.position - transform.position;
        float distance = toTarget.magnitude;
        bool shouldMove = isEnabled && distance > _stopDistance;
        float targetSpeed = shouldMove ? _forwardSpeed : 0f;
        float accel = shouldMove ? _acceleration : _deceleration;

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            targetSpeed,
            accel * Time.deltaTime
        );
        transform.position += transform.right * currentForwardSpeed * Time.deltaTime;
        #endregion

        #region Rotation Movement
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
        #endregion

        #region Shooting
        shotTimer += Time.deltaTime;
        if (shotTimer >= _shotDelay)
        {
            shotTimer -= _shotDelay;
            foreach (Gun gun in _guns)
            {
                gun.Fire();
            }
        }
        #endregion
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Geometry")
        {
            StartCoroutine(DeathAnimation());
        }
    }

    private IEnumerator DelayedEnable()
    {
        _visual.gameObject.SetActive(false);

        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        transform.localPosition = Vector3.zero;
        _visual.localPosition = new Vector3(-1000, 0, 0);
        for (int i = 0; i < _guns.Length; i++)
        {
            _guns[i].transform.rotation = Quaternion.identity;
        }

        yield return null;

        _visual.gameObject.SetActive(true);

        while (_visual.localPosition.x < -1f)
        {
            _visual.localPosition = Vector3.Lerp(
                _visual.localPosition,
                Vector3.zero,
                0.05f
            );
            yield return null;
        }

        _visual.localPosition = Vector3.zero;
        isEnabled = true;
        _collider.enabled = isEnabled;
    }

    private IEnumerator DelayedDisable()
    {
        isEnabled = false;
        _collider.enabled = isEnabled;
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator DeathAnimation()
    {
        isEnabled = false;
        _collider.enabled = isEnabled;
        _visual.gameObject.SetActive(false);
        Instantiate(_deathVFX, transform.position, Quaternion.identity);
        AudioManager.PlaySound(deathClip, EMixerGroup.SFX);

        yield return new WaitForSeconds(2f);

        GameManager.Instance.StopGame();
    }
}
