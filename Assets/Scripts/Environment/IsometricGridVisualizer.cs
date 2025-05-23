using UnityEngine;

public class IsometricGridVisualizer : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Color gridColor = new Color(0f, 0f, 0f, 0.8f); // More visible black
    [SerializeField] private float lineWidth = 0.01f; // Thinner default lines
    [SerializeField] private bool showGrid = true;
    [SerializeField] private float lineHeightOffset = 0.01f; // Height above tiles to prevent z-fighting

    private IsometricGroundManager groundManager;
    private LineRenderer[] gridLines;
    private Material lineMaterial;

    private void Start()
    {
        groundManager = GetComponent<IsometricGroundManager>();
        if (groundManager == null)
        {
            Debug.LogError("IsometricGridVisualizer requires an IsometricGroundManager component!");
            return;
        }

        CreateLineMaterial();
        CreateGridLines();
    }

    private void CreateLineMaterial()
    {
        // Create an unlit material that's always visible
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = gridColor;
    }

    private void CreateGridLines()
    {
        // Calculate grid dimensions
        float gridWidth = groundManager.GridWidth * groundManager.TileSize;
        float gridHeight = groundManager.GridHeight * groundManager.TileSize;
        Vector3 gridOffset = groundManager.GridOffset;

        // Get the height of tiles for proper line positioning
        float tileTopY = 0f;
        if (groundManager.GridWidth > 0 && groundManager.GridHeight > 0)
        {
            // Get a sample tile to determine height
            var sampleTile = groundManager.GetTileAtPosition(new Vector2Int(0, 0));
            if (sampleTile != null)
            {
                MeshRenderer renderer = sampleTile.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    tileTopY = renderer.bounds.max.y;
                }
                else
                {
                    tileTopY = sampleTile.transform.localScale.y * 0.5f;
                }
            }
        }
        float lineY = tileTopY + lineHeightOffset;

        // IMPORTANT: Offset grid lines by half tile size to align with tile edges
        // This is because tiles are positioned with their center at grid coordinates
        float halfTileSize = groundManager.TileSize * 0.5f;

        // Create line renderers for vertical and horizontal lines
        int totalLines = groundManager.GridWidth + 1 + groundManager.GridHeight + 1;
        gridLines = new LineRenderer[totalLines];

        // Create vertical lines
        for (int i = 0; i <= groundManager.GridWidth; i++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            
            // Adjust position to account for tile centering
            float xPos = gridOffset.x + (i * groundManager.TileSize) - halfTileSize;
            line.SetPosition(0, new Vector3(xPos, lineY, gridOffset.z - halfTileSize));
            line.SetPosition(1, new Vector3(xPos, lineY, gridOffset.z + gridHeight - halfTileSize));
            
            gridLines[i] = line;
        }

        // Create horizontal lines
        for (int i = 0; i <= groundManager.GridHeight; i++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startColor = line.endColor = gridColor;
            line.startWidth = line.endWidth = lineWidth;
            line.positionCount = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            
            // Adjust position to account for tile centering
            float zPos = gridOffset.z + (i * groundManager.TileSize) - halfTileSize;
            line.SetPosition(0, new Vector3(gridOffset.x - halfTileSize, lineY, zPos));
            line.SetPosition(1, new Vector3(gridOffset.x + gridWidth - halfTileSize, lineY, zPos));
            
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

    public void SetGridColor(Color color)
    {
        gridColor = color;
        if (lineMaterial != null)
        {
            lineMaterial.color = color;
        }
        
        if (gridLines != null)
        {
            foreach (LineRenderer line in gridLines)
            {
                if (line != null)
                {
                    line.startColor = line.endColor = color;
                }
            }
        }
    }

    public void SetLineWidth(float width)
    {
        lineWidth = width;
        if (gridLines != null)
        {
            foreach (LineRenderer line in gridLines)
            {
                if (line != null)
                {
                    line.startWidth = line.endWidth = width;
                }
            }
        }
    }
} 