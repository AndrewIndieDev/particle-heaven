using AndrewDowsett.CommonObservers;
using UnityEngine;

public class ConnectToCursor : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private LineRenderer _lr;

    private Transform cursorTransform;

    private void Start()
    {
        cursorTransform = CursorScript.Instance.transform;
    }

    public void Enable()
    {
        _lr.enabled = true;
        UpdateManager.RegisterObserver(this);
    }

    public void Disable()
    {
        _lr.enabled = false;
        _lr.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        _lr.SetPositions(new Vector3[] { transform.position, cursorTransform.position });
    }
}
