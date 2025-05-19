#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class JointArcGizmo : MonoBehaviour
{
    [Header("Visibility")]
    public bool showFlexionAxis = true;
    public bool showAbductionAxis = true;
    public bool showRotationAxis = true;
    public bool showCenterAxes = true;

    [Header("Global Ring Offset")]
    public float globalOffset = 0f;

    [Header("Per-Axis Ring Offsets")]
    public float offsetFlexion = 0.05f;  // around local X
    public float offsetAbduction = 0.05f;  // around local Y
    public float offsetRotation = 0.05f;  // around local Z

    [Header("Ring Appearance")]
    public float radius = 0.2f;
    [Tooltip("Screen-pixel thickness of each semi-circle")]
    public float lineThickness = 4f;
    [Tooltip("Number of segments per semi-circle")]
    public int segments = 64;
    public Color colorFlexion = new Color(1, 0, 0, 0.6f);
    public Color colorAbduction = new Color(0, 1, 0, 0.6f);
    public Color colorIntRotation = new Color(0, 0, 1, 0.6f);

    [Header("Arrow Settings")]
    [Range(0.05f, 0.3f), Tooltip("Height of the cone head as fraction of radius")]
    public float arrowSizeRatio = 0.15f;
    [Range(0f, 0.2f), Tooltip("How far outside the ring the tip sits")]
    public float arrowOffsetRatio = 0.05f;

    [Header("Center Axes Appearance")]
    [Tooltip("Half-length of each axis line")]
    public float axisLength = 0.1f;
    [Tooltip("Screen-pixel thickness of the tripod lines")]
    public float centerAxisThickness = 2f;
    public Color xAxisColor = Color.red;
    public Color yAxisColor = Color.green;
    public Color zAxisColor = Color.blue;

    [Header("Center Axes Offsets (along each axis)")]
    public float offsetCenterX = 0f;
    public float offsetCenterY = 0f;
    public float offsetCenterZ = 0f;

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Vector3 p = transform.position;

        if (showFlexionAxis)
            DrawHalfRingWithOppositeArrows(
                p,
                transform.right,   // axis
                transform.up,      // fromDir
                colorFlexion,
                globalOffset + offsetFlexion
            );

        if (showAbductionAxis)
            DrawHalfRingWithOppositeArrows(
                p,
                transform.up,
                transform.forward,
                colorAbduction,
                globalOffset + offsetAbduction
            );

        if (showRotationAxis)
            DrawHalfRingWithOppositeArrows(
                p,
                transform.forward,
                transform.right,
                colorIntRotation,
                globalOffset + offsetRotation
            );

        if (showCenterAxes)
            DrawCenterAxes(p);
#endif
    }

#if UNITY_EDITOR
    void DrawHalfRingWithOppositeArrows(
        Vector3 center,
        Vector3 axis,
        Vector3 fromDir,
        Color col,
        float offsetAlongAxis
    )
    {
        // ring center offset along its rotation axis
        Vector3 c = center + axis.normalized * offsetAlongAxis;

        // sample & draw the 0→180° semi-circle
        Vector3[] pts = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(0f, 180f, t);
            Vector3 dir = Quaternion.AngleAxis(angle, axis) * fromDir.normalized;
            pts[i] = c + dir * radius;
        }
        Handles.color = col;
        Handles.DrawAAPolyLine(lineThickness, pts);

        // arrows at 0° and 180°
        DrawTangentArrow(c, axis, fromDir, col, 0f);
        DrawTangentArrow(c, axis, fromDir, col, 180f);
    }

    void DrawTangentArrow(
        Vector3 c,
        Vector3 axis,
        Vector3 fromDir,
        Color col,
        float angleDeg
    )
    {
        // compute radial dir & tip position
        Vector3 d = Quaternion.AngleAxis(angleDeg, axis) * fromDir.normalized;
        Vector3 tip = c + d * (radius + radius * arrowOffsetRatio);

        // true tangent along the ring
        Vector3 tangent = Vector3.Cross(axis, d).normalized;

        // invert first arrow so they face opposite
        if (Mathf.Approximately(angleDeg, 0f))
            tangent = -tangent;

        // build rotation: forward = tangent (arrow nose), up = axis
        Quaternion rot = Quaternion.LookRotation(tangent, axis);

        // cone head height = arrowSizeRatio * radius
        float headHeight = radius * arrowSizeRatio;

        Handles.color = col;
        Handles.ConeHandleCap(0, tip, rot, headHeight, EventType.Repaint);
    }

    void DrawCenterAxes(Vector3 center)
    {
        // X-axis
        Handles.color = xAxisColor;
        Vector3 cx = center + transform.right * offsetCenterX;
        Handles.DrawAAPolyLine(
            centerAxisThickness,
            cx - transform.right * axisLength,
            cx + transform.right * axisLength
        );

        // Y-axis
        Handles.color = yAxisColor;
        Vector3 cy = center + transform.up * offsetCenterY;
        Handles.DrawAAPolyLine(
            centerAxisThickness,
            cy - transform.up * axisLength,
            cy + transform.up * axisLength
        );

        // Z-axis
        Handles.color = zAxisColor;
        Vector3 cz = center + transform.forward * offsetCenterZ;
        Handles.DrawAAPolyLine(
            centerAxisThickness,
            cz - transform.forward * axisLength,
            cz + transform.forward * axisLength
        );
    }
#endif
}
