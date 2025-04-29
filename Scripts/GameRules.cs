using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Linq;

public struct Match
{
    public Animals       animal;
    public HexCoordinate coord;
};

public struct PlaceTileResult
{
    public Match match;
    public int   level;
};

public class AnimalCard
{
    public Animals animal;
    public int     matchedCount;
};

public class GameRules
{
    public uint[]           tilesNumber { get; private set; }
    public List<Animals>    animalsToDraw { get; private set; }
    public HexaGrid         grid { get; private set; }
    public CentralGameboard tokenBoard { get; private set; }
    public Phase            phase { get; private set; }

    private List<AnimalCard> playerCards;
    private RandomGenerator  random;
    private uint             currentlySelectedTokenBoard;
    private CardData[]       cardsToSpawn;

    public GameRules(Resources[] tiles, CardData[] cardsToSpawn)
    {
        grid = new HexaGrid();
        playerCards = new List<AnimalCard>();        
        random = new RandomGenerator(4102);

        int enumCount = Enum.GetValues(typeof(TileType)).Length;
        tilesNumber = new uint[enumCount];

        foreach(Resources r in tiles)
        {
            tilesNumber[(int)r.type] += r.number;
        }

        tokenBoard = new CentralGameboard(HardRules.choiceOptions, random, HardRules.tokenNumberPerSlot, tilesNumber);

        Animals[] enumArray = (Animals[])System.Enum.GetValues(typeof(Animals));
        animalsToDraw = Enum.GetValues(typeof(Animals))
                            .Cast<Animals>()
                            .Skip(1) 
                            .ToList();
        shuffle(animalsToDraw, random);
        this.cardsToSpawn = cardsToSpawn;
    }

    public bool selectTokenBoard(uint nb)
    {
        bool result = false;
        if((phase == Phase.SelectTokens) && nb >= 0 && nb < tokenBoard.getTokenChoices())
        {
            currentlySelectedTokenBoard = nb;
            phase = Phase.PlaceTokens;
            result = true;
        }

        return result;
    }

    public bool canPlayThisType(TileType type)
    {
        bool result = (phase == Phase.PlaceTokens) && tokenBoard.cellContains(type, currentlySelectedTokenBoard);
        return result;
    }

    public bool consume(TileType type)
    {
        bool empty = false;
        if(phase == Phase.PlaceTokens)
        {
            tokenBoard.consume(type, currentlySelectedTokenBoard);
            empty = tokenBoard.getCellContent(currentlySelectedTokenBoard).Count == 0;

            if(empty)
            {
                phase = Phase.SelectTokens;
                tokenBoard.refill(currentlySelectedTokenBoard, tilesNumber);
                currentlySelectedTokenBoard = 0;
            }
        }

        return empty;
    }

    public TileType nextTile()
    {
        TileType result = TileType.None;
        if(phase == Phase.PlaceTokens)
        {
            result = tokenBoard.nextTile(currentlySelectedTokenBoard);
        }
        return result;
    }

    public void clearBoard()
    {
        grid.clear();
    }

    public int getCubesOnCards(Animals animal)
    {
        for(int i =0; i < playerCards.Count; ++i)
        {
            if(playerCards[i].animal == animal)
            {
                return playerCards[i].matchedCount;
            }
        }

        return 0;
    }

    public PlaceTileResult placeTile(HexCoordinate coord, TileType type)
    {
        int level = grid.placeTile(coord, type);
        Match match = bruteForceMatches();

        if(match.animal != Animals.None)
        {
            grid.placeAnimal(match.coord, match.animal);
            addAnimalMatch(playerCards, match.animal);
        }

        return new PlaceTileResult { match = match, level = level};
    }

    public bool canPlaceTile(HexCoordinate coord, TileType type)
    {
        return grid.canPlaceTile(coord, type);
    }

    public uint tileRemaining(TileType type)
    {
        return tilesNumber[(int)type];
    }

    public Animals drawOneCardAnimal()
    {
        Animals result = Animals.None;
        if(animalsToDraw.Count > 0)
        {
            result = popBack(animalsToDraw);
            playerCards.Add(new AnimalCard{ animal = result, matchedCount = 0});
        }

        return result;
    }

    private static T popBack<T>(List<T> list)
    {
        T value = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return value;
    }

    private static void shuffle<T>(List<T> array, RandomGenerator rng)
    {
        for (int i = array.Count - 1; i > 0; i--)
        {
            int j = rng.getRandomValueInclusive(0, i);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private Match bruteForceMatches()
    {
        foreach(CardData card in getPlayerShapes(playerCards))
        {
            foreach(var (key, rotation) in grid.getMatchingPositions(card.shape.shape))
            {
                return new Match{ animal = card.animal, coord = key };
            }
        }

        return new Match{ animal = Animals.None};
    }

    private IEnumerable<CardData> getPlayerShapes(List<AnimalCard> playerCards)
    {
        if(cardsToSpawn != null)
        {
            foreach(AnimalCard animal in playerCards)
            {
                foreach(CardData card in cardsToSpawn)
                {
                    if(card.animal == animal.animal)
                    {
                        yield return card;
                    }
                }
            }
        }
    }

    private void addAnimalMatch(List<AnimalCard> playerCards, Animals animal)
    {
        for(int i =0; i < playerCards.Count; ++i)
        {
            if(playerCards[i].animal == animal)
            {
                playerCards[i].matchedCount += 1;
                break;
            }
        }
    }
};