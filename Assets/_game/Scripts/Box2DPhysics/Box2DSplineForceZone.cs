using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Creates a force zone around a Unity Spline that applies directional forces to Box2D physics objects
/// ONLY APPLIES FORCES - Never sets velocity directly
/// </summary>
[RequireComponent(typeof(SplineContainer))]
public class Box2DSplineForceZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Radius of the force zone around the spline")]
    [Range(0.1f, 20f)]
    public float zoneRadius = 3f;

    [Header("Force Settings")]
    [Tooltip("Force pushing objects along the spline direction")]
    [Range(0f, 100f)]
    public float directionalForce = 15f;

    [Tooltip("Use custom direction instead of spline tangent")]
    public bool useCustomDirection = false;

    [Tooltip("Custom force direction (normalized automatically)")]
    public Vector2 customDirection = Vector2.right;

    [Tooltip("Force pulling objects toward the spline center")]
    [Range(0f, 100f)]
    public float centeringForce = 10f;

    [Tooltip("How force strength changes with distance from center (0=center, 1=edge)")]
    public AnimationCurve forceFalloff = AnimationCurve.Linear(0, 1, 1, 0.5f);

    [Header("Optional Settings")]
    [Tooltip("Only affect objects with this tag (empty = affect all)")]
    public string targetTag = "";

    [Tooltip("How often to scan for objects in zone (seconds)")]
    [Range(0.1f, 2f)]
    public float detectionInterval = 0.5f;

    [Tooltip("Number of sample points along spline")]
    [Range(10, 200)]
    public int splineSampleCount = 50;

    [Header("Visualization")]
    [Tooltip("Show force zone in Scene view")]
    public bool showGizmos = true;

    [Tooltip("Gizmo color")]
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.3f);

    // Private variables
    private SplineContainer splineContainer;
    private List<Box2DPhysicsShape> objectsInZone = new List<Box2DPhysicsShape>();
    private float lastDetectionTime;
    private float3[] splinePoints;
    private float3[] splineTangents;

    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
        {
            Debug.LogError("Box2DSplineForceZone requires a SplineContainer component!");
            enabled = false;
            return;
        }

        CacheSplineData();
        DetectObjectsInZone();
    }

    void FixedUpdate()
    {
        // Periodic detection
        if (Time.time - lastDetectionTime > detectionInterval)
        {
            DetectObjectsInZone();
            lastDetectionTime = Time.time;
        }

        // Apply forces to all objects in zone
        for (int i = objectsInZone.Count - 1; i >= 0; i--)
        {
            var obj = objectsInZone[i];

            // Remove null or destroyed objects
            if (obj == null || !obj.body.isValid)
            {
                objectsInZone.RemoveAt(i);
                continue;
            }

            ApplyForcesToObject(obj);
        }
    }

    /// <summary>
    /// Cache spline data for faster lookups
    /// </summary>
    private void CacheSplineData()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        splinePoints = new float3[splineSampleCount];
        splineTangents = new float3[splineSampleCount];

        for (int i = 0; i < splineSampleCount; i++)
        {
            float t = i / (float)(splineSampleCount - 1);

            // Get position and tangent in world space
            splineContainer.Spline.Evaluate(t, out float3 position, out float3 tangent, out _);

            float3 worldPos = transform.TransformPoint(position);
            float3 worldTangent = math.normalize(transform.TransformDirection(tangent));

            splinePoints[i] = worldPos;
            splineTangents[i] = worldTangent;
        }
    }

    /// <summary>
    /// Find the closest point on the spline to a given world position
    /// </summary>
    private void GetClosestPointOnSpline(float3 worldPos, out float3 closestPoint, out float3 tangent, out float distance)
    {
        closestPoint = float3.zero;
        tangent = new float3(0, 1, 0);
        distance = float.MaxValue;

        // Find closest cached point
        for (int i = 0; i < splinePoints.Length; i++)
        {
            float dist = math.distance(worldPos, splinePoints[i]);
            if (dist < distance)
            {
                distance = dist;
                closestPoint = splinePoints[i];
                tangent = splineTangents[i];
            }
        }
    }

    /// <summary>
    /// Apply forces to a physics object - ONLY FORCES, NEVER SET VELOCITY
    /// </summary>
    private void ApplyForcesToObject(Box2DPhysicsShape physicsShape)
    {
        if (!physicsShape.body.isValid)
            return;

        // Get object position in 2D
        float3 objectPos = new float3(physicsShape.transform.position.x, physicsShape.transform.position.y, 0);

        // Get closest point on spline
        GetClosestPointOnSpline(objectPos, out float3 closestPoint, out float3 tangent, out float distance);

        // Check if within zone radius
        if (distance > zoneRadius)
            return;

        // Calculate force multiplier based on distance from center
        float normalizedDistance = math.clamp(distance / zoneRadius, 0f, 1f);
        float forceMultiplier = forceFalloff.Evaluate(normalizedDistance);

        bool forcesApplied = false;

        // Apply directional force along spline
        if (directionalForce > 0)
        {
            float2 forceDirection;

            if (useCustomDirection)
            {
                // Use custom direction
                forceDirection = math.normalize(new float2(customDirection.x, customDirection.y));
            }
            else
            {
                // Use spline tangent direction
                forceDirection = new float2(tangent.x, tangent.y);
            }

            float2 dirForce = forceDirection * directionalForce * forceMultiplier;

            physicsShape.AddForce(dirForce);
            forcesApplied = true;
        }

        // Apply centering force toward spline
        if (centeringForce > 0 && distance > 0.01f)
        {
            float3 directionToCenter = math.normalize(closestPoint - objectPos);
            float2 centerForce = new float2(directionToCenter.x, directionToCenter.y) * centeringForce * forceMultiplier;

            physicsShape.AddForce(centerForce);
            forcesApplied = true;
        }

        // Debug: Log when forces are applied (only first frame for each object)
        if (forcesApplied && Time.frameCount % 60 == 0) // Log once per second at 60fps
        {
            Debug.Log($"Applying forces to {physicsShape.name}: distance={distance:F2}, multiplier={forceMultiplier:F2}, dirForce={directionalForce * forceMultiplier:F1}, centerForce={centeringForce * forceMultiplier:F1}");
        }
    }

    /// <summary>
    /// Detect all Box2D physics objects in zone
    /// </summary>
    private void DetectObjectsInZone()
    {
        objectsInZone.Clear();

        var allPhysicsObjects = FindObjectsOfType<Box2DPhysicsShape>();

        foreach (var obj in allPhysicsObjects)
        {
            // Skip if not dynamic
            if (obj.bodyType != PhysicsBody.BodyType.Dynamic)
                continue;

            // Skip if tag filtering is enabled and doesn't match
            if (!string.IsNullOrEmpty(targetTag) && !obj.CompareTag(targetTag))
                continue;

            // Check if within zone radius
            float3 objPos = new float3(obj.transform.position.x, obj.transform.position.y, 0);
            GetClosestPointOnSpline(objPos, out _, out _, out float distance);

            if (distance <= zoneRadius)
            {
                objectsInZone.Add(obj);
            }
        }

        Debug.Log($"Spline Force Zone: Detected {objectsInZone.Count} objects in zone (radius: {zoneRadius})");
    }

    /// <summary>
    /// Manually add an object to the force zone
    /// </summary>
    public void AddObject(Box2DPhysicsShape physicsShape)
    {
        if (!objectsInZone.Contains(physicsShape))
        {
            objectsInZone.Add(physicsShape);
        }
    }

    /// <summary>
    /// Manually remove an object from the force zone
    /// </summary>
    public void RemoveObject(Box2DPhysicsShape physicsShape)
    {
        objectsInZone.Remove(physicsShape);
    }

    /// <summary>
    /// Clear all objects from the zone
    /// </summary>
    public void ClearObjects()
    {
        objectsInZone.Clear();
    }

    /// <summary>
    /// Refresh the spline cache (call if spline is modified at runtime)
    /// </summary>
    public void RefreshSplineCache()
    {
        CacheSplineData();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || splineContainer == null || splineContainer.Spline == null)
            return;

        Gizmos.color = gizmoColor;

        // Draw zone radius around spline
        int segments = 50;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;

            splineContainer.Spline.Evaluate(t1, out float3 pos1, out float3 tan1, out _);
            splineContainer.Spline.Evaluate(t2, out float3 pos2, out float3 tan2, out _);

            pos1 = transform.TransformPoint(pos1);
            pos2 = transform.TransformPoint(pos2);

            // Draw center line
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawLine(pos1, pos2);

            // Calculate perpendicular direction in 2D (rotate tangent 90 degrees)
            float3 worldTan1 = math.normalize(transform.TransformDirection(tan1));
            float3 worldTan2 = math.normalize(transform.TransformDirection(tan2));

            float3 perp1 = new float3(-worldTan1.y, worldTan1.x, 0);
            float3 perp2 = new float3(-worldTan2.y, worldTan2.x, 0);

            Gizmos.color = gizmoColor;

            // Upper boundary
            float3 upper1 = pos1 + perp1 * zoneRadius;
            float3 upper2 = pos2 + perp2 * zoneRadius;
            Gizmos.DrawLine(upper1, upper2);

            // Lower boundary
            float3 lower1 = pos1 - perp1 * zoneRadius;
            float3 lower2 = pos2 - perp2 * zoneRadius;
            Gizmos.DrawLine(lower1, lower2);
        }

        // Draw objects in zone with force vectors
        foreach (var obj in objectsInZone)
        {
            if (obj != null)
            {
                // Yellow sphere for object
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(obj.transform.position, 0.3f);

                // Get force information
                float3 objectPos = new float3(obj.transform.position.x, obj.transform.position.y, 0);
                GetClosestPointOnSpline(objectPos, out float3 closestPoint, out float3 tangent, out float distance);

                if (distance <= zoneRadius)
                {
                    float normalizedDistance = math.clamp(distance / zoneRadius, 0f, 1f);
                    float forceMultiplier = forceFalloff.Evaluate(normalizedDistance);

                    // Draw directional force (GREEN)
                    if (directionalForce > 0)
                    {
                        Gizmos.color = Color.green;
                        float2 forceDirection;

                        if (useCustomDirection)
                        {
                            forceDirection = math.normalize(new float2(customDirection.x, customDirection.y));
                        }
                        else
                        {
                            forceDirection = new float2(tangent.x, tangent.y);
                        }

                        float forceScale = directionalForce * forceMultiplier * 0.01f; // Scale for visibility
                        Vector3 forceVec = new Vector3(forceDirection.x, forceDirection.y, 0) * forceScale;
                        Gizmos.DrawLine(obj.transform.position, obj.transform.position + forceVec);
                        Gizmos.DrawSphere(obj.transform.position + forceVec, 0.1f);
                    }

                    // Draw centering force (BLUE)
                    if (centeringForce > 0 && distance > 0.01f)
                    {
                        Gizmos.color = Color.blue;
                        float3 directionToCenter = math.normalize(closestPoint - objectPos);
                        float forceScale = centeringForce * forceMultiplier * 0.01f; // Scale for visibility
                        Vector3 forceVec = new Vector3(directionToCenter.x, directionToCenter.y, 0) * forceScale;
                        Gizmos.DrawLine(obj.transform.position, obj.transform.position + forceVec);
                        Gizmos.DrawSphere(obj.transform.position + forceVec, 0.1f);
                    }

                    // Draw closest point (MAGENTA)
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(closestPoint, 0.2f);
                    Gizmos.DrawLine(obj.transform.position, closestPoint);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        // Draw force direction arrows
        Gizmos.color = Color.green;

        int arrows = 10;
        for (int i = 0; i < arrows; i++)
        {
            float t = i / (float)(arrows - 1);

            splineContainer.Spline.Evaluate(t, out float3 pos, out float3 tan, out _);

            pos = transform.TransformPoint(pos);
            tan = math.normalize(transform.TransformDirection(tan));

            // Draw arrow showing force direction
            float3 arrowEnd = pos + tan * 1.5f;
            Gizmos.DrawLine(pos, arrowEnd);

            // Arrow head
            float3 perpDir = new float3(-tan.y, tan.x, 0) * 0.3f;
            Gizmos.DrawLine(arrowEnd, arrowEnd - tan * 0.4f + perpDir);
            Gizmos.DrawLine(arrowEnd, arrowEnd - tan * 0.4f - perpDir);
        }
    }
}