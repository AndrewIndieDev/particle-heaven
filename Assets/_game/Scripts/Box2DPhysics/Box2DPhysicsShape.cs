using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.Collections;

/// <summary>
/// Box2D v3 Physics Shape Component for Unity 6.3
/// Handles physics bodies with custom polygon shapes
/// Based on official Unity PhysicsExamples2D documentation
/// </summary>
public class Box2DPhysicsShape : MonoBehaviour
{
    [Header("Body Settings")]
    [Tooltip("Type of physics body")]
    public PhysicsBody.BodyType bodyType = PhysicsBody.BodyType.Dynamic;
    
    [Tooltip("Initial linear velocity")]
    public Vector2 linearVelocity = Vector2.zero;
    
    [Tooltip("Initial angular velocity (radians/sec)")]
    public float angularVelocity = 0f;
    
    [Tooltip("Linear damping - reduces linear velocity over time")]
    [Range(0f, 10f)]
    public float linearDamping = 0f;
    
    [Tooltip("Angular damping - reduces angular velocity over time")]
    [Range(0f, 10f)]
    public float angularDamping = 0f;
    
    [Tooltip("Gravity scale multiplier")]
    public float gravityScale = 1f;
    
    [Tooltip("Enable continuous collision detection for fast-moving objects")]
    public bool fastCollisions = false;
    
    [Header("Shape Settings")]
    [Tooltip("Define vertices for your polygon shape (will auto-decompose if concave)")]
    public Vector2[] vertices = new Vector2[]
    {
        new Vector2(-0.5f, -0.5f),
        new Vector2(0.5f, -0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(-0.5f, 0.5f)
    };
    
    [Tooltip("Density of the shape (affects mass)")]
    public float density = 1f;
    
    [Tooltip("Friction coefficient (0 = no friction, higher = more friction)")]
    [Range(0f, 1f)]
    public float friction = 0.3f;
    
    [Tooltip("Restitution/bounciness (0 = no bounce, 1 = perfect bounce)")]
    [Range(0f, 1f)]
    public float restitution = 0f;
    
    [Tooltip("Is this a trigger (no collision response, only overlap events)?")]
    public bool isTrigger = false;
    
    [Tooltip("Automatically extract shape from SpriteRenderer if available")]
    public bool useSprite = false;
    
    // Physics references
    [HideInInspector] public PhysicsBody body;
    private NativeArray<PhysicsShape> shapes;
    
    void Start()
    {
        // Find or create the world manager
        var worldManager = FindObjectOfType<Box2DPhysicsWorld>();
        if (worldManager != null)
        {
            worldManager.RegisterShape(this);
        }
        else
        {
            Debug.LogError("Box2DPhysicsWorld not found! Please add Box2DPhysicsWorld component to the scene.");
        }
    }
    
    /// <summary>
    /// Initialize this shape in the given physics world
    /// </summary>
    public void InitializeInWorld(PhysicsWorld world)
    {
        // Extract vertices from sprite if requested
        Vector2[] finalVertices = vertices;
        if (useSprite)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                finalVertices = ExtractVerticesFromSprite(spriteRenderer.sprite);
            }
        }
        
        // Create body definition
        var bodyDef = new PhysicsBodyDefinition
        {
            type = bodyType,
            position = transform.position,
            rotation = new PhysicsRotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
            linearVelocity = linearVelocity,
            angularVelocity = angularVelocity,
            linearDamping = linearDamping,
            angularDamping = angularDamping,
            gravityScale = gravityScale,
            fastCollisionsAllowed = fastCollisions
        };
        
        // Create the body
        body = world.CreateBody(bodyDef);
        
