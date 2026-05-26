using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using TMPro;
using System.Text;
using System.Collections.Generic; 

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
    
    [Header("Endless Mode Settings")]
    [Tooltip("A space-separated list of words to generate the test from.")]
    [TextArea(3, 5)]
    public string wordList = "the be to of and in that have it for not on with he as you do at this but his by from they we say her she or an will my one all would there their what so up out if about who get which go me when make can like time no just him know take people into year your good some could them see other than then now look only come its over think also back after use two how our work first well way even new want any these give day most us number before behalf anyone belief corner";
    
    [Header("UI")]
    public TextMeshProUGUI wpmText;

    private const int Width = 10;
    private const int Height = 20;
    private readonly Vector2Int visualOffset = new Vector2Int(-5, -10);

    private TextMeshPro[,] textGrid;

    // state variables
    private StringBuilder textBuilder = new StringBuilder();
    private Dictionary<int, List<string>> wordsByLength;
    private List<int[]> validLineTemplates; 
    
    private int currentIndex = 0;
    private bool hasError = false;
    private char lastWrongChar = '\0';
    
    // Timer state for WPM
    private bool hasStarted = false;
    private float startTime;

    void Start()
    {
        InitializeDictionary();
        InitializeText();
        GenerateText(200);
        DrawBoard();
    }

    void Update()
    {
        // WPM calculation
        if (hasStarted && wpmText != null)
        {
            float minutesPassed = (Time.time - startTime) / 60f;
            
            if (minutesPassed > 0)
            {
                // WPM Formula: (characters / 5) / minutes
                float wpm = (currentIndex / 5f) / minutesPassed;
                wpmText.text = $"WPM: {Mathf.RoundToInt(wpm)}";
            }
        }
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

    private void InitializeDictionary()
    {
        wordsByLength = new Dictionary<int, List<string>>();
        string[] rawWords = wordList.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        // Group words by their length
        foreach (string w in rawWords)
        {
            if (!wordsByLength.ContainsKey(w.Length))
            {
                wordsByLength[w.Length] = new List<string>();
            }
            wordsByLength[w.Length].Add(w);
        }

        validLineTemplates = new List<int[]>();
        foreach (int l1 in wordsByLength.Keys)
        {
            int l2 = 8 - l1;
            if (wordsByLength.ContainsKey(l2))
            {
                validLineTemplates.Add(new int[] { l1, l2 });
            }
        }

        if (validLineTemplates.Count == 0)
        {
            Debug.LogError("Word list has no valid length combinations to perfectly fill a 10-character line with 1-2 words!");
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
                textMesh.fontSize = 6f;
                textMesh.sortingOrder = 10; 

                RectTransform rt = charObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(1, 1);

                Vector3Int visualPos = LogicalToVisual(x, y);
                charObj.transform.position = tilemap.GetCellCenterWorld(visualPos);

                textGrid[x, y] = textMesh;
            }
        }
    }

    // Generate random text
    private void GenerateText(int minCharsToAdd)
    {
        if (validLineTemplates == null || validLineTemplates.Count == 0) return;

        int added = 0;
        
        while (added < minCharsToAdd)
        {
            // Pick a valid line template at random (e.g., [4, 4] or [5, 3] or [6, 2])
            int[] template = validLineTemplates[Random.Range(0, validLineTemplates.Count)];
            
            foreach (int len in template)
            {
                List<string> options = wordsByLength[len];
                string randomWord = options[Random.Range(0, options.Count)];
                
                textBuilder.Append(randomWord).Append(" ");
            }
            
            added += 10; // Every template perfectly equals exactly 10 characters
        }
    }

    private void OnTextInput(char inputChar)
    {
        // Ignore control characters (like enter, backspace)
        if (char.IsControl(inputChar)) return;

        // Start timer on the first typed character
        if (!hasStarted)
        {
            hasStarted = true;
            startTime = Time.time;
        }

        // Check if correct
        if (inputChar == textBuilder[currentIndex])
        {
            currentIndex++;
            hasError = false;
            lastWrongChar = '\0';

            // Generate more text if needed
            if (textBuilder.Length - currentIndex < 200)
            {
                GenerateText(100);
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
                
                if (charIndex < textBuilder.Length)
                {
                    textGrid[x, rowY].text = textBuilder[charIndex].ToString();
                    
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
                        // Untyped text background
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