using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Complete example demonstrating Box2D v3 physics features
/// Shows various physics interactions and use cases
/// </summary>
public class Box2DExampleScene : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("Automatically create example scene on start")]
    public bool createSceneOnStart = true;

    private Box2DPhysicsWorld world;

    void Awake()
    {
        if (createSceneOnStart)
        {
            CreateExampleScene();
        }
    }

    [ContextMenu("Create Example Scene")]
    public void CreateExampleScene()
    {
        // Get or create world
        world = FindObjectOfType<Box2DPhysicsWorld>();
        if (world == null)
        {
            var worldObj = new GameObject("Physics World");
            world = worldObj.AddComponent<Box2DPhysicsWorld>();
            world.gravity = new Vector2(0, -9.81f);
        }

        // Create various example objects
        CreateGround();
        CreateWalls();
        CreateDynamicBoxes();
        CreateCircularObject();
        CreateTriggerZone();
        CreateMovingPlatform();

        Debug.Log("Example scene created! Press Space to spawn more boxes.");
    }

    /// <summary>
    /// Create a static ground platform
    /// </summary>
    private void CreateGround()
    {
        var groundObj = new GameObject("Ground");
        var ground = groundObj.AddComponent<Box2DPhysicsShape>();

        ground.bodyType = PhysicsBody.BodyType.Static;
        ground.vertices = new Vector2[]
        {
            new Vector2(-15f, -0.5f),
            new Vector2(15f, -0.5f),
            new Vector2(15f, 0.5f),
            new Vector2(-15f, 0.5f)
        };
        ground.friction = 0.5f;
        ground.restitution = 0f;

        groundObj.transform.position = new Vector3(0, -5, 0);

        // Add visual (optional)
        AddSimpleVisual(groundObj, new Vector2(30f, 1f), Color.gray);
    }

    /// <summary>
    /// Create side walls
    /// </summary>
    private void CreateWalls()
    {
        // Left wall
        var leftWall = new GameObject("Left Wall");
        var leftShape = leftWall.AddComponent<Box2DPhysicsShape>();
        leftShape.bodyType = PhysicsBody.BodyType.Static;
        leftShape.vertices = new Vector2[]
        {
            new Vector2(-0.5f, -10f),
            new Vector2(0.5f, -10f),
            new Vector2(0.5f, 80f),
            new Vector2(-0.5f, 80f)
        };
        leftWall.transform.position = new Vector3(-16, 0, 0);
        AddSimpleVisual(leftWall, new Vector2(1f, 20f), Color.gray);

        // Right wall
        var rightWall = new GameObject("Right Wall");
        var rightShape = rightWall.AddComponent<Box2DPhysicsShape>();
        rightShape.bodyType = PhysicsBody.BodyType.Static;
        rightShape.vertices = new Vector2[]
        {
            new Vector2(-0.5f, -10f),
            new Vector2(0.5f, -10f),
            new Vector2(0.5f, 80f),
            new Vector2(-0.5f, 80f)
        };
        rightWall.transform.position = new Vector3(16, 0, 0);
        AddSimpleVisual(rightWall, new Vector2(1f, 20f), Color.gray);
    }

    /// <summary>
    /// Create some dynamic boxes that fall
    /// </summary>
    private void CreateDynamicBoxes()
    {
        for (int i = 0; i < 5; i++)
        {
            var box = new GameObject($"Dynamic Box {i}");
            var shape = box.AddComponent<Box2DPhysicsShape>();

            shape.bodyType = PhysicsBody.BodyType.Dynamic;
            shape.vertices = CreateBoxVertices(1f);
            shape.density = 1f;
            shape.friction = 0.3f;
            shape.restitution = 0.3f;

            box.transform.position = new Vector3(-4 + i * 2f, 5f + i * 1.5f, 0);
            box.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

            AddSimpleVisual(box, Vector2.one, new Color(Random.value, Random.value, Random.value));
        }
    }

    /// <summary>
    /// Create a circular object (approximated by polygon)
    /// </summary>
    private void CreateCircularObject()
    {
        var circle = new GameObject("Circle");
        var shape = circle.AddComponent<Box2DPhysicsShape>();

        shape.bodyType = PhysicsBody.BodyType.Dynamic;
        shape.vertices = CreateCircleVertices(0.5f, 16);
        shape.density = 2f;
        shape.friction = 0.1f;
        shape.restitution = 0.8f; // Very bouncy!

        circle.transform.position = new Vector3(0, 8, 0);

        AddSimpleVisual(circle, Vector2.one, Color.red);
    }

    /// <summary>
    /// Create a trigger zone
    /// </summary>
    private void CreateTriggerZone()
    {
        var trigger = new GameObject("Trigger Zone");
        var shape = trigger.AddComponent<Box2DPhysicsShape>();

        shape.bodyType = PhysicsBody.BodyType.Static;
        shape.vertices = CreateBoxVertices(3f);
        shape.isTrigger = true;

        trigger.transform.position = new Vector3(8, -2, 0);

        AddSimpleVisual(trigger, Vector2.one * 3f, new Color(1f, 1f, 0f, 0.3f));
    }

    /// <summary>
    /// Create a moving platform (kinematic body)
    /// </summary>
    private void CreateMovingPlatform()
    {
        var platform = new GameObject("Moving Platform");
        var shape = platform.AddComponent<Box2DPhysicsShape>();

        shape.bodyType = PhysicsBody.BodyType.Kinematic;
        shape.vertices = new Vector2[]
        {
            new Vector2(-5f, -0.5f),
            new Vector2(5f, -0.5f),
            new Vector2(5f, 0.5f),
            new Vector2(-5f, 0.5f)
        };
        shape.friction = 0.8f;

        platform.transform.position = new Vector3(-8, 0, 0);

        // Add movement script
        platform.AddComponent<SimplePlatformMover>();

        AddSimpleVisual(platform, new Vector2(4f, 0.5f), Color.green);
    }

    void Update()
    {
        // Press Space to spawn a box
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBox();
        }

        // Press R to add random forces to all dynamic objects
        if (Input.GetKeyDown(KeyCode.R))
        {
            ApplyRandomForces();
        }
    }

    /// <summary>
    /// Spawn a new dynamic box at cursor position
    /// </summary>
    private void SpawnBox()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        var box = new GameObject("Spawned Box");
        var shape = box.AddComponent<Box2DPhysicsShape>();

        shape.bodyType = PhysicsBody.BodyType.Dynamic;
        shape.vertices = CreateBoxVertices(Random.Range(0.5f, 1.5f));
        shape.density = Random.Range(0.5f, 2f);
        shape.friction = Random.Range(0.1f, 0.8f);
        shape.restitution = Random.Range(0f, 0.5f);

        box.transform.position = mousePos;
        box.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        AddSimpleVisual(box, Vector2.one * shape.vertices[2].x * 2f,
            new Color(Random.value, Random.value, Random.value));
    }

    /// <summary>
    /// Apply random forces to all dynamic objects
    /// </summary>
    private void ApplyRandomForces()
    {
        var shapes = FindObjectsOfType<Box2DPhysicsShape>();
        foreach (var shape in shapes)
        {
            if (shape.bodyType == PhysicsBody.BodyType.Dynamic)
            {
                Vector2 randomForce = Random.insideUnitCircle * 50f;
                shape.AddForce(randomForce);
                shape.AddTorque(Random.Range(-10f, 10f));
            }
        }
    }

    // Helper methods

    private Vector2[] CreateBoxVertices(float size)
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

    private Vector2[] CreateCircleVertices(float radius, int segments)
    {
        Vector2[] vertices = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            vertices[i] = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }
        return vertices;
    }

    private void AddSimpleVisual(GameObject obj, Vector2 size, Color color)
    {
        // Create a simple sprite renderer for visualization
        var sprite = obj.AddComponent<SpriteRenderer>();

        // Create a simple white texture
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        sprite.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        sprite.color = color;
        sprite.transform.localScale = new Vector3(size.x, size.y, 1f);
    }
}