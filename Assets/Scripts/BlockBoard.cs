using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.InputSystem; 

public class BlockBoard : MonoBehaviour
{
    public Tilemap tilemap;
    public BlockData[] blockDataList;
    
    private const int Width = 10;
    private const int Height = 24;
    private int[,] grid;
    private int score = 0;
    
    private readonly Vector2Int visualOffset = new Vector2Int(-5, -10);
    private readonly Vector2Int holdVisualOffset = new Vector2Int(-9, 7); 
    private readonly Vector2Int queueVisualOffset = new Vector2Int(8, 7);

    private int currentBlockIndex;
    private Vector2Int position;
    private int rotation;
    private bool gameEnd = false;

    private List<int> queue = new List<int>();
    private int hold = -1;
    private bool held = false;

    public float gravity = 1.0f;
    private float lastFallTime;

    void Start()
    {
        InitializeGrid();
        RefillQueue();
        SpawnBlock();
    }

    void Update()
    {
        if (gameEnd) return;
        HandleInput();
        if (Time.time - lastFallTime > gravity)
        {
            Tick();
            lastFallTime = Time.time;
        }
    }

    private void InitializeGrid()
    {
        grid = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = -1;
            }
        }
    }

    private void SpawnBlock()
    {
        int nextBlock = GetNextBlock();
        InitializeBlock(nextBlock);
        
        held = false;
        DrawQueue();
    }

    private void InitializeBlock(int blockIndex)
    {
        currentBlockIndex = blockIndex;
        position = new Vector2Int(4, 19);
        rotation = 0;

        lastFallTime = Time.time;

        if (!CheckCollision(position.x, position.y, rotation))
        {
            gameEnd = true;
            Debug.Log("Game Over! Score: " + score);
        }
        
        DrawActiveBlock();
    }

    private void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame) MoveLeft();
        if (kb.rightArrowKey.wasPressedThisFrame) MoveRight();
        if (kb.downArrowKey.wasPressedThisFrame) MoveDown();
        if (kb.upArrowKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame) RotateCW();
        if (kb.zKey.wasPressedThisFrame) RotateCCW();
        if (kb.spaceKey.wasPressedThisFrame) HardDrop();
        if (kb.cKey.wasPressedThisFrame) HoldBlock();
    }

    private void Tick()
    {
        ClearActiveBlock();
        
        if (CheckCollision(position.x, position.y - 1, rotation))
        {
            position.y--;
        }
        else
        {
            PlaceBlock();
            ClearLines();
            SpawnBlock();
        }
        
        DrawActiveBlock();
    }

    private void PlaceBlock()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2Int offset = BlockGameData.Config[currentBlockIndex, rotation, i];
            int logicX = position.x + offset.x;
            int logicY = position.y + offset.y;
            
            grid[logicX, logicY] = currentBlockIndex;
            tilemap.SetTile(LogicalToVisual(logicX, logicY), GetTileForBlock(currentBlockIndex));
        }
    }

    private bool CheckCollision(int targetX, int targetY, int targetRot)
    {
        for (int i = 0; i < 4; i++)
        {
            int x = targetX + BlockGameData.Config[currentBlockIndex, targetRot, i].x;
            int y = targetY + BlockGameData.Config[currentBlockIndex, targetRot, i].y;

            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            if (grid[x, y] != -1) return false;
        }
        return true;
    }

    private void ClearLines()
    {
        for (int y = 0; y < Height; y++)
        {
            bool fullLine = true;
            for (int x = 0; x < Width; x++)
            {
                if (grid[x, y] == -1) fullLine = false;
            }

            if (fullLine)
            {
                for (int j = y; j < Height - 1; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        grid[i, j] = grid[i, j + 1];
                        
                        Vector3Int visualPos = LogicalToVisual(i, j);
                        if (grid[i, j] == -1) tilemap.SetTile(visualPos, null);
                        else tilemap.SetTile(visualPos, GetTileForBlock(grid[i, j]));
                    }
                }
                
                for (int i = 0; i < Width; i++) 
                {
                    grid[i, Height - 1] = -1;
                    tilemap.SetTile(LogicalToVisual(i, Height - 1), null);
                }

                score++;
                y--; 
            }
        }
    }

    private void MoveLeft()
    {
        ClearActiveBlock();
        if (CheckCollision(position.x - 1, position.y, rotation)) position.x--;
        DrawActiveBlock();
    }

    private void MoveRight()
    {
        ClearActiveBlock();
        if (CheckCollision(position.x + 1, position.y, rotation)) position.x++;
        DrawActiveBlock();
    }

    private void MoveDown()
    {
        ClearActiveBlock();
        if (CheckCollision(position.x, position.y - 1, rotation)) 
        {
            position.y--;
            lastFallTime = Time.time;
        }
        DrawActiveBlock();
    }

    private void HardDrop()
    {
        ClearActiveBlock();
        while (CheckCollision(position.x, position.y - 1, rotation)) position.y--;
        Tick(); 
    }

    private void RotateCW()
    {
        ClearActiveBlock();
        int newRot = (rotation + 1) % 4;
        
        if (CheckCollision(position.x, position.y, newRot)) rotation = newRot;
        else if (currentBlockIndex != 1) ApplyKicks(newRot, rotation * 2);
        
        DrawActiveBlock();
    }

    private void RotateCCW()
    {
        ClearActiveBlock();
        int newRot = (rotation + 3) % 4;
        
        if (CheckCollision(position.x, position.y, newRot)) rotation = newRot;
        else if (currentBlockIndex != 1) ApplyKicks(newRot, (rotation * 2 + 7) % 8);
        
        DrawActiveBlock();
    }

    private void ApplyKicks(int newRot, int kickIndex)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2Int kick = (currentBlockIndex == 0) ? BlockGameData.IKicks[kickIndex, i] : BlockGameData.Kicks[kickIndex, i];
            if (CheckCollision(position.x + kick.x, position.y + kick.y, newRot))
            {
                position.x += kick.x;
                position.y += kick.y;
                rotation = newRot;
                return;
            }
        }
    }

    private void HoldBlock()
    {
        if (held) return;
        
        ClearActiveBlock();
        int previousHold = hold;
        hold = currentBlockIndex;
        
        DrawHoldBlock();

        if (previousHold == -1) SpawnBlock();
        else InitializeBlock(previousHold);
        
        held = true;
    }

    private void RefillQueue()
    {
        List<int> bag = new List<int> {0, 1, 2, 3, 4, 5, 6};
        while (bag.Count > 0)
        {
            int index = Random.Range(0, bag.Count);
            queue.Add(bag[index]);
            bag.RemoveAt(index);
        }
    }

    private int GetNextBlock()
    {
        if (queue.Count < 6) RefillQueue();
        int block = queue[0];
        queue.RemoveAt(0);
        return block;
    }

    private Tile GetTileForBlock(int blockIndex)
    {
        foreach (var data in blockDataList)
        {
            if ((int)data.block == blockIndex) return data.tile;
        }
        return null;
    }

    private void ClearActiveBlock()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2Int offset = BlockGameData.Config[currentBlockIndex, rotation, i];
            tilemap.SetTile(LogicalToVisual(position.x + offset.x, position.y + offset.y), null);
        }
    }

    private void DrawActiveBlock()
    {
        Tile tile = GetTileForBlock(currentBlockIndex);
        for (int i = 0; i < 4; i++)
        {
            Vector2Int offset = BlockGameData.Config[currentBlockIndex, rotation, i];
            tilemap.SetTile(LogicalToVisual(position.x + offset.x, position.y + offset.y), tile);
        }
    }

    private void DrawHoldBlock()
    {
        for (int x = -2; x <= 3; x++)
            for (int y = -2; y <= 3; y++)
                tilemap.SetTile(new Vector3Int(holdVisualOffset.x + x, holdVisualOffset.y + y, 0), null);

        if (hold == -1) return;

        Tile tile = GetTileForBlock(hold);
        for (int i = 0; i < 4; i++)
        {
            Vector2Int offset = BlockGameData.Config[hold, 0, i]; // Rotation 0
            tilemap.SetTile(new Vector3Int(holdVisualOffset.x + offset.x, holdVisualOffset.y + offset.y, 0), tile);
        }
    }

    private void DrawQueue()
    {
        for (int x = -2; x <= 3; x++)
            for (int y = -20; y <= 3; y++)
                tilemap.SetTile(new Vector3Int(queueVisualOffset.x + x, queueVisualOffset.y + y, 0), null);

        for (int q = 0; q < 5; q++)
        {
            if (q >= queue.Count) break;
            
            int block = queue[q];
            Tile tile = GetTileForBlock(block);
            
            Vector2Int blockOrigin = new Vector2Int(queueVisualOffset.x, queueVisualOffset.y - (q * 3));

            for (int i = 0; i < 4; i++)
            {
                Vector2Int offset = BlockGameData.Config[block, 0, i];
                tilemap.SetTile(new Vector3Int(blockOrigin.x + offset.x, blockOrigin.y + offset.y, 0), tile);
            }
        }
    }

    private Vector3Int LogicalToVisual(int logicX, int logicY)
    {
        return new Vector3Int(logicX + visualOffset.x, logicY + visualOffset.y, 0);
    }
}