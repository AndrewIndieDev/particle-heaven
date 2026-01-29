using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Visual vertex editor for Box2DPhysicsShape
/// Allows dragging vertices in Scene view using Unity's move handles
/// </summary>
[RequireComponent(typeof(Box2DPhysicsShape))]
public class Box2DVertexEditor : MonoBehaviour
{
    [Header("Editor Settings")]
    [Tooltip("Enable vertex editing in Scene view")]
    public bool enableEditing = true;

    [Tooltip("Size of vertex handles")]
    [Range(0.05f, 0.5f)]
    public float handleSize = 0.15f;

    [Tooltip("Snap to grid when moving vertices")]
    public bool snapToGrid = false;

    [Tooltip("Grid snap size")]
    [Range(0.1f, 1f)]
    public float gridSnapSize = 0.25f;

    [Header("Quick Actions")]
    [Tooltip("Add a new vertex")]
    public bool addVertex = false;

    [Tooltip("Remove last vertex")]
    public bool removeVertex = false;

    [Tooltip("Center all vertices around origin")]
    public bool centerVertices = false;

    [Tooltip("Reverse vertex order")]
    public bool reverseOrder = false;

    private Box2DPhysicsShape physicsShape;

    void OnValidate()
    {
        if (physicsShape == null)
            physicsShape = GetComponent<Box2DPhysicsShape>();

        // Handle quick actions
        if (addVertex)
        {
            addVertex = false;
            AddVertexAtCenter();
        }

        if (removeVertex)
        {
            removeVertex = false;
            RemoveLastVertex();
        }

        if (centerVertices)
        {
            centerVertices = false;
            CenterVertices();
        }

        if (reverseOrder)
        {
            reverseOrder = false;
            ReverseVertexOrder();
        }
    }

    void Reset()
    {
        physicsShape = GetComponent<Box2DPhysicsShape>();
    }

    private void AddVertexAtCenter()
    {
        if (physicsShape == null || physicsShape.vertices == null)
            return;

        Vector2 center = Vector2.zero;
        foreach (var v in physicsShape.vertices)
            center += v;
        center /= physicsShape.vertices.Length;

        var newVertices = new Vector2[physicsShape.vertices.Length + 1];
        for (int i = 0; i < physicsShape.vertices.Length; i++)
            newVertices[i] = physicsShape.vertices[i];
        newVertices[physicsShape.vertices.Length] = center;

        physicsShape.vertices = newVertices;
        Debug.Log($"Added vertex at center: {center}");
    }

    private void RemoveLastVertex()
    {
        if (physicsShape == null || physicsShape.vertices == null || physicsShape.vertices.Length <= 3)
        {
            Debug.LogWarning("Cannot remove vertex - need at least 3 vertices!");
            return;
        }

        var newVertices = new Vector2[physicsShape.vertices.Length - 1];
        for (int i = 0; i < newVertices.Length; i++)
            newVertices[i] = physicsShape.vertices[i];

        physicsShape.vertices = newVertices;
        Debug.Log("Removed last vertex");
    }

    private void CenterVertices()
    {
        if (physicsShape == null || physicsShape.vertices == null)
            return;

        Vector2 center = Vector2.zero;
        foreach (var v in physicsShape.vertices)
            center += v;
        center /= physicsShape.vertices.Length;

        for (int i = 0; i < physicsShape.vertices.Length; i++)
            physicsShape.vertices[i] -= center;

        Debug.Log($"Centered vertices (offset by {-center})");
    }

    private void ReverseVertexOrder()
    {
        if (physicsShape == null || physicsShape.vertices == null)
            return;

        System.Array.Reverse(physicsShape.vertices);
        Debug.Log("Reversed vertex order");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!enableEditing || physicsShape == null || physicsShape.vertices == null)
            return;

        // Draw hint text when not selected
        if (Selection.activeGameObject != gameObject)
        {
            Handles.Label(
                transform.position,
                "Select to edit vertices",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = new Color(1f, 1f, 0f, 0.5f) },
                    fontSize = 12
                }
            );
        }
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for Box2DVertexEditor that draws interactive handles
/// </summary>
[CustomEditor(typeof(Box2DVertexEditor))]
public class Box2DVertexEditorInspector : Editor
{
    private Box2DVertexEditor vertexEditor;
    private Box2DPhysicsShape physicsShape;
    private int selectedVertex = -1;
    private bool isDragging = false;

    void OnEnable()
    {
        vertexEditor = (Box2DVertexEditor)target;
        physicsShape = vertexEditor.GetComponent<Box2DPhysicsShape>();
    }

