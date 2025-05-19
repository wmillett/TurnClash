using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GroundStructureVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool showStructure = true;
    [SerializeField] private Color structureColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    [SerializeField] private float labelOffset = 0.5f;

    private GroundManager groundManager;

    private void OnEnable()
    {
        groundManager = GetComponent<GroundManager>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showStructure || groundManager == null) return;

        // Draw grid boundaries
        Gizmos.color = structureColor;
        float width = groundManager.GridWidth * groundManager.TileSize;
        float height = groundManager.GridHeight * groundManager.TileSize;
        Vector2 offset = groundManager.GridOffset;

        // Draw main rectangle
        Vector3 center = new Vector3(
            offset.x + width / 2,
            offset.y + height / 2,
            0
        );
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0));

        // Draw tile boundaries
        for (int x = 0; x <= groundManager.GridWidth; x++)
        {
            for (int y = 0; y <= groundManager.GridHeight; y++)
            {
                Vector3 pos = new Vector3(
                    offset.x + x * groundManager.TileSize,
                    offset.y + y * groundManager.TileSize,
                    0
                );
                Gizmos.DrawWireCube(pos, new Vector3(groundManager.TileSize, groundManager.TileSize, 0) * 0.95f);
            }
        }

        // Draw labels
        GUIStyle style = new GUIStyle();
        style.normal.textColor = structureColor;
        style.alignment = TextAnchor.MiddleCenter;

        for (int x = 0; x < groundManager.GridWidth; x++)
        {
            for (int y = 0; y < groundManager.GridHeight; y++)
            {
                Vector3 pos = new Vector3(
                    offset.x + x * groundManager.TileSize + groundManager.TileSize / 2,
                    offset.y + y * groundManager.TileSize + groundManager.TileSize / 2,
                    0
                );
                Handles.Label(pos + Vector3.up * labelOffset, $"({x},{y})", style);
            }
        }
    }
#endif
} 