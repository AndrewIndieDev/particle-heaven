using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Curve generator for Box2DPhysicsShape
/// Creates smooth curves with adjustable parameters
/// </summary>
[RequireComponent(typeof(Box2DPhysicsShape))]
public class Box2DCurveGenerator : MonoBehaviour
{
    [Header("Curve Settings")]
    [Tooltip("Width of the curve")]
    public float curveWidth = 5f;

    [Tooltip("Height/depth of the curve")]
    public float curveHeight = 5f;

    [Tooltip("Number of segments in the curve")]
    public int curveSegments = 16;

    [Tooltip("Curve type")]
    public CurveType curveType = CurveType.Arc;

    [Tooltip("Create filled or hollow curve")]
    public bool fillCurve = true;

    [Tooltip("Curve angle (0-360 degrees) - Arc only")]
    [Range(0f, 360f)]
    public float curveAngle = 90f;

    [Header("Generate")]
    [Tooltip("Generate the curve")]
    public bool generateCurve = false;

    public enum CurveType
    {
        Arc,            // Circular arc
        Parabola,       // Parabolic curve
        Sine,           // Sine wave
        Exponential     // Exponential curve
    }

    private Box2DPhysicsShape physicsShape;

    void OnValidate()
    {
        if (physicsShape == null)
            physicsShape = GetComponent<Box2DPhysicsShape>();

        if (generateCurve)
        {
            generateCurve = false;
            GenerateCurve();
        }
    }

    void Reset()
    {
        physicsShape = GetComponent<Box2DPhysicsShape>();
    }

    /// <summary>
    /// Generate curve based on current settings
    /// </summary>
    public void GenerateCurve()
    {
        if (physicsShape == null)
            return;

        Vector2[] curveVertices = null;

        switch (curveType)
        {
            case CurveType.Arc:
                curveVertices = GenerateArcCurve();
                break;
            case CurveType.Parabola:
                curveVertices = GenerateParabolicCurve();
                break;
            case CurveType.Sine:
                curveVertices = GenerateSineCurve();
                break;
            case CurveType.Exponential:
                curveVertices = GenerateExponentialCurve();
                break;
        }

        if (curveVertices != null)
        {
            physicsShape.vertices = curveVertices;
            Debug.Log($"Generated {curveType} curve with {curveVertices.Length} vertices");
        }
    }

    private Vector2[] GenerateArcCurve()
    {
        // Generate circular arc
        float angleRad = curveAngle * Mathf.Deg2Rad;

        if (fillCurve)
        {
            // Filled arc (wedge shape)
            Vector2[] vertices = new Vector2[curveSegments + 2];

            // Center point
            vertices[0] = Vector2.zero;

            // Arc points
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float angle = t * angleRad;

                vertices[i + 1] = new Vector2(
                    Mathf.Cos(angle) * curveWidth,
                    Mathf.Sin(angle) * curveHeight
                );
            }

            return vertices;
        }
        else
        {
            // Hollow arc (curved strip)
            Vector2[] vertices = new Vector2[curveSegments * 2 + 2];

            // Outer arc
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float angle = t * angleRad;

                vertices[i] = new Vector2(
                    Mathf.Cos(angle) * curveWidth,
                    Mathf.Sin(angle) * curveHeight
                );
            }

            // Inner arc (reverse direction)
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float angle = t * angleRad;

