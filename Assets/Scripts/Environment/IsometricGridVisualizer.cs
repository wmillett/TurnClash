using UnityEngine;

public class IsometricGridVisualizer : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Color gridColor = Color.black;
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private bool showGrid = true;

    private IsometricGroundManager groundManager;
    private LineRenderer[] gridLines;

    private void Start()
    {
        groundManager = GetComponent<IsometricGroundManager>();
        if (groundManager == null)
        {
            Debug.LogError("IsometricGridVisualizer requires an IsometricGroundManager component!");
            return;
        }

        CreateGridLines();
    }

    private void CreateGridLines()
    {
        // Calculate grid dimensions
        float gridWidth = groundManager.GridWidth * groundManager.TileSize;
        float gridHeight = groundManager.GridHeight * groundManager.TileSize;
        Vector3 gridOffset = groundManager.GridOffset;

        // Create line renderers for vertical and horizontal lines
        int totalLines = groundManager.GridWidth + 1 + groundManager.GridHeight + 1;
        gridLines = new LineRenderer[totalLines];

        // Create vertical lines
        for (int i = 0; i <= groundManager.GridWidth; i++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            
            float xPos = gridOffset.x + (i * groundManager.TileSize);
            line.SetPosition(0, new Vector3(xPos, 0.01f, gridOffset.z)); // Slightly above ground to prevent z-fighting
            line.SetPosition(1, new Vector3(xPos, 0.01f, gridOffset.z + gridHeight));
            
            gridLines[i] = line;
        }

        // Create horizontal lines
        for (int i = 0; i <= groundManager.GridHeight; i++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            
            float zPos = gridOffset.z + (i * groundManager.TileSize);
            line.SetPosition(0, new Vector3(gridOffset.x, 0.01f, zPos));
            line.SetPosition(1, new Vector3(gridOffset.x + gridWidth, 0.01f, zPos));
            
            gridLines[groundManager.GridWidth + 1 + i] = line;
        }
    }

    private void Update()
    {
        // Toggle grid visibility
        if (gridLines != null)
        {
            foreach (LineRenderer line in gridLines)
            {
                if (line != null)
                    line.enabled = showGrid;
            }
        }
    }

    public void ToggleGrid()
    {
        showGrid = !showGrid;
    }
} 