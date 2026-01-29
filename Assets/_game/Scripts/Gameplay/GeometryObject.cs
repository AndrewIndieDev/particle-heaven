using AndrewDowsett.CommonObservers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class GeometryObject : MonoBehaviour, IUpdateObserver
{
    public Vector2 Position { get => _trans.anchoredPosition; }
    public Vector2 Size { get => _trans.sizeDelta; }
    public Vector2 Direction { get; private set; }

    [SerializeField] private GameObject _deathVFX;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private RectTransform _trans;
    [SerializeField] private Image _visual;
    [SerializeField] private MMF_Player _onHitFeedbacks;
    [SerializeField] private UIBar _healthBar;
    [SerializeField] private float _baseHealth = 2f;
    [SerializeField] private OreObject _orePrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip _destroyClip;
    [SerializeField] private AudioClip _hitClip;

    private float rotationSpeed;
    private bool isDestroying;

    private void Start()
    {
        SetPosition(GetRandomScreenEdgePosition());
        SetRandomRotation();
        SetDirection();
        SetupHealthBar();

        GeometryManager.Instance.AddObject(this);
        UpdateManager.RegisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        Move(deltaTime);
        Rotate(deltaTime);
    }

    public void OnHit(float damage)
    {
        AudioManager.PlaySound(_hitClip, EMixerGroup.SFX, Random.Range(0.5f, 0.7f), 0.5f);
        _onHitFeedbacks.PlayFeedbacks();
        _healthBar.RemoveValue(damage);
        if (!isDestroying && _healthBar.Value <= 0)
        {
            isDestroying = true;
            Destroy();
        }
    }

    private void Move(float deltaTime)
    {
        SetPosition(_trans.anchoredPosition + (Direction * deltaTime * _speed));
        if (_trans.anchoredPosition.x > 110 || _trans.anchoredPosition.x < -110 || _trans.anchoredPosition.y > 70 || _trans.anchoredPosition.y < -70)
            DestroyImmediate();
    }

    private void Rotate(float deltaTime)
    {
        _visual.transform.Rotate(Vector3.forward, rotationSpeed * deltaTime);
    }

    private void SetPosition(Vector2 position)
    {
        _trans.anchoredPosition = position;
    }

    private void SetRandomRotation()
    {
        _visual.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 360));
        rotationSpeed = Random.Range(-5f, 5f);
    }

    private void SetDirection()
    {
        Direction = (new Vector2(Random.Range(-30f, 30f), Random.Range(-20f, 20f)) - Position).normalized;
        _visual.transform.localScale = new Vector3(Direction.x > 0 ? -1 : 1, 1, 1);
    }

    private void SetupHealthBar()
    {
        _healthBar.Init(_baseHealth);
    }

    private Vector2 GetRandomScreenEdgePosition()
    {
        RectTransform rect = transform.GetComponentInParent<RectTransform>();
        float width = Random.Range(-100f, 100f), height = Random.Range(-60f, 60f);
        int rand = Random.Range(0, 4);
        switch (rand)
        {
            case 0: // top
                height = 60f;
                break;
            case 1: // bottom
                height = -60f;
                break;
            case 2: // left
                width = -100f;
                break;
            case 3: // right
                width = 100f;
                break;
        }
        return new Vector2(width, height);
    }

    /// <summary>
    /// Used when you want the geometry to break apart.
    /// </summary>
    public void Destroy()
    {
        UpdateManager.UnregisterObserver(this);
        GeometryManager.Instance.RemoveObject(this);
        AudioManager.PlaySound(_destroyClip, EMixerGroup.SFX, volume: 1f);

        Instantiate(_deathVFX, transform.position, Quaternion.identity);
        
        OreObject ore = Instantiate(_orePrefab, transform.parent);
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
        Vector3 direction = (spawnPosition - transform.position).normalized;
        ore.Init(spawnPosition, direction);

        UIBar.GetByName("fuel").AddValue(1f);

        Destroy(gameObject);
    }

    /// <summary>
    /// Used when you want the geometry to disappear without breaking apart.
    /// </summary>
    public void DestroyImmediate()
    {
        UpdateManager.UnregisterObserver(this);
        GeometryManager.Instance.RemoveObject(this);
        Destroy(gameObject);
    }
}