        // Create polygon shapes from vertices
        CreateShapesFromVertices(finalVertices);
    }
    
    /// <summary>
    /// Create physics shapes from vertex array
    /// </summary>
    private void CreateShapesFromVertices(Vector2[] verts)
    {
        if (verts == null || verts.Length < 3)
        {
            Debug.LogWarning($"Invalid vertices on {name}. Need at least 3 vertices.");
            return;
        }
        
        // Box2D requires convex polygons, so use CreatePolygons to auto-decompose
        var polygons = PolygonGeometry.CreatePolygons(
            verts,
            PhysicsTransform.identity,
            Vector2.one,
            Allocator.Temp
        );
        
        // Create shape definition
        var shapeDef = new PhysicsShapeDefinition
        {
            density = density,
            isTrigger = isTrigger
        };
        
        // Set surface material (friction and bounciness)
        var surfaceMaterial = shapeDef.surfaceMaterial;
        surfaceMaterial.friction = friction;
        surfaceMaterial.bounciness = restitution;
        shapeDef.surfaceMaterial = surfaceMaterial;
        
        // Create shapes from the decomposed polygons
        shapes = body.CreateShapeBatch(polygons, shapeDef);
        
        // Cleanup
        polygons.Dispose();
    }
    
    /// <summary>
    /// Extract vertices from a sprite
    /// </summary>
    private Vector2[] ExtractVerticesFromSprite(Sprite sprite)
    {
        // Get sprite vertices in local space
        var spriteVertices = sprite.vertices;
        var bounds = sprite.bounds;
        
        // Convert to world-space relative vertices
        Vector2[] vertices = new Vector2[spriteVertices.Length];
        for (int i = 0; i < spriteVertices.Length; i++)
        {
            // Sprite vertices are in sprite local space
            vertices[i] = spriteVertices[i];
        }
        
        return vertices;
    }
    
    /// <summary>
    /// Apply a force at the center of mass
    /// </summary>
    public void AddForce(Vector2 force)
    {
        if (body.isValid)
        {
            body.ApplyForce(force, body.position, true);
        }
    }
    
    /// <summary>
    /// Apply a force at a specific world point
    /// </summary>
    public void AddForceAtPosition(Vector2 force, Vector2 position)
    {
        if (body.isValid)
        {
            body.ApplyForce(force, position, true);
        }
    }
    
    /// <summary>
    /// Apply an impulse at the center of mass
    /// </summary>
    public void AddImpulse(Vector2 impulse)
    {
        if (body.isValid)
        {
            body.ApplyLinearImpulse(impulse, body.position, true);
        }
    }
    
    /// <summary>
    /// Apply an impulse at a specific world point
    /// </summary>
    public void AddImpulseAtPosition(Vector2 impulse, Vector2 position)
    {
        if (body.isValid)
        {
            body.ApplyLinearImpulse(impulse, position, true);
        }
    }
    
    /// <summary>
    /// Apply angular torque
    /// </summary>
    public void AddTorque(float torque)
    {
        if (body.isValid)
        {
            body.ApplyTorque(torque, true);
        }
    }
    
    /// <summary>
    /// Set linear velocity directly
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (body.isValid)
        {
            body.linearVelocity = velocity;
        }
    }
    
    /// <summary>
    /// Set angular velocity directly
    /// </summary>
    public void SetAngularVelocity(float velocity)
    {
        if (body.isValid)
        {
            body.angularVelocity = velocity;
        }
    }
    
    /// <summary>
    /// Sync Unity transform from physics body
    /// </summary>
    public void SyncFromPhysics()
    {
        if (body.isValid)
        {
            transform.position = new Vector3(body.position.x, body.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0, 0, body.rotation.angle * Mathf.Rad2Deg);
        }
    }
    
    /// <summary>
    /// Sync physics body from Unity transform
    /// </summary>
    public void SyncToPhysics()
    {
        if (body.isValid)
        {
            body.position = transform.position;
            body.rotation = new PhysicsRotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        }
    }
    
    /// <summary>
    /// Wake the body from sleep
    /// </summary>
    public void WakeUp()
    {
        if (body.isValid)
        {
            body.awake = true;
        }
    }
    
    /// <summary>
    /// Put the body to sleep
    /// </summary>
    public void PutToSleep()
    {
        if (body.isValid)
        {
            body.awake = false;
        }
    }

    /// <summary>
    /// Check if body is awake
    /// </summary>
    public bool IsAwake => body.isValid && body.awake;
    
    /// <summary>
    /// Get current velocity
    /// </summary>
    public Vector2 Velocity => body.isValid ? body.linearVelocity : Vector2.zero;
    
    /// <summary>
    /// Get current angular velocity
    /// </summary>
    public float AngularVelocity => body.isValid ? body.angularVelocity : 0f;
    
    /// <summary>
    /// Get body mass
    /// </summary>
    public float Mass => body.isValid ? body.mass : 0f;
    
    /// <summary>
    /// Cleanup physics objects
    /// </summary>
    public void Cleanup()
    {
        // Only clean up if the shapes array is valid AND the world still exists
        // If the world was destroyed first (during scene shutdown), Unity handles cleanup
        if (shapes.IsCreated)
        {
            try
            {
                // Only destroy through API if world is still valid
                var worldManager = FindObjectOfType<Box2DPhysicsWorld>();
                if (worldManager != null && worldManager.World.isValid)
                {
                    PhysicsShape.DestroyBatch(shapes, updateBodyMass: false);
                }
                shapes.Dispose();
            }
            catch (System.Exception)
            {
                // Silently handle cleanup errors during scene destruction
                // The physics world may have already been destroyed
            }
        }
        
        // Try to destroy body if world is still valid
        if (body.isValid)
        {
            try
            {
                var worldManager = FindObjectOfType<Box2DPhysicsWorld>();
                if (worldManager != null && worldManager.World.isValid)
                {
                    body.Destroy();
                }
            }
            catch (System.Exception)
            {
                // Silently handle cleanup errors during scene destruction
            }
        }
    }
    
    void OnDestroy()
    {
        Cleanup();
    }
    
    void OnDrawGizmos()
    {
        // Draw the physics shape outline in Scene view
        if (vertices == null || vertices.Length < 3)
            return;
        
        // Choose color based on body type
        Color gizmoColor = bodyType switch
        {
            PhysicsBody.BodyType.Static => new Color(0.5f, 0.5f, 0.5f, 0.8f),      // Gray for static
            PhysicsBody.BodyType.Dynamic => new Color(0.2f, 1f, 0.2f, 0.8f),       // Green for dynamic
            PhysicsBody.BodyType.Kinematic => new Color(1f, 0.8f, 0.2f, 0.8f),     // Yellow for kinematic
            _ => Color.white
        };
        
        // If it's a trigger, make it more transparent and use magenta tint
        if (isTrigger)
        {
            gizmoColor = new Color(1f, 0f, 1f, 0.4f); // Magenta transparent
        }
        
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        // Draw the polygon outline
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 start = vertices[i];
            Vector3 end = vertices[(i + 1) % vertices.Length];
            Gizmos.DrawLine(start, end);
        }
        
        // Draw a filled polygon for better visibility
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
        DrawFilledPolygon(vertices);
        
        // Draw center point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.zero, 0.05f);
        
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw more detailed info when selected
        if (vertices == null || vertices.Length < 3)
            return;
        
        Gizmos.matrix = transform.localToWorldMatrix;
        
        // Draw vertex points
        Gizmos.color = Color.cyan;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.08f);
            
            // Draw vertex numbers in Scene view
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.TransformPoint(vertices[i]), 
                i.ToString(), 
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white } }
            );
            #endif
        }
        
        // Draw normal vectors for each edge
        Gizmos.color = Color.blue;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % vertices.Length];
            Vector2 edgeCenter = (start + end) / 2f;
            
            // Calculate normal (perpendicular to edge)
            Vector2 edge = end - start;
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized * 0.3f;
            
            Gizmos.DrawLine(edgeCenter, edgeCenter + normal);
            Gizmos.DrawSphere(edgeCenter + normal, 0.03f);
        }
        
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    // Helper method to draw filled polygon
    private void DrawFilledPolygon(Vector2[] points)
    {
        if (points.Length < 3) return;
        
        // Simple triangle fan from center
        Vector2 center = Vector2.zero;
        foreach (var point in points)
            center += point;
        center /= points.Length;
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % points.Length];
            Vector3 c = center;
            
            // Draw triangle
            Gizmos.DrawLine(c, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, c);
        }
    }
}