    void OnSceneGUI()
    {
        if (!vertexEditor.enableEditing || physicsShape == null || physicsShape.vertices == null)
            return;

        // Draw interactive handles for each vertex
        Transform t = vertexEditor.transform;

        for (int i = 0; i < physicsShape.vertices.Length; i++)
        {
            DrawVertexHandle(i, t);
        }

        // Draw instructions
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");

        GUILayout.Label("Vertex Editor", EditorStyles.boldLabel);
        GUILayout.Label($"Vertices: {physicsShape.vertices.Length}");

        if (selectedVertex >= 0)
            GUILayout.Label($"Selected: Vertex {selectedVertex}", EditorStyles.helpBox);

        GUILayout.Space(5);
        GUILayout.Label("Controls:", EditorStyles.miniLabel);
        GUILayout.Label("• Drag handles to move vertices");
        GUILayout.Label("• Click vertex to select");
        GUILayout.Label("• Use Inspector quick actions");

        GUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();

        // Force repaint when dragging
        if (isDragging)
        {
            SceneView.RepaintAll();
        }
    }

    private void DrawVertexHandle(int index, Transform transform)
    {
        Vector3 worldPos = transform.TransformPoint(physicsShape.vertices[index]);
        float handleSize = HandleUtility.GetHandleSize(worldPos) * vertexEditor.handleSize;

        // Determine handle color
        Color handleColor = selectedVertex == index ? Color.yellow : Color.cyan;
        Handles.color = handleColor;

        // Draw the vertex sphere
        if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
        {
            selectedVertex = index;
            Repaint();
        }

        // If this vertex is selected, show position handle
        if (selectedVertex == index)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                isDragging = true;
                Undo.RecordObject(physicsShape, "Move Vertex");

                Vector3 localPos = transform.InverseTransformPoint(newWorldPos);

                // Apply grid snapping if enabled
                if (vertexEditor.snapToGrid)
                {
                    float snap = vertexEditor.gridSnapSize;
                    localPos.x = Mathf.Round(localPos.x / snap) * snap;
                    localPos.y = Mathf.Round(localPos.y / snap) * snap;
                }

                physicsShape.vertices[index] = localPos;
                EditorUtility.SetDirty(physicsShape);
            }
            else
            {
                isDragging = false;
            }

            // Draw label with coordinates
            Handles.Label(
                worldPos + Vector3.up * handleSize * 2f,
                $"V{index}: ({physicsShape.vertices[index].x:F2}, {physicsShape.vertices[index].y:F2})",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.yellow },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                }
            );
        }
        else
        {
            // Draw simple label for unselected vertices
            Handles.Label(
                worldPos,
                index.ToString(),
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white },
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Select the GameObject in Scene view to see and drag vertex handles.\n\n" +
            "• Cyan spheres = vertices\n" +
            "• Yellow = selected vertex\n" +
            "• Click to select, drag to move",
            MessageType.Info
        );

        // Quick preset shapes
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Square (1x1)"))
            CreateSquare(1f);
        if (GUILayout.Button("Rectangle (2x1)"))
            CreateRectangle(2f, 1f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Triangle"))
            CreateTriangle(1f);
        if (GUILayout.Button("Circle (16)"))
            CreateCircle(0.5f, 16);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Hexagon"))
            CreateCircle(0.5f, 6);
        if (GUILayout.Button("Pentagon"))
            CreateCircle(0.5f, 5);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateSquare(float size)
    {
        Undo.RecordObject(physicsShape, "Create Square");
        float half = size / 2f;
        physicsShape.vertices = new Vector2[]
        {
            new Vector2(-half, -half),
            new Vector2(half, -half),
            new Vector2(half, half),
            new Vector2(-half, half)
        };
        EditorUtility.SetDirty(physicsShape);
    }

    private void CreateRectangle(float width, float height)
    {
        Undo.RecordObject(physicsShape, "Create Rectangle");
        float halfW = width / 2f;
        float halfH = height / 2f;
        physicsShape.vertices = new Vector2[]
        {
            new Vector2(-halfW, -halfH),
            new Vector2(halfW, -halfH),
            new Vector2(halfW, halfH),
            new Vector2(-halfW, halfH)
        };
        EditorUtility.SetDirty(physicsShape);
    }

    private void CreateTriangle(float size)
    {
        Undo.RecordObject(physicsShape, "Create Triangle");
        physicsShape.vertices = new Vector2[]
        {
            new Vector2(0f, size),
            new Vector2(-size, -size),
            new Vector2(size, -size)
        };
        EditorUtility.SetDirty(physicsShape);
    }

    private void CreateCircle(float radius, int segments)
    {
        Undo.RecordObject(physicsShape, $"Create Circle ({segments} sides)");
        var vertices = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            vertices[i] = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }
        physicsShape.vertices = vertices;
        EditorUtility.SetDirty(physicsShape);
    }
}
#endif