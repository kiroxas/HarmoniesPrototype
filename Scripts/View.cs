using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ViewUtils
{
    static public Color getColor(TileType type)
    {
        switch(type)
        {
            case TileType.Gray : return Color.gray;
            case TileType.Red  : return Color.red;
            case TileType.Brown  : return new Color(0.6f, 0.3f, 0.1f);
            case TileType.Green  : return Color.green;
            case TileType.Blue  : return Color.blue;
            case TileType.Yellow  : return Color.yellow;
        }

        return Color.white;
    }
   
};


public class View : MonoBehaviour
{
    public HexOptions options;
    public GameObject hexPrefab;
    public GameObject tokenPrefab;
    public GameObject cardPrefab;
    public CardData[] cardsToSpawn;
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;
    public Color matchColor = Color.green;
    public Color cancelColor = Color.red;
    public TokenRessources resources;

    private GameObject highlighted = null;
    private Dictionary<HexCoordinate, GameObject> hexGrid;
    private GameRules rules;
    private GameObject tokenBoard = null;
    private GameObject currentHighlightedBoard = null;
    private GameObject[] highlightTokenBoard;
    private string boardToken = "BoardToken";
    private string boardTokenArea = "BoardTokenArea";
    private Dictionary<uint, List<GameObject>> boardTokens;
    private Dictionary<Animals, GameObject> animalCards;
    private uint selectedBoardIndex;
    private float tokenBoardXStep = 1.5f;

    void Awake()
    {
        hexGrid = new Dictionary<HexCoordinate, GameObject>();
        rules   = new GameRules(resources.resources, cardsToSpawn);
        highlightTokenBoard = new GameObject[HardRules.choiceOptions];
        boardTokens = new Dictionary<uint, List<GameObject>>();
        for(uint i = 0; i < HardRules.choiceOptions; ++i)
        {
            boardTokens[i] = new List<GameObject>();
        }

        animalCards = new Dictionary<Animals, GameObject>();
    }

    void Start()
    {
        options = ScriptableObject.CreateInstance<HexOptions>();
        layoutHexGrid();
        layoutTokens();
        layoutTokenBoard(new Vector3(8.5f,0,0));

        instantiateCard(rules.drawOneCardAnimal(), new Vector3(0,1,-2));
        instantiateCard(rules.drawOneCardAnimal(), new Vector3(2,1,-2));
        instantiateCard(rules.drawOneCardAnimal(), new Vector3(4,1,-2));
    }

    void Update()
    {    
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Phase phase = rules.phase;
            GameObject hoveredTile = hit.collider.gameObject;

            if(hoveredTile.CompareTag("HexTile") && phase == Phase.PlaceTokens)
            {
                HexCoordinate coord = hoveredTile.GetComponent<HexRenderer>().coordinate;
                bool canPlace = rules.canPlaceTile(coord, rules.nextTile());

                // @Note Highlight logic
                if(hoveredTile != highlighted)
                {
                    hoveredTile.GetComponent<Renderer>().material.color = canPlace ? highlightColor : cancelColor;
                    if(highlighted != null)
                    {
                        highlighted.GetComponent<Renderer>().material.color = normalColor;
                    }
                    highlighted = hoveredTile;
                }

                // @Note Click
                if (canPlace && Input.GetMouseButtonDown(0)) 
                {
                    TileType type = rules.nextTile();
                    PlaceTileResult placement = rules.placeTile(coord, type);
                    int level = placement.level;
                    bool isEmpty = rules.consume(type);

                    spawnTokenOnBoard(coord, type, level);

                    if(placement.match.animal != Animals.None)
                    {
                        spawnAnimalOnBoard(placement.match.coord, placement.match.animal, level + 1);
                        spawnCubeOnAnimalCard(coord, placement.match.animal, rules.getCubesOnCards(placement.match.animal));
                    }

                    // @Improve Not very robust as the logic may want to remove them in different order. Maybe at least remove it by color ?
                    if(boardTokens[selectedBoardIndex].Count > 0)
                    {
                        Destroy(boardTokens[selectedBoardIndex][0]);
                        boardTokens[selectedBoardIndex].RemoveAt(0);
                    }

                    if(isEmpty)
                    {
                        if(currentHighlightedBoard)
                        {
                            currentHighlightedBoard.GetComponent<Renderer>().material.color = normalColor;
                            currentHighlightedBoard.SetActive(false);
                            currentHighlightedBoard = null;
                        }

                        boardTokens[selectedBoardIndex].Clear();

                        if(highlighted != null)
                        {
                            highlighted.GetComponent<Renderer>().material.color = normalColor;
                        }
                        highlighted = null;

                        instantiateTokenChoices(rules.tokenBoard.choices[selectedBoardIndex], selectedBoardIndex);
                    }
                }
            }
            else if(phase == Phase.SelectTokens)
            {
                bool clicked = false;

                if(hoveredTile.CompareTag(boardToken))
                {
                    if(currentHighlightedBoard)
                    {
                        currentHighlightedBoard.SetActive(false);
                    }

                    Token tok = hoveredTile.GetComponent<Token>();
                    selectedBoardIndex = tok.boardIndex;

                    if(selectedBoardIndex < highlightTokenBoard.Length)
                    {
                        currentHighlightedBoard = highlightTokenBoard[selectedBoardIndex];
                        currentHighlightedBoard.SetActive(true);

                        // @Note Click
                        if (Input.GetMouseButtonDown(0)) 
                        {
                            clicked = true;
                        }
                    }
                }
                else if(hoveredTile.CompareTag(boardTokenArea) && Input.GetMouseButtonDown(0))
                {
                    clicked = true;
                }

                if(clicked)
                {
                    rules.selectTokenBoard(selectedBoardIndex);

                    if(currentHighlightedBoard)
                    {
                        currentHighlightedBoard.GetComponent<Renderer>().material.color = matchColor;
                    }
                }
                
            }
        }
    }

    private void instantiateCard(Animals animal, Vector3 position)
    {
        foreach(CardData c in cardsToSpawn)
        {
            if(c.animal == animal)
            {
                GameObject card = Instantiate(cardPrefab, position, Quaternion.Euler(90f, 0f, 0f));
                CardMesh mesh = card.GetComponent<CardMesh>();

                mesh.createCard(c);
                animalCards[animal] = card;
            }
        }
    }

    private Vector3 getTokenBoardPosition(uint index)
    {
        return  new Vector3(0,0, index * 2.5f);
    }

    private void instantiateTokenChoices(ResourcesChoice c, uint index)
    {
        Vector3 localPosition = getTokenBoardPosition(index);
        foreach(TileType type in c.tiles)
        {
            GameObject token = Instantiate(tokenPrefab, Vector3.zero, Quaternion.identity);
            token.tag = boardToken;

            Token tok = token.GetComponent<Token>();
            token.transform.SetParent(tokenBoard.transform);
            tok.setColor(ViewUtils.getColor(type));
            tok.transform.localPosition = localPosition;
            tok.boardIndex = index;

            boardTokens[index].Add(token);

            localPosition.x += tokenBoardXStep;
        }
    }

    private void layoutTokenBoard(Vector3 position)
    {
        tokenBoard = new GameObject("tokenBoard");
        tokenBoard.transform.position += position;

        uint index = 0;
        
        foreach(ResourcesChoice c in rules.tokenBoard.choices)
        {
            instantiateTokenChoices(c, index);

            if(index < highlightTokenBoard.Length)
            {
                highlightTokenBoard[index] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                highlightTokenBoard[index].transform.SetParent(tokenBoard.transform);
                highlightTokenBoard[index].transform.localPosition = getTokenBoardPosition(index) + new Vector3(tokenBoardXStep, -0.1f, 0);
                highlightTokenBoard[index].transform.localScale    = new Vector3(5, 2, 10);
                highlightTokenBoard[index].transform.eulerAngles   = new Vector3(90, 0, 0);
                highlightTokenBoard[index].SetActive(false);
                highlightTokenBoard[index].tag = boardTokenArea;
            }

            ++index;
        }

        
    }
    
    private void layoutHexGrid()
    {
        foreach (var (key, value) in rules.grid.iterate())
        {
            Vector3 pos = HexUtils.CubeToWorld_FlatTop(key, options.outerSize + options.margin);
            Vector3 center = pos + transform.position;
            GameObject tile = Instantiate(hexPrefab, center, Quaternion.identity);

            HexRenderer renderer =  tile.GetComponent<HexRenderer>();
            renderer.setCoordinate(key);

            hexGrid.Add(key, tile);
        }
    }

    private void spawnAnimalOnBoard(HexCoordinate c, Animals animal, int level)
    {
        if(animal != Animals.None)
        {
            Vector3 pos = HexUtils.CubeToWorld_FlatTop(c, options.outerSize + options.margin);
            Vector3 center = pos + transform.position;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = center + Vector3.up * VisualValues.tokenHeight * (level + 1);

            float scale = 0.45f;
            cube.transform.localScale = new Vector3(scale, scale, scale);
            cube.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    private void spawnCubeOnAnimalCard(HexCoordinate c, Animals animal, int level)
    {
        if(animal != Animals.None)
        {
            GameObject parentCard;
            if(animalCards.TryGetValue(animal, out parentCard))
            {
                float scale = 0.45f;
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = parentCard.transform.position + Vector3.up * scale * (level - 1);
                
                cube.transform.localScale = new Vector3(scale, scale, scale);
                cube.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
    }

    // @Debug use to verify that shape definitions are correct 
    private void spawnShapeOn(HexCoordinate c, HexShape shape)
    {
        foreach(HexShapePiece piece in shape.pieces)
        {
            HexCoordinate coord = c + piece.offset;

            int level = 0;
            foreach(TileType type in piece.levels)
            {
                spawnTokenOnBoard(coord, type, level);
                ++level;
            }
        }
    }

    private void spawnTokenOnBoard(HexCoordinate c, TileType type, int level)
    {
        if(type != TileType.None)
        {
            Vector3 pos = HexUtils.CubeToWorld_FlatTop(c, options.outerSize + options.margin);
            Vector3 center = pos + transform.position;

            GameObject token = Instantiate(tokenPrefab, center, Quaternion.identity);
            Token tok = token.GetComponent<Token>();
            MeshCollider collider = tok.GetComponent<MeshCollider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            token.transform.position += Vector3.up * VisualValues.tokenHeight * (level + 1);
            tok.setColor(ViewUtils.getColor(type));
        }
    }

    private void layoutTokens()
    {
        foreach (var (key, value) in rules.grid.iterate())
        {
            int level = 0;
            for(;;)
            {
                TileType type = value.getTileAtLevel(level);
                if(type == TileType.None)
                {
                    break;
                }
                spawnTokenOnBoard(key, type, level);
                ++level;
            }
        }
    }
}
