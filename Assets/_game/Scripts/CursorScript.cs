using AndrewDowsett.CommonObservers;
using UnityEngine;

public class CursorScript : MonoBehaviour, IUpdateObserver
{
    public static CursorScript Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void Enable()
    {
        Cursor.visible = false;
        ToggleVisuals(true);
        UpdateManager.RegisterObserver(this);
    }

    public void Disable()
    {
        Cursor.visible = true;
        ToggleVisuals(false);
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }

    void ToggleVisuals(bool enable)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(enable);
        }
    }
}
