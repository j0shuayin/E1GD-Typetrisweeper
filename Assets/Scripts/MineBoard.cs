using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MineBoard : MonoBehaviour
{
    
    // Class variables
    public Tilemap tilemap;

    [Header("Tiles")]
    public Tile hiddenTile;
    public Tile flagTile;
    public Tile falseFlagTile;
    public Tile mineTile;
    public Tile explodedTile;
    public Tile[] numberTiles;

    [Header("Progress Bar UI")]
    public Image progressBarFill;
    public TextMeshProUGUI scoreText;
    public Color barColor = new Color(0.3f, 0.8f, 0.3f);
    public Color overfillColor = new Color(1f, 0.8f, 0f);

    // Score value tables
    [Header("Scoring Parameters")]
    public int currentScore = 1000;
    public int maxScoreCapacity = 2000;

    public int pointsReveal1 = 1;
    public int pointsReveal2 = 2;
    public int pointsReveal3 = 5;
    public int pointsReveal4 = 10;
    public int pointsReveal5 = 20;
    public int pointsReveal6to8 = 50;
    
    public int pointsWin = 100;
    public int pointsLoss = -60;
    public int pointsReset = -30;

    private const int Width = 10;
    private const int Height = 20;
    private const int numMines = 36;
    private readonly Vector2Int visualOffset = new Vector2Int(-5, -10);

    private struct Cell
    {
        public bool isMine;
        public bool isRevealed;
        public bool isFlagged;
        public int adjacentMines;
    }

    private Cell[,] grid;
    private bool gameOver;
    private bool firstClick;
    private bool noScoring;

    void Start()
    {
        UpdateScoreUI();
        InitializeGame();
    }

    void Update()
    {
        
        // check if user has restarted the game
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
        {
            if (!gameOver) Score(pointsReset);
            InitializeGame();
        }

        if (gameOver) return;

        // handle mouse clicks
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            HandleClick(true);
        }
        else if (mouse.rightButton.wasPressedThisFrame)
        {
            HandleClick(false);
        }
    }

    private void InitializeGame()
    {
        grid = new Cell[Width, Height];
        gameOver = false;
        firstClick = true;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = new Cell();
                DrawTile(x, y);
            }
        }
    }

    // Scoring
    private void Score(int amount)
    {
        currentScore += amount;
        if (currentScore < 0) currentScore = 0;
        
        UpdateScoreUI();
    }

    // Progress bar display logic
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore} / {maxScoreCapacity}";
        }

        if (progressBarFill != null)
        {
            float fillRatio = (float)currentScore / maxScoreCapacity;
            
            if (fillRatio < 0) fillRatio = 0;
            
            progressBarFill.rectTransform.localScale = new Vector3(fillRatio, 1f, 1f);

            if (currentScore > maxScoreCapacity)
            {
                progressBarFill.color = overfillColor;
                if (scoreText != null) scoreText.color = overfillColor;
            }
            else
            {
                progressBarFill.color = barColor;
                if (scoreText != null) scoreText.color = Color.white;
            }
        }
    }

    // Game Logic
    private void HandleClick(bool isLeftClick)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z));
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        
        int logicX = cellPos.x - visualOffset.x;
        int logicY = cellPos.y - visualOffset.y;

        if (logicX < 0 || logicX >= Width || logicY < 0 || logicY >= Height) return;

        if (isLeftClick)
        {
            if (grid[logicX, logicY].isFlagged) return;

            // generate mines on first click
            if (firstClick)
            {
                GenerateMines(logicX, logicY);
                firstClick = false;
                noScoring = true;
            }

            if (grid[logicX, logicY].isRevealed) TryChord(logicX, logicY);
            else Reveal(logicX, logicY);
            
            noScoring = false;
            
            CheckWinCondition();
        }
        else 
        {
            if (!grid[logicX, logicY].isRevealed)
            {
                grid[logicX, logicY].isFlagged = !grid[logicX, logicY].isFlagged;
                DrawTile(logicX, logicY);
            }
        }
    }

    private void GenerateMines(int startX, int startY)
    {
        int minesPlaced = 0;
        while (minesPlaced < numMines)
        {
            int rx = Random.Range(0, Width);
            int ry = Random.Range(0, Height);

            // ensure that the first click creates an opening
            if (Mathf.Abs(rx - startX) <= 1 && Mathf.Abs(ry - startY) <= 1) continue;

            if (!grid[rx, ry].isMine)
            {
                grid[rx, ry].isMine = true;
                minesPlaced++;
            }
        }

        // precompute safe tile values
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!grid[x, y].isMine)
                {
                    grid[x, y].adjacentMines = CountAdjacentMines(x, y);
                }
            }
        }
    }

    private void Reveal(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        if (grid[x, y].isRevealed || grid[x, y].isFlagged) return;

        grid[x, y].isRevealed = true;
        DrawTile(x, y);

        // check if it's a mine
        if (grid[x, y].isMine)
        {
            Explode(x, y);
            return;
        }

        // update score with value of revealed tile
        int adj = grid[x, y].adjacentMines;
        if (!noScoring) {
            if (adj == 1) Score(pointsReveal1);
            else if (adj == 2) Score(pointsReveal2);
            else if (adj == 3) Score(pointsReveal3);
            else if (adj == 4) Score(pointsReveal4);
            else if (adj == 5) Score(pointsReveal5);
            else if (adj >= 6 && adj <= 8) Score(pointsReveal6to8);
        }

        if (adj == 0)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Reveal(x + dx, y + dy);
                }
            }
        }
    }

    private void TryChord(int x, int y)
    {
        int adjacentFlags = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    if (grid[nx, ny].isFlagged) adjacentFlags++;
                }
            }
        }

        if (adjacentFlags == grid[x, y].adjacentMines)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                    {
                        if (!grid[nx, ny].isRevealed && !grid[nx, ny].isFlagged)
                        {
                            Reveal(nx, ny);
                        }
                    }
                }
            }
        }
    }

    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                    if (grid[nx, ny].isMine) count++;
            }
        }
        return count;
    }

    // Exploded mine logic
    private void Explode(int explodedX, int explodedY)
    {
        gameOver = true;
        Score(pointsLoss);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3Int visualPos = new Vector3Int(x + visualOffset.x, y + visualOffset.y, 0);

                if (grid[x, y].isFlagged && !grid[x, y].isMine) tilemap.SetTile(visualPos, falseFlagTile);
                else if (grid[x, y].isMine && !grid[x, y].isFlagged)
                {
                    grid[x, y].isRevealed = true;
                    DrawTile(x, y);
                }
            }
        }

        Vector3Int deathPos = new Vector3Int(explodedX + visualOffset.x, explodedY + visualOffset.y, 0);
        tilemap.SetTile(deathPos, explodedTile);
    }

    // Check win state
    private void CheckWinCondition()
    {
        if (gameOver) return;

        int revealedCount = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y].isRevealed && !grid[x, y].isMine) revealedCount++;
            }
        }

        if (revealedCount == (Width * Height) - numMines)
        {
            gameOver = true;
            Score(pointsWin);
        }
    }

    // Draw logic for individual tiles
    private void DrawTile(int x, int y)
    {
        Vector3Int visualPos = new Vector3Int(x + visualOffset.x, y + visualOffset.y, 0);
        Tile tile;

        if (grid[x, y].isRevealed)
        {
            if (grid[x, y].isMine) tile = mineTile;
            else tile = numberTiles[grid[x, y].adjacentMines];
        }
        else
        {
            if (grid[x, y].isFlagged) tile = flagTile;
            else tile = hiddenTile;
        }

        tilemap.SetTile(visualPos, tile);
    }
}