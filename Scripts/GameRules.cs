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

public enum PlaceTileReturnTypes
{
    Validated,
    InvalidPhase,
    InvalidType,
    InvalidCoordinate,
};

public struct PlaceTileResult
{
    public PlaceTileReturnTypes result;
    public Match match;
    public int   levelOnBoard;
    public bool  cardFinished;
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
    private List<Animals>    validatedCards;
    private RandomGenerator  random;
    private uint             currentlySelectedTokenBoard;
    private CardData[]       cardsToSpawn;

    public GameRules(Resources[] tiles, CardData[] cardsToSpawn)
    {
        grid = new HexaGrid();
        playerCards = new List<AnimalCard>(); 
        validatedCards = new List<Animals>();   
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

        for(int i =0; i < validatedCards.Count; ++i)
        {
            if(validatedCards[i] == animal)
            {
                uint index = getCardData(animal);
                if(index < cardsToSpawn.Length)
                {
                    return cardsToSpawn[index].scores.Length;
                }
            }
        }

        return 0;
    }

    public PlaceTileResult placeTile(HexCoordinate coord, TileType type)
    {
        PlaceTileReturnTypes result = canPlaceTile(coord, type);
        if(result == PlaceTileReturnTypes.Validated)
        {
            if(canPlayThisType(type) == false)
            {
                result = PlaceTileReturnTypes.InvalidType;
            }
            else
            {
                int level = grid.placeTile(coord, type);
                Match match = bruteForceMatches();
                bool cardFinished = false;

                if(match.animal != Animals.None)
                {
                    grid.placeAnimal(match.coord, match.animal);
                    cardFinished = addAnimalMatch(playerCards, match.animal);
                }

                return new PlaceTileResult { result = result, match = match, levelOnBoard = level, cardFinished = cardFinished};
            }
        }

        return new PlaceTileResult { result = result };
    }

    public PlaceTileReturnTypes canPlaceTile(HexCoordinate coord, TileType type)
    {
        if(phase != Phase.PlaceTokens)
        {
            return PlaceTileReturnTypes.InvalidPhase;
        }
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

    private uint getCardData(Animals animal)
    {
        for(uint i = 0; i <  (uint)cardsToSpawn.Length; ++i)
        {
            if(cardsToSpawn[i].animal == animal)
            {
                return i;
            }
        }

        return (uint)cardsToSpawn.Length;
    } 

    // @Note If this returns true then it means the card has been validated
    private bool addAnimalMatch(List<AnimalCard> playerCards, Animals animal)
    {
        bool finishedCard = false;
        for(int i =0; i < playerCards.Count; ++i)
        {
            if(playerCards[i].animal == animal)
            {
                playerCards[i].matchedCount += 1;
                uint index = getCardData(animal);
                if(index < cardsToSpawn.Length)
                {
                    finishedCard = playerCards[i].matchedCount >= cardsToSpawn[index].scores.Length;
                    if(finishedCard)
                    {
                        validatedCards.Add(animal);
                        playerCards.RemoveAt(i);
                    }
                }
                break;
            }
        }

        return finishedCard;
    }
};