using UnityEngine;
using System.Collections.Generic;

public class GroundManager : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Sprite[] groundSprites;
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float tileSize = 1f;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraPadding = 2f;

    private Dictionary<Vector2Int, GroundTile> tiles = new Dictionary<Vector2Int, GroundTile>();
    private Vector2 gridOffset;

    // Public properties for GridVisualizer
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float TileSize => tileSize;
    public Vector2 GridOffset => gridOffset;

    private void Start()
    {
        ValidateSettings();
        
        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateGround();
        CenterCamera();
    }

    private void ValidateSettings()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab is not assigned in GroundManager!");
            enabled = false;
            return;
        }

        if (groundSprites == null || groundSprites.Length == 0)
        {
            Debug.LogWarning("No ground sprites assigned. Using default sprite.");
            // Create a default white sprite
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            
            groundSprites = new Sprite[] { 
                Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f))
            };
        }

        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.LogWarning("Invalid grid dimensions. Using default values.");
            gridWidth = Mathf.Max(1, gridWidth);
            gridHeight = Mathf.Max(1, gridHeight);
        }

        if (tileSize <= 0)
        {
            Debug.LogWarning("Invalid tile size. Using default value.");
            tileSize = 1f;
        }
    }

    private void CreateGround()
    {
        // Calculate grid offset to center the ground
        gridOffset = new Vector2(
            -(gridWidth * tileSize) / 2f,
            -(gridHeight * tileSize) / 2f
        );

        // Create tiles
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateTile(x, y);
            }
        }
    }

    private void CreateTile(int x, int y)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        Vector3 worldPos = new Vector3(
            gridOffset.x + (x * tileSize),
            gridOffset.y + (y * tileSize),
            0f
        );

        GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
        tileObj.name = $"Tile_{x}_{y}";

        GroundTile tile = tileObj.GetComponent<GroundTile>();
        if (tile != null)
        {
            // Get a random sprite from the array, or use the first one if only one exists
            Sprite tileSprite = groundSprites.Length > 1 
                ? groundSprites[Random.Range(0, groundSprites.Length)]
                : groundSprites[0];
                
            tile.Initialize(gridPos, tileSprite);
            tiles[gridPos] = tile;
        }
        else
        {
            Debug.LogError($"GroundTile component missing on tile at position ({x}, {y})");
        }
    }

    private void CenterCamera()
    {
        if (mainCamera != null)
        {
            // Calculate the center of the grid
            Vector3 center = new Vector3(
                gridOffset.x + (gridWidth * tileSize) / 2f,
                gridOffset.y + (gridHeight * tileSize) / 2f,
                mainCamera.transform.position.z
            );

            // Set camera position
            mainCamera.transform.position = center;

            // Adjust orthographic size to fit the grid with padding
            float screenRatio = (float)Screen.width / Screen.height;
            float targetOrthographicSize = (gridHeight * tileSize) / 2f + cameraPadding;
            mainCamera.orthographicSize = targetOrthographicSize;
        }
    }

    public GroundTile GetTileAtPosition(Vector2Int position)
    {
        if (tiles.TryGetValue(position, out GroundTile tile))
            return tile;
        return null;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPosition.x - gridOffset.x) / tileSize),
            Mathf.FloorToInt((worldPosition.y - gridOffset.y) / tileSize)
        );
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridOffset.x + (gridPosition.x * tileSize),
            gridOffset.y + (gridPosition.y * tileSize),
            0f
        );
    }

    public void RegenerateGrid()
    {
        // Clear existing tiles
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        tiles.Clear();

        // Recreate ground
        CreateGround();
        CenterCamera();
    }

    public void SetGridSize(int width, int height)
    {
        gridWidth = Mathf.Max(1, width);
        gridHeight = Mathf.Max(1, height);
        RegenerateGrid();
    }

    public void SetTileSize(float size)
    {
        tileSize = Mathf.Max(0.1f, size);
        RegenerateGrid();
    }
} 