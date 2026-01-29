using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class SimplePinballPaddle : MonoBehaviour
{
    [Header("Paddle Settings")]
    [Tooltip("Angle to rotate when activated (degrees)")]
    public float angleToMove = 1f;
    public bool isRightPaddle = false;

    [Tooltip("Rotation speed (degrees per second)")]
    public float angularVelocity = 300f;

    [Header("Input")]
    [Tooltip("Key to activate the paddle")]
    public KeyCode activationKey = KeyCode.Space;

    private float startAngle;
    private float targetAngle;
    private Box2DPhysicsShape shape;

    void Start()
    {
        shape = GetComponent<Box2DPhysicsShape>();

        if (shape == null)
        {
            Debug.LogError("SimplePinballPaddle requires Box2DPhysicsShape component!");
            enabled = false;
            return;
        }

        // Store starting angle
        startAngle = transform.rotation.z * 2f;
        targetAngle = startAngle;
    }

    void Update()
    {
        if (shape == null || !shape.body.isValid) return;

        // Set target angle based on input
        if (Input.GetKey(activationKey))
        {
            targetAngle = startAngle + angleToMove;

            if (!isRightPaddle)
            {
                if (shape.body.rotation.angle > targetAngle)
                {
                    shape.SetAngularVelocity(0);
                    return;
                }
                shape.SetAngularVelocity(angularVelocity);
            }
            else
            {
                if (shape.body.rotation.angle < -targetAngle)
                {
                    shape.SetAngularVelocity(0);
                    return;
                }
                shape.SetAngularVelocity(-angularVelocity);
            }

            Debug.Log($"Start angle: {startAngle} | Target angle: {targetAngle} | Current angle: {shape.body.rotation.angle}");
        }
        else
        {
            targetAngle = startAngle;

            if (!isRightPaddle)
            {
                if (shape.body.rotation.angle < targetAngle)
                {
                    shape.SetAngularVelocity(0);
                    return;
                }
                shape.SetAngularVelocity(-angularVelocity);
            }
            else
            {
                if (shape.body.rotation.angle > targetAngle)
                {
                    shape.SetAngularVelocity(0);
                    return;
                }
                shape.SetAngularVelocity(angularVelocity);
            }
        }
    }
}