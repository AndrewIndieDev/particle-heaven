using AndrewDowsett.CommonObservers;
using MoreMountains.Feedbacks;
using UnityEngine;

public class GeometryObject : MonoBehaviour, IUpdateObserver
{
    public Vector2 Position { get => rect.anchoredPosition; }
    public Vector2 Size { get => rect.sizeDelta; }
    public Vector2 Direction { get; private set; }
    public float Speed { get; private set; }

    [SerializeField] private RectTransform rect;
    [SerializeField] private new BoxCollider2D collider;
    [SerializeField] private MMF_Player onHitFeedbacks;
    [SerializeField] private UIBar healthBar;

    private void Start()
    {
        SetSize();
        SetSpeedBasedOnSize();
        SetPosition(GetRandomScreenEdgePosition());
        SetRandomRotation();
        SetDirection();

        healthBar.SetMaxValue(Size.x * 50f);
        healthBar.SetValue(Size.x * 50f);

        GeometryManager.Instance.AddObject(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        Move(deltaTime);
    }

    public void OnHit(float damage)
    {
        onHitFeedbacks.PlayFeedbacks();
        healthBar.RemoveValue(damage);
        if (healthBar.Value <= 0)
            DestroyWithPoints();
    }

    public void Move(float deltaTime)
    {
        SetPosition(rect.anchoredPosition + (Direction * deltaTime * Speed));
        if (rect.anchoredPosition.x > 110 || rect.anchoredPosition.x < -110 || rect.anchoredPosition.y > 70 || rect.anchoredPosition.y < -70)
            DestroyWithoutPoints();
    }

    private void SetSize()
    {
        float random = Random.Range(5f, 20f);
        Vector2 size = new Vector2(random, random);
        rect.sizeDelta = size;
        collider.size = size;
    }

    private void SetSpeedBasedOnSize()
    {
        Speed = 1f / Size.x * 100f;
    }

    private void SetPosition(Vector2 position)
    {
        rect.localPosition = position;
    }

    private void SetRandomRotation()
    {
        rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
    }

    private void SetDirection()
    {
        Direction = (new Vector2(Random.Range(-30f, 30f), Random.Range(-20f, 20f)) - Position).normalized;
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

    private void DestroyWithPoints()
    {
        UIBar.GetByName("xp")?.AddValue(Size.x * 2f);
        UIBar.GetByName("health")?.AddValue(Size.x * 0.5f);
        GeometryManager.Instance.RemoveObject(transform.GetSiblingIndex());
    }

    private void DestroyWithoutPoints()
    {
        GeometryManager.Instance.RemoveObject(transform.GetSiblingIndex());
    }
}
