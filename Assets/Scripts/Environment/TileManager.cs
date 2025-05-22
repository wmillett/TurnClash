using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private GameObject tilePrefab;
    
    private Tile[,] tiles;
    private Unit selectedUnit;
    private List<Tile> highlightedTiles = new List<Tile>();
    
    private void Start()
    {
        InitializeGrid();
    }
    
    private void InitializeGrid()
    {
        tiles = new Tile[gridWidth, gridHeight];
        
        // Create the grid of tiles
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                
                if (tile != null)
                {
                    tile.SetGridPosition(new Vector2Int(x, z));
                    tiles[x, z] = tile;
                }
            }
        }
    }
    
    public Tile GetTileAt(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridWidth && 
            position.y >= 0 && position.y < gridHeight)
        {
            return tiles[position.x, position.y];
        }
        return null;
    }
    
    public void SelectUnit(Unit unit)
    {
        // Deselect previous unit
        if (selectedUnit != null)
        {
            ClearHighlights();
        }
        
        selectedUnit = unit;
        
        if (selectedUnit != null)
        {
            ShowMovementRange();
        }
    }
    
    private void ShowMovementRange()
    {
        if (selectedUnit == null) return;
        
        ClearHighlights();
        
        Vector2Int startPos = selectedUnit.CurrentTile.GridPosition;
        int range = selectedUnit.MovementRange;
        
        // Simple range calculation (can be improved with pathfinding)
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int pos = startPos + new Vector2Int(x, y);
                Tile tile = GetTileAt(pos);
                
                if (tile != null && tile.IsWalkable && !tile.IsOccupied())
                {
                    tile.Highlight(true);
                    highlightedTiles.Add(tile);
                }
            }
        }
    }
    
    private void ClearHighlights()
    {
        foreach (Tile tile in highlightedTiles)
        {
            tile.Highlight(false);
        }
        highlightedTiles.Clear();
    }
    
    public void HandleTileClick(Tile clickedTile)
    {
        if (selectedUnit != null && highlightedTiles.Contains(clickedTile))
        {
            selectedUnit.MoveToTile(clickedTile);
            ClearHighlights();
            selectedUnit = null;
        }
    }
    
    public void ResetTurn()
    {
        // Reset all units' movement
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            unit.ResetTurn();
        }
    }
} 