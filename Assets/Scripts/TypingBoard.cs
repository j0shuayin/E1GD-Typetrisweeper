using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using TMPro;

public class TypingBoard : MonoBehaviour
{
    public Tilemap tilemap;

    // game assets
    [Header("Typing Tiles")]
    public Tile bgTile;
    public Tile cursorTile;
    public Tile errorTile;
    public Tile correctTile;

    [Header("Text Settings")]
    public TMP_FontAsset fontAsset;
    
    private const int Width = 10;
    private const int Height = 20;
    private readonly Vector2Int visualOffset = new Vector2Int(-5, -10);

    private TextMeshPro[,] textGrid;

    // state variables
    private string text = "To make power, people try to put pieces of this metal close enough together that they make heat fast, but not so close that they go out of control and blow up. This is very hard, but there is so much heat and power stored in this metal that some people have wanted to try anyway.";
    
    private int currentIndex = 0;
    private bool hasError = false;
    private char lastWrongChar = '\0';
    private bool gameOver = false;

    void Start()
    {
        InitializeText();
        DrawBoard();
    }

    void OnEnable()
    {
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    private void InitializeText()
    {
        textGrid = new TextMeshPro[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject charObj = new GameObject($"Char_{x}_{y}");
                charObj.transform.SetParent(transform);

                TextMeshPro textMesh = charObj.AddComponent<TextMeshPro>();
                if (fontAsset != null) textMesh.font = fontAsset;
                
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.color = Color.white;
                textMesh.fontSize = 5f;
                textMesh.sortingOrder = 10; 

                RectTransform rt = charObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(1, 1);

                Vector3Int visualPos = LogicalToVisual(x, y);
                charObj.transform.position = tilemap.GetCellCenterWorld(visualPos);

                textGrid[x, y] = textMesh;
            }
        }
    }

    private void OnTextInput(char inputChar)
    {
        if (gameOver) return;

        if (char.IsControl(inputChar)) return;

        if (inputChar == text[currentIndex])
        {
            currentIndex++;
            hasError = false;
            lastWrongChar = '\0';

            if (currentIndex >= text.Length)
            {
                gameOver = true;
                Debug.Log("text completed");
            }
        }
        else 
        {
            hasError = true;
            lastWrongChar = inputChar;
        }

        DrawBoard();
    }

    private void DrawBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                textGrid[x, y].text = "";
                tilemap.SetTile(LogicalToVisual(x, y), bgTile);
            }
        }

        // keep cursor on the second row
        int activeLine = Mathf.Max(0, (currentIndex / Width) - 1);

        for (int screenY = 0; screenY < Height; screenY++)
        {
            int textLine = activeLine + screenY;
            int rowY = (Height - 1) - screenY;

            for (int x = 0; x < Width; x++)
            {
                int charIndex = (textLine * Width) + x;
                if (charIndex < text.Length)
                {
                    textGrid[x, rowY].text = text[charIndex].ToString();
                    if (charIndex < currentIndex)
                    {
                        // Typed correct
                        tilemap.SetTile(LogicalToVisual(x, rowY), correctTile);
                    }
                    else if (charIndex == currentIndex) // cursor tile
                    {
                        tilemap.SetTile(LogicalToVisual(x, rowY), (hasError) ? errorTile : cursorTile);
                    }
                    else
                    {
                        tilemap.SetTile(LogicalToVisual(x, rowY), bgTile);
                    }
                }
            }
        }
    }

    private Vector3Int LogicalToVisual(int logicX, int logicY)
    {
        return new Vector3Int(logicX + visualOffset.x, logicY + visualOffset.y, 0);
    }
}