using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using System.Collections.Generic;

/// <summary>
/// Box2D v3 Physics World Manager for Unity 6.3
/// Manages the physics simulation and all registered shapes
/// Based on official Unity PhysicsExamples2D documentation
/// </summary>
public class Box2DPhysicsWorld : MonoBehaviour
{
    [Header("World Settings")]
    [Tooltip("Gravity vector (usually negative Y)")]
    public Vector2 gravity = new Vector2(0, -9.81f);

    [Tooltip("When to simulate physics")]
    public PhysicsWorld.SimulationType simulateType = PhysicsWorld.SimulationType.FixedUpdate;

    [Tooltip("Time step for simulation (only used in Script mode)")]
    public float timeStep = 1f / 60f;

    [Header("Performance")]
    [Tooltip("Use the default world (recommended) or create a custom world")]
    public bool useDefaultWorld = true;

    [Tooltip("Enable multithreading for physics simulation")]
    public bool enableMultithreading = true;

    [Tooltip("Number of worker threads (0 = auto)")]
    [Range(0, 8)]
    public int workerThreads = 0;

    private PhysicsWorld world;
    private List<Box2DPhysicsShape> registeredShapes = new List<Box2DPhysicsShape>();

    void Awake()
    {
        InitializeWorld();
    }

    void Start()
    {
        // Register all existing shapes in the scene
        var existingShapes = FindObjectsOfType<Box2DPhysicsShape>();
        foreach (var shape in existingShapes)
        {
            if (!registeredShapes.Contains(shape))
            {
                // Shape will register itself in its Start method
            }
        }

        Debug.Log($"Box2D World initialized with {registeredShapes.Count} shapes. Mode: {simulateType}");
    }

    void InitializeWorld()
    {
        if (useDefaultWorld)
        {
            // Use Unity's default world
            world = PhysicsWorld.defaultWorld;

            // Configure default world settings
            world.gravity = gravity;
        }
        else
        {
            // Create a custom world
            var worldDef = new PhysicsWorldDefinition
            {
                gravity = gravity,
                simulateType = simulateType,
                simulationWorkers = enableMultithreading ? workerThreads : 1
            };

            world = PhysicsWorld.Create(worldDef);
        }
    }

    /// <summary>
    /// Register a shape with this world
    /// </summary>
    public void RegisterShape(Box2DPhysicsShape shape)
    {
        if (!registeredShapes.Contains(shape))
        {
            shape.InitializeInWorld(world);
            registeredShapes.Add(shape);
        }
    }

    /// <summary>
    /// Unregister a shape from this world
    /// </summary>
    public void UnregisterShape(Box2DPhysicsShape shape)
    {
        registeredShapes.Remove(shape);
    }

    void FixedUpdate()
    {
        // Only sync if using FixedUpdate mode
        if (simulateType == PhysicsWorld.SimulationType.FixedUpdate || useDefaultWorld)
        {
            SyncTransforms();
        }
    }

    void Update()
    {
        // Manual simulation if in Script mode
        if (simulateType == PhysicsWorld.SimulationType.Script && !useDefaultWorld)
        {
            world.Simulate(timeStep);
            SyncTransforms();
        }
        // Sync if using Update mode
        else if (simulateType == PhysicsWorld.SimulationType.Update)
        {
            SyncTransforms();
        }
    }

    /// <summary>
    /// Manually step the simulation (only works in Script mode)
    /// </summary>
    public void ManualSimulate(float deltaTime)
    {
        if (simulateType == PhysicsWorld.SimulationType.Script && !useDefaultWorld)
        {
            world.Simulate(deltaTime);
            SyncTransforms();
        }
    }

    /// <summary>
    /// Sync all transforms from physics
    /// </summary>
    private void SyncTransforms()
    {
        foreach (var shape in registeredShapes)
        {
            if (shape != null && shape.bodyType == PhysicsBody.BodyType.Dynamic)
            {
                shape.SyncFromPhysics();
            }
        }
    }

    /// <summary>
    /// Update gravity at runtime
    /// </summary>
    public void SetGravity(Vector2 newGravity)
    {
        gravity = newGravity;
        if (world.isValid)
        {
            world.gravity = newGravity;
        }
    }

    /// <summary>
    /// Get the physics world
    /// </summary>
    public PhysicsWorld World => world;

    /// <summary>
    /// Get number of registered shapes
    /// </summary>
    public int ShapeCount => registeredShapes.Count;

    void OnDestroy()
    {
        // Cleanup all shapes first
        foreach (var shape in registeredShapes)
        {
            if (shape != null)
            {
                shape.Cleanup();
            }
        }
        registeredShapes.Clear();

        // Destroy custom world (don't destroy default world)
        if (!useDefaultWorld && world.isValid)
        {
            try
            {
                world.Destroy();
            }
            catch (System.Exception)
            {
                // Silently handle errors during scene destruction
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw gravity direction in scene view
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 gravityDir = gravity.normalized;
        Gizmos.DrawLine(center, center + (Vector3)gravity * 0.5f);
        Gizmos.DrawSphere(center + (Vector3)gravity * 0.5f, 0.1f);
    }
}