                vertices[curveSegments + 1 + i] = new Vector2(
                    Mathf.Cos(angleRad - angle) * curveWidth * 0.7f,
                    Mathf.Sin(angleRad - angle) * curveHeight * 0.7f
                );
            }

            return vertices;
        }
    }

    private Vector2[] GenerateParabolicCurve()
    {
        if (fillCurve)
        {
            Vector2[] vertices = new Vector2[curveSegments + 3];

            // Start point (bottom left)
            vertices[0] = new Vector2(-curveWidth / 2f, 0);

            // Curve points
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);

                // Parabola equation: y = -4h * (x/w)^2 + h
                float normalizedX = (x / (curveWidth / 2f));
                float y = curveHeight * (1f - normalizedX * normalizedX);

                vertices[i + 1] = new Vector2(x, y);
            }

            // End point (bottom right)
            vertices[curveSegments + 2] = new Vector2(curveWidth / 2f, 0);

            return vertices;
        }
        else
        {
            // Hollow parabolic curve
            Vector2[] vertices = new Vector2[curveSegments * 2 + 2];
            float thickness = curveHeight * 0.1f;

            // Top curve
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);
                float normalizedX = (x / (curveWidth / 2f));
                float y = curveHeight * (1f - normalizedX * normalizedX);

                vertices[i] = new Vector2(x, y);
            }

            // Bottom curve
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(curveWidth / 2f, -curveWidth / 2f, t);
                float normalizedX = (x / (curveWidth / 2f));
                float y = curveHeight * (1f - normalizedX * normalizedX) - thickness;

                vertices[curveSegments + 1 + i] = new Vector2(x, y);
            }

            return vertices;
        }
    }

    private Vector2[] GenerateSineCurve()
    {
        if (fillCurve)
        {
            Vector2[] vertices = new Vector2[curveSegments + 3];

            vertices[0] = new Vector2(-curveWidth / 2f, 0);

            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);
                float y = Mathf.Sin(t * Mathf.PI * 2f) * curveHeight;

                vertices[i + 1] = new Vector2(x, y);
            }

            vertices[curveSegments + 2] = new Vector2(curveWidth / 2f, 0);

            return vertices;
        }
        else
        {
            Vector2[] vertices = new Vector2[curveSegments * 2 + 2];
            float thickness = curveHeight * 0.2f;

            // Top sine wave
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);
                float y = Mathf.Sin(t * Mathf.PI * 2f) * curveHeight + thickness / 2f;

                vertices[i] = new Vector2(x, y);
            }

            // Bottom sine wave
            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(curveWidth / 2f, -curveWidth / 2f, t);
                float y = Mathf.Sin((1f - t) * Mathf.PI * 2f) * curveHeight - thickness / 2f;

                vertices[curveSegments + 1 + i] = new Vector2(x, y);
            }

            return vertices;
        }
    }

    private Vector2[] GenerateExponentialCurve()
    {
        if (fillCurve)
        {
            Vector2[] vertices = new Vector2[curveSegments + 3];

            vertices[0] = new Vector2(-curveWidth / 2f, 0);

            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);
                float y = Mathf.Pow(t, 2f) * curveHeight;

                vertices[i + 1] = new Vector2(x, y);
            }

            vertices[curveSegments + 2] = new Vector2(curveWidth / 2f, 0);

            return vertices;
        }
        else
        {
            Vector2[] vertices = new Vector2[curveSegments * 2 + 2];
            float thickness = curveHeight * 0.1f;

            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(-curveWidth / 2f, curveWidth / 2f, t);
                float y = Mathf.Pow(t, 2f) * curveHeight;

                vertices[i] = new Vector2(x, y);
            }

            for (int i = 0; i <= curveSegments; i++)
            {
                float t = i / (float)curveSegments;
                float x = Mathf.Lerp(curveWidth / 2f, -curveWidth / 2f, t);
                float y = Mathf.Pow(1f - t, 2f) * curveHeight - thickness;

                vertices[curveSegments + 1 + i] = new Vector2(x, y);
            }

            return vertices;
        }
    }

    /// <summary>
    /// Generate curve with custom parameters (can be called from code)
    /// </summary>
    public void GenerateCurveWithParams(CurveType type, float width, float height, int segments, bool filled, float angle = 90f)
    {
        curveType = type;
        curveWidth = width;
        curveHeight = height;
        curveSegments = segments;
        fillCurve = filled;
        curveAngle = angle;

        GenerateCurve();
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for Box2DCurveGenerator
/// </summary>
[CustomEditor(typeof(Box2DCurveGenerator))]
public class Box2DCurveGeneratorInspector : Editor
{
    private Box2DCurveGenerator curveGen;

    void OnEnable()
    {
        curveGen = (Box2DCurveGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Adjust curve settings above, then click Generate or check the box.\n\n" +
            "Curve Types:\n" +
            "• Arc - Circular curves (adjustable angle)\n" +
            "• Parabola - Smooth hills and arcs\n" +
            "• Sine - Wavy terrain\n" +
            "• Exponential - Steep ramps",
            MessageType.Info
        );

        // Big generate button
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("GENERATE", GUILayout.Height(40)))
        {
            Undo.RecordObject(curveGen.GetComponent<Box2DPhysicsShape>(), "Generate Curve");
            curveGen.GenerateCurve();
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        GUI.backgroundColor = Color.white;

        // Quick presets
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Quarter Circle\n(90°)"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Arc, 5f, 5f, 16, true, 90f);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        if (GUILayout.Button("Half Pipe\n(180°)"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Arc, 8f, 4f, 20, false, 180f);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Smooth Hill"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Parabola, 10f, 3f, 16, true);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        if (GUILayout.Button("Jump Arc"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Parabola, 6f, 4f, 12, true);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wavy Ground"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Sine, 15f, 1f, 24, true);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        if (GUILayout.Button("Steep Ramp"))
        {
            curveGen.GenerateCurveWithParams(Box2DCurveGenerator.CurveType.Exponential, 8f, 6f, 16, true);
            EditorUtility.SetDirty(curveGen.GetComponent<Box2DPhysicsShape>());
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif