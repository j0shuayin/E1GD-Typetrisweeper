using UnityEngine;

public static class BlockGameData
{
    public static readonly Vector2Int[,,] Config = new Vector2Int[7, 4, 4]
    {
        // I
        { {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)},
          {new Vector2Int(1,1), new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(1,-2)},
          {new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1,-1), new Vector2Int(2,-1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2)} },
        // O
        { {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(0,1)},
          {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(0,1)},
          {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(0,1)},
          {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,0), new Vector2Int(0,1)} },
        // T
        { {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,0)},
          {new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(1,0)},
          {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,0)},
          {new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1)} },
        // L
        { {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,-1)},
          {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,-1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(-1,1)} },
        // J
        { {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,1)},
          {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(-1,-1)} },
        // S
        { {new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,1)},
          {new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1)},
          {new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(0,0), new Vector2Int(1,0)},
          {new Vector2Int(-1,1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,-1)} },
        // Z
        { {new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0)},
          {new Vector2Int(1,1), new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(0,-1)},
          {new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,-1)},
          {new Vector2Int(-1,-1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1)} }
    };

    public static readonly Vector2Int[,] IKicks = new Vector2Int[8, 4] {
        {new Vector2Int(-2,0), new Vector2Int(1,0), new Vector2Int(-2,-1), new Vector2Int(1,2)},
        {new Vector2Int(2,0), new Vector2Int(-1,0), new Vector2Int(2,1), new Vector2Int(-1,-2)},
        {new Vector2Int(-1,0), new Vector2Int(2,0), new Vector2Int(-1,2), new Vector2Int(2,-1)},
        {new Vector2Int(1,0), new Vector2Int(-2,0), new Vector2Int(1,-2), new Vector2Int(-2,1)},
        {new Vector2Int(2,0), new Vector2Int(-1,0), new Vector2Int(2,1), new Vector2Int(-1,-2)},
        {new Vector2Int(-2,0), new Vector2Int(1,0), new Vector2Int(-2,-1), new Vector2Int(1,2)},
        {new Vector2Int(1,0), new Vector2Int(-2,0), new Vector2Int(1,-2), new Vector2Int(-2,1)},
        {new Vector2Int(-1,0), new Vector2Int(2,0), new Vector2Int(-1,2), new Vector2Int(2,-1)}
    };

    public static readonly Vector2Int[,] Kicks = new Vector2Int[8, 4] {
        {new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,-2), new Vector2Int(-1,-2)},
        {new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,2), new Vector2Int(1,2)},
        {new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,2), new Vector2Int(1,2)},
        {new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,-2), new Vector2Int(-1,-2)},
        {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,-2), new Vector2Int(1,-2)},
        {new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0,2), new Vector2Int(-1,2)},
        {new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0,2), new Vector2Int(-1,2)},
        {new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,-2), new Vector2Int(1,-2)}
    };
}