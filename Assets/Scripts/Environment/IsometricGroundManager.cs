using UnityEngine;
using System.Collections.Generic;

public class IsometricGroundManager : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Material[] groundMaterials; // Changed from sprites to materials
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float tileHeight = 0.5f; // Height of the tile for isometric view

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraPadding = 2f;
    [SerializeField] private float cameraAngle = 30f; // Changed to 30 degrees for better isometric view
    [SerializeField] private float cameraDistance = 15f; // Added explicit camera distance

    private Dictionary<Vector2Int, IsometricGroundTile> tiles = new Dictionary<Vector2Int, IsometricGroundTile>();
    private Vector3 gridOffset;

    // Public properties for GridVisualizer
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float TileSize => tileSize;
    public Vector3 GridOffset => gridOffset;

    private void Start()
    {
        ValidateSettings();
        
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Create ground first at world origin
        CreateGround();
        
        // Then set up camera to view the entire ground
        SetupCamera();
    }

    private void ValidateSettings()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab is not assigned in IsometricGroundManager!");
            enabled = false;
            return;
        }

        if (groundMaterials == null || groundMaterials.Length == 0)
        {
            Debug.LogWarning("No ground materials assigned. Creating default material.");
            // Create a default material
            Material defaultMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            defaultMaterial.color = Color.gray;
            groundMaterials = new Material[] { defaultMaterial };
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
        // Calculate grid offset centered at world origin
        gridOffset = new Vector3(
            -(gridWidth * tileSize) / 2f,
            0f,
            -(gridHeight * tileSize) / 2f
        );

        // Create tiles
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                CreateTile(x, z);
            }
        }
    }

    private void CreateTile(int x, int z)
    {
        Vector2Int gridPos = new Vector2Int(x, z);
        Vector3 worldPos = new Vector3(
            gridOffset.x + (x * tileSize),
            0f,
            gridOffset.z + (z * tileSize)
        );

        GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
        tileObj.name = $"Tile_{x}_{z}";

        // Set the tile's scale for isometric view
        tileObj.transform.localScale = new Vector3(tileSize, tileHeight, tileSize);

        IsometricGroundTile tile = tileObj.GetComponent<IsometricGroundTile>();
        if (tile != null)
        {
            // Get a random material from the array, or use the first one if only one exists
            Material tileMaterial = groundMaterials.Length > 1 
                ? groundMaterials[Random.Range(0, groundMaterials.Length)]
                : groundMaterials[0];
                
            // Apply the material to the tile's renderer
            MeshRenderer renderer = tileObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = tileMaterial;
            }
                
            tile.Initialize(gridPos);
            tiles[gridPos] = tile;
        }
        else
        {
            Debug.LogError($"IsometricGroundTile component missing on tile at position ({x}, {z})");
        }
    }

    private void SetupCamera()
    {
        if (mainCamera != null)
        {
            // Calculate the center of the grid
            Vector3 gridCenter = new Vector3(
                gridOffset.x + (gridWidth * tileSize) / 2f,
                cameraDistance,
                gridOffset.z + (gridHeight * tileSize) / 2f
            );
            
            // Position camera at grid center
            mainCamera.transform.position = gridCenter;
            mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, 45f, 0f);
            
            // Set camera to orthographic
            mainCamera.orthographic = true;
            
            // Calculate orthographic size to fit the entire grid with padding
            float gridSize = Mathf.Max(gridWidth, gridHeight) * tileSize;
            mainCamera.orthographicSize = (gridSize / 2f) + cameraPadding;
        }
    }

    public IsometricGroundTile GetTileAtPosition(Vector2Int position)
    {
        if (tiles.TryGetValue(position, out IsometricGroundTile tile))
            return tile;
        return null;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPosition.x - gridOffset.x) / tileSize),
            Mathf.FloorToInt((worldPosition.z - gridOffset.z) / tileSize)
        );
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridOffset.x + (gridPosition.x * tileSize),
            0f,
            gridOffset.z + (gridPosition.y * tileSize)
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
        SetupCamera();
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