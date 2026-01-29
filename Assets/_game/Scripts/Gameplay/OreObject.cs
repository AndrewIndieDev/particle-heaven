using AndrewDowsett.CommonObservers;
using UnityEngine;
using System.Collections;

public class OreObject : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _speed = 2f;

    [Header("Pickup Settings")]
    [SerializeField] private AnimationCurve _pickupCurve;
    [SerializeField] private float _pickupDuration = 0.5f;
    [SerializeField] private float _pickupKickbackDistance = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip _pickupClip;

    private float rotationSpeed;
    private Vector2 direction;
    private bool isBeingPickedUp;

    public void Init(Vector3 position, Vector2 direction)
    {
        transform.position = position;
        this.direction = direction;
        _speed *= Random.Range(0.8f, 1.2f);
        rotationSpeed = Random.Range(-2f, 2f);
        UpdateManager.RegisterObserver(this);
    }

    public void Pickup(Transform player)
    {
        if (isBeingPickedUp)
            return;

        AudioManager.PlaySound(_pickupClip, EMixerGroup.SFX, Random.Range(0.4f, 0.6f), 0.5f);

        UIBar.GetByName("fuel").AddValue(0.1f);

        GameManager.Instance.AddOre();

        isBeingPickedUp = true;

        UpdateManager.UnregisterObserver(this);
        _collider.enabled = false;

        StartCoroutine(PickupRoutine(player));
    }

    private IEnumerator PickupRoutine(Transform player)
    {
        Vector3 startPosition = transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < _pickupDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / _pickupDuration);
            Vector3 awayFromPlayer = (startPosition - player.position).normalized;
            float posT = _pickupCurve.Evaluate(t);
            Vector3 pullPosition = Vector3.LerpUnclamped(
                startPosition,
                player.position,
                posT
            );
            float kickAmount = Mathf.Clamp01(-posT);
            Vector3 kickOffset = awayFromPlayer * _pickupKickbackDistance * kickAmount;

            transform.position = pullPosition + kickOffset;

            yield return null;
        }

        // Snap cleanly to player at the end
        transform.position = player.position;

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        if (isBeingPickedUp)
            return;

        transform.position += (Vector3)direction * _speed * deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Pickup(collision.transform);
        }
    }
}
