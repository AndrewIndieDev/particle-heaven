using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.Collections;

/// <summary>
/// Utility for spawning multiple Box2D physics shapes efficiently
/// Demonstrates batch creation for performance
/// </summary>
public class Box2DShapeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab with Box2DPhysicsShape component")]
    public GameObject shapePrefab;
    public Transform spawnParent;

    [Tooltip("Number of shapes to spawn")]
    public int spawnCount = 1;

    [Tooltip("Spawn area size")]
    public Vector2 spawnArea = new Vector2(10f, 10f);

    [Tooltip("Random velocity range")]
    public Vector2 xVelocityRange = new Vector2(-2f, 2f);
    public float yVelocity = -20f;

    [Tooltip("Shape size range")]
    public Vector2 sizeRange = new Vector2(0.3f, 0.8f);

    [Tooltip("Spawn on start")]
    public bool spawnOnStart = false;

    private Box2DPhysicsWorld worldManager;

    void Start()
    {
        worldManager = FindFirstObjectByType<Box2DPhysicsWorld>(FindObjectsInactive.Include);

        if (spawnOnStart)
        {
            SpawnShapes();
        }
    }

    [ContextMenu("Spawn Shapes")]
    public void SpawnShapes()
    {
        if (worldManager == null)
        {
            Debug.LogError("Box2DPhysicsWorld not found!");
            return;
        }

        if (shapePrefab == null)
        {
            Debug.LogError("Shape prefab not assigned!");
            return;
        }

        // For better performance, we could batch-create bodies directly
        // But for simplicity, we'll instantiate prefabs
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleShape();
        }

        Debug.Log($"Spawned {spawnCount} shapes");
    }

    private void SpawnSingleShape()
    {
        // Random position within spawn area
        Vector2 randomPos = new Vector2(
            Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
            Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f)
        );

        // Instantiate the prefab
        GameObject instance = Instantiate(shapePrefab,
            transform.position + (Vector3)randomPos,
            Quaternion.Euler(0, 0, 90f));
        instance.transform.SetParent(spawnParent);

        // Get the physics shape component
        var physicsShape = instance.GetComponent<Box2DPhysicsShape>();
        if (physicsShape == null)
        {
            // Randomize size
            float size = Random.Range(sizeRange.x, sizeRange.y);
            physicsShape.vertices = CreateSquareVertices(size);
        }

        // Randomize velocity
        physicsShape.linearVelocity = new Vector2(
            Random.Range(xVelocityRange.x, xVelocityRange.y),
            yVelocity
        );

        physicsShape.angularVelocity = Random.Range(-5f, 5f);
    }

    /// <summary>
    /// Efficient batch spawning using direct physics API
    /// This is much faster than instantiating prefabs
    /// </summary>
    [ContextMenu("Batch Spawn (Fast)")]
    public void BatchSpawnFast()
    {
        if (worldManager == null)
        {
            Debug.LogError("Box2DPhysicsWorld not found!");
            return;
        }

        var world = worldManager.World;

        // Prepare body definitions
        var bodyDefs = new NativeArray<PhysicsBodyDefinition>(spawnCount, Allocator.Temp);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
                Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f)
            );

            bodyDefs[i] = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                position = (Vector2)transform.position + randomPos,
                rotation = new PhysicsRotate(Random.Range(0f, 360f) * Mathf.Deg2Rad),
                linearVelocity = new Vector2(
                    Random.Range(xVelocityRange.x, xVelocityRange.y),
                    Random.Range(xVelocityRange.x, xVelocityRange.y)
                ),
                angularVelocity = Random.Range(-5f, 5f)
            };
        }

        // Batch create all bodies at once (very fast!)
        var bodies = world.CreateBodyBatch(bodyDefs);
        bodyDefs.Dispose();

        // Create a simple box geometry
        var boxSize = (sizeRange.x + sizeRange.y) / 2f;
        var boxGeometry = PolygonGeometry.CreateBox(Vector2.one * boxSize, radius: 0.05f);

        // Create shape definition
        var shapeDef = new PhysicsShapeDefinition
        {
            density = 1f,
            isTrigger = false
        };

        var surfaceMaterial = shapeDef.surfaceMaterial;
        surfaceMaterial.friction = 0.3f;
        surfaceMaterial.bounciness = 0.3f;
        shapeDef.surfaceMaterial = surfaceMaterial;

        // Create shapes for all bodies
        foreach (var body in bodies)
        {
            body.CreateShape(boxGeometry, shapeDef);
        }

        bodies.Dispose();

        Debug.Log($"Batch spawned {spawnCount} shapes (fast method)");
    }

    private Vector2[] CreateSquareVertices(float size)
    {
        float halfSize = size / 2f;
        return new Vector2[]
        {
            new Vector2(-halfSize, -halfSize),
            new Vector2(halfSize, -halfSize),
            new Vector2(halfSize, halfSize),
            new Vector2(-halfSize, halfSize)
        };
    }

    void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0.1f));
    }
}