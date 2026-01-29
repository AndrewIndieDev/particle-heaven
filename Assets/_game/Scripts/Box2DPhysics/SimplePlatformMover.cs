using UnityEngine;

/// <summary>
/// Simple script to move a kinematic platform back and forth
/// </summary>
public class SimplePlatformMover : MonoBehaviour
{
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Box2DPhysicsShape shape;

    void Start()
    {
        startPos = transform.position;
        shape = GetComponent<Box2DPhysicsShape>();
    }

    void Update()
    {
        if (shape == null || !shape.body.isValid) return;

        // Calculate movement
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        Vector2 targetPos = startPos + Vector3.right * offset;

        // Calculate velocity needed to reach target
        Vector2 velocity = (targetPos - shape.body.position) * moveSpeed;

        // Set kinematic velocity
        shape.SetVelocity(velocity);
    }
}