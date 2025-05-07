using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine; // @Debug

public class ResourcesChoice
{
    public List<TileType> tiles { get; private set; }

    public void fill(IGameRandom random, uint number, uint[] tilesNumber)
    {
        uint sum = 0;

        foreach (uint v in tilesNumber)
        {
            sum += v; // @Warning very unlikely overflow
        }
        
        uint iterationMax = Math.Min(number, sum);

        for(uint i = 0; i < iterationMax;)
        {
            TileType type = random.getRandomTileType();

            if(tilesNumber[(int)type] > 0)
            {
                --tilesNumber[(int)type];
                tiles.Add(type);
                ++i;
            }
        }
    }

    public void consume(TileType type)
    {
        tiles.Remove(type);
    }

    public void empty()
    {
        tiles.Clear();
    }

    public bool isValid()
    {
        return tiles.Count > 0;
    }

    public int tilesRemaining()
    {
        return tiles.Count;
    }

    public bool contains(TileType type)
    {
        return tiles.Contains(type);
    }

    public TileType next()
    {
        TileType type = TileType.None;
        if(tiles.Count > 0)
        {
            type = tiles[0];
        }

        return type;
    }

    public ResourcesChoice(IGameRandom random, uint number, uint[] tilesNumber)
    {
        tiles = new List<TileType>();
        fill(random, number, tilesNumber);
    }
};

[System.Serializable]
public struct Resources
{
    public TileType type;
    public uint     number;

    public Resources(TileType type, uint number)
    {
        this.type = type;
        this.number = number;
    }
};

public class CentralGameboard
{
    public ResourcesChoice[] choices  { get; private set; }
    private uint numberPerChoice;
    private IGameRandom rand;

    public void fill(int index, IGameRandom random, uint[] tilesNumber)
    {
        if(index >= 0 && index < choices.Length)
        {
            choices[index].fill(random, numberPerChoice, tilesNumber);
        }
    }

    public int getTokenChoices()
    {
        return choices.Length;
    }

    public void consume(TileType type, uint index)
    {
        if(index < choices.Length)
        {
            ResourcesChoice c = choices[index];
            c.consume(type);
        }
    }

    public CentralGameboard(uint choiceOptions, IGameRandom random, uint numberPerChoice, uint[] tilesNumber)
    {
        this.numberPerChoice = numberPerChoice;
        choices = new ResourcesChoice[choiceOptions];
        rand = random;
        for (int i = 0; i < choiceOptions; i++)
        {
            choices[i] = new ResourcesChoice(random, numberPerChoice, tilesNumber);
        }
    }

    public bool contains(TileType type)
    {
        bool contain = false;
        foreach(ResourcesChoice choice in choices)
        {
            if(choice.contains(type))
            {
                contain = true;
                break;
            }
        }

        return contain;
    }

    public bool cellContains(TileType type, uint index)
    {
        bool contains = false;
        if(index < choices.Length)
        {
            ResourcesChoice c = choices[index];
            contains = c.contains(type);
        }

        return contains;
    }

    public TileType nextTile(uint index)
    {
        TileType type = TileType.None;
        if(index < choices.Length)
        {
            ResourcesChoice c = choices[index];
            type = c.next();
        }

        return type;
    }

    public void empty(uint index)
    {
        if(index < choices.Length)
        {
            choices[index].empty();
        }
    }

    public void refill(uint index, uint[] tilesNumber)
    {
        if(index < choices.Length && (choices[index].isValid() == false))
        {
            choices[index].fill(rand, numberPerChoice, tilesNumber);
        }
    }

    #nullable enable
    public IReadOnlyList<TileType>? getCellContent(uint index)
    {
        if(index < choices.Length)
        {
            return choices[index].tiles;
        }

        return null;
    } 
    #nullable disable
};