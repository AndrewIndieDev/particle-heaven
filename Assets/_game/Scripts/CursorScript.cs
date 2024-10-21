using AndrewDowsett.CommonObservers;
using MoreMountains.Feedbacks;
using UnityEngine;

public class CursorScript : MonoBehaviour, IUpdateObserver
{
    public static CursorScript Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Transform bulletParent;
    [SerializeField] private MMF_Player shootFeedbacks;

    private float _shootDelay;

    public void Enable()
    {
        Cursor.visible = false;
        ToggleChildren(true);
        UpdateManager.RegisterObserver(this);
    }

    public void Disable()
    {
        Cursor.visible = true;
        ToggleChildren(false);
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        _shootDelay += deltaTime;
        if (_shootDelay >= 2f)
        {
            _shootDelay = 0f;
            shootFeedbacks.PlayFeedbacks();
            bulletParent.position = transform.position;
            for (int i = 0; i < bulletParent.childCount; i++)
            {
                bulletParent.GetChild(i).GetComponent<Bullet>().Shoot();
            }
        }

        // raycast to find an object to hit to set the position of this object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }

    void ToggleChildren(bool enable)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(enable);
        }
    }
}
