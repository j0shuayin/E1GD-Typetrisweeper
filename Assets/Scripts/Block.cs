using UnityEngine;
using UnityEngine.Tilemaps;

public enum Block {
    I = 0, O = 1, T = 2, L = 3, J = 4, S = 5, Z = 6,
}

[System.Serializable] 
public struct BlockData {
    public Block block;
    public Tile tile;
}