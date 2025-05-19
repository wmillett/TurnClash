using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private bool showGrid = true;

    private GroundManager groundManager;
    private LineRenderer[] gridLines;

    private void Start()
    {
        groundManager = GetComponent<GroundManager>();
        if (groundManager == null)
        {
            Debug.LogError("GridVisualizer requires a GroundManager component!");
            return;
        }

        CreateGridLines();
    }

    private void CreateGridLines()
    {
        // Calculate grid dimensions
        float gridWidth = groundManager.GridWidth * groundManager.TileSize;
        float gridHeight = groundManager.GridHeight * groundManager.TileSize;
        Vector2 gridOffset = groundManager.GridOffset;

        // Create line renderers for vertical and horizontal lines
        int totalLines = groundManager.GridWidth + 1 + groundManager.GridHeight + 1;
        gridLines = new LineRenderer[totalLines];

        // Create vertical lines
        for (int i = 0; i <= groundManager.GridWidth; i++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            
            float xPos = gridOffset.x + (i * groundManager.TileSize);
            line.SetPosition(0, new Vector3(xPos, gridOffset.y, 0));
            line.SetPosition(1, new Vector3(xPos, gridOffset.y + gridHeight, 0));
            
            gridLines[i] = line;
        }

        // Create horizontal lines
        for (int i = 0; i <= groundManager.GridHeight; i++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            
            float yPos = gridOffset.y + (i * groundManager.TileSize);
            line.SetPosition(0, new Vector3(gridOffset.x, yPos, 0));
            line.SetPosition(1, new Vector3(gridOffset.x + gridWidth, yPos, 0));
            
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