using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.Assertions;
using UnityEngine;

// http://devmag.org.za/2013/08/31/geometry-with-hex-coordinates/
//@Note https://www.redblobgames.com/grids/hexagons/
[System.Serializable]
public struct HexCoordinate
{
    public int q;
    public int r;
    public int s;

    public HexCoordinate(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public HexCoordinate(int q, int r)
    {
        this.q = q;
        this.r = r;
        this.s = -(q + r);
    }

    public int length()
    {
        return (Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2;
    }

    public static HexCoordinate operator +(HexCoordinate a, HexCoordinate b)
    {
        return new HexCoordinate(a.q + b.q, a.r + b.r, a.s + b.s);
    }

    public static HexCoordinate operator -(HexCoordinate a, HexCoordinate b)
    {
        return new HexCoordinate(a.q - b.q, a.r - b.r, a.s - b.s);
    }

    public static HexCoordinate operator *(HexCoordinate a, HexCoordinate b)
    {
        return new HexCoordinate(a.q * b.q, a.r * b.r, a.s * b.s);
    }

    public static bool operator ==(HexCoordinate a, HexCoordinate b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(HexCoordinate a, HexCoordinate b)
    {
        return (a == b) == false;
    }

    public override bool Equals(object obj)
    {
        if (obj is HexCoordinate other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Simple hash combination
        return q.GetHashCode() ^ (r.GetHashCode() << 2);
    }

    public static int distance(HexCoordinate a, HexCoordinate b)
    {
        HexCoordinate result = a - b;
        return result.length();
    }

    public override string ToString()
    {
        return $"({q}, {r}, {s})";
    }

    public static int inferRotationFromVectors(HexCoordinate expected, HexCoordinate given)
    {
        for (int rotation = 0; rotation < 6; rotation++)
        {
            HexCoordinate rot = rotate60(expected, rotation);
            if (rot == given)
            {
                return rotation;
            }
        }
        return -1;
    }

    public static HexCoordinate rotate60(HexCoordinate c, int rotation)
    {
        rotation = ((rotation % 6) + 6) % 6;
        switch (rotation)
        {
            case 0: return new HexCoordinate(c.q, c.r);
            case 1: return new HexCoordinate(-c.r, c.q + c.r);
            case 2: return new HexCoordinate(-c.q - c.r, c.q);
            case 3: return new HexCoordinate(-c.q, -c.r);
            case 4: return new HexCoordinate(c.r, -c.q - c.r);
            case 5: return new HexCoordinate(c.q + c.r, -c.q);
        }

        return new HexCoordinate(0, 0);
    }
};

[System.Serializable]
public struct HexShapePiece
{
    public HexCoordinate offset;
    public TileType[] levels;

    public HexShapePiece(TileType type)
    {
        levels = new TileType[HardRules.maxElevation];
        levels[0] = type;
        levels[1] = TileType.None;
        levels[2] = TileType.None;
        offset = new HexCoordinate(0,0,0);
    }

    public HexShapePiece(TileType type, TileType t2)
    {
        levels = new TileType[HardRules.maxElevation];
        levels[0] = type;
        levels[1] = t2;
        levels[2] = TileType.None;
        offset = new HexCoordinate(0,0,0);
    }

    public HexShapePiece(TileType type, HexCoordinate offset)
    {
        levels = new TileType[HardRules.maxElevation];
        levels[0] = type;
        levels[1] = TileType.None;
        levels[2] = TileType.None;
        this.offset = offset;
    }

    public HexShapePiece(TileType type, TileType t2, HexCoordinate offset)
    {
        levels = new TileType[HardRules.maxElevation];
        levels[0] = type;
        levels[1] = t2;
        levels[2] = TileType.None;
        this.offset = offset;
    }

    public HexShapePiece(TileType type, TileType t2, TileType t3, HexCoordinate offset)
    {
        levels = new TileType[HardRules.maxElevation];
        levels[0] = type;
        levels[1] = t2;
        levels[2] = t3;
        this.offset = offset;
    }
};

[System.Serializable]
public struct HexShape
{
    public List<HexShapePiece> pieces;

    public HexShape(TileType type)
    {
        pieces = new List<HexShapePiece>();
        pieces.Add(new HexShapePiece(type));
    }

    public HexShape(TileType type, TileType t2)
    {
        pieces = new List<HexShapePiece>();
        pieces.Add(new HexShapePiece(type, t2));
    }

    public void addTile(HexCoordinate offset, TileType type)
    {
        pieces.Add(new HexShapePiece(type, offset));
    }

    public void addTile(HexCoordinate offset, TileType type,TileType t2)
    {
        pieces.Add(new HexShapePiece(type, t2, offset));
    }
};

public class HexaGrid
{
    private Dictionary<HexCoordinate, GridCell> grid;
    static readonly HexCoordinate[] hexDirections = { new HexCoordinate(1,0 ,-1), new HexCoordinate(1,-1,0), new HexCoordinate(0,-1,1), new HexCoordinate(-1,0,1), new HexCoordinate(-1,1,0), new HexCoordinate(0,1,-1) };

    public HexaGrid(uint rows = HardRules.normalRows, uint columns = HardRules.normalColumns)
    {
        grid = new Dictionary<HexCoordinate, GridCell>();

        for (int q = 0; q < (int)rows; q++)
        { 
            int q_offset = (int)Math.Floor(q/2.0f);

            int cols = q % 2 == 0 ? (int)columns : (int)columns - 1;
            for (int r = 0 - q_offset; r < cols - q_offset; r++) 
            {
                HexCoordinate coordinate = new HexCoordinate(q, r, -q-r);
                grid[coordinate] = new GridCell();
            }
        }
    }

    public bool isValidCoordinate(HexCoordinate coordinate)
    {
        return grid.ContainsKey(coordinate);
    }

    public void clear()
    {
        foreach (var (key, value) in grid)
        {
            value.clear();
        }
    }

    public IEnumerable<KeyValuePair<HexCoordinate, GridCell>> iterate()
    {
        return grid;
    }

    public int placeTile(HexCoordinate coordinate, TileType tile)
    {
        int result = -1;
        GridCell cell;
        if(grid.TryGetValue(coordinate, out cell))
        {
            if(cell.canPlace(tile))
            {
                result = cell.addTile(tile);
            }
        }

        return result;
    }

    public bool placeAnimal(HexCoordinate coordinate, Animals animal)
    {
        bool result = false;
        GridCell cell;
        if(grid.TryGetValue(coordinate, out cell))
        {
            result = cell.placeAnimal(animal);
        }

        return result;
    }

    public IEnumerable<HexCoordinate> getNeighboors(HexCoordinate coordinate)
    {
        foreach(HexCoordinate coord in hexDirections)
        {
            HexCoordinate c = coordinate + coord;
            if(isValidCoordinate(c))
            {
                yield return c;
            }
        }
    }

    public PlaceTileReturnTypes canPlaceTile(HexCoordinate coordinate, TileType tile)
    {
        GridCell cell;
        if(grid.TryGetValue(coordinate, out cell))
        {
            if(cell.canPlace(tile))
            {
                return PlaceTileReturnTypes.Validated;
            }
            else
            {
                return PlaceTileReturnTypes.InvalidType;
            }
        }

        return PlaceTileReturnTypes.InvalidCoordinate;
    }

    public (bool, int) matchShape(HexCoordinate coordinate, HexShape shape)
    {
        bool result = true;
        int rotation = 0;
        Assert.IsTrue(shape.pieces[0].offset.q == 0);
        Assert.IsTrue(shape.pieces[0].offset.r == 0);
        Assert.IsTrue(shape.pieces[0].offset.s == 0);

        if(shape.pieces.Count > 0)
        {
            if(haveAnimal(coordinate) != Animals.None)
            {
                return (false, 0);
            }
            result = matchShape(coordinate, shape.pieces[0].levels);
            
            if(result && (shape.pieces.Count > 1))
            {
                for(; rotation < 6; ++rotation)
                {
                    HexCoordinate c1 = coordinate + HexCoordinate.rotate60(shape.pieces[1].offset, rotation);
                    bool matchCurrent = matchShape(c1, shape.pieces[1].levels);

                    if(matchCurrent)
                    {
                        bool allMatched = true;
                        for(int i = 2; i < shape.pieces.Count; ++i)
                        {
                            if(matchShape(coordinate + HexCoordinate.rotate60(shape.pieces[i].offset, rotation), shape.pieces[i].levels) == false)
                            {
                                allMatched = false;
                                break;
                            }
                        }

                        if(allMatched)
                        {
                            return (true, rotation);
                        }
                    }
                }

                result = false; // @Note searched all neighboors, none came back true
            }
            
        }

        return (result, rotation);
    }

    public uint getScoreFromBoard(TileType type)
    {
        uint score = 0;
        switch(type)
        {
            case TileType.Green:
            {
                foreach(var (coordinate, cell) in iterate())
                {
                    TileType topType = cell.getTopTile();
                    if(topType == TileType.Green)
                    {
                        switch(cell.currentLevel)
                        {
                            case 1 : score += HardRules.greenSize1Points; break;
                            case 2 : score += HardRules.greenSize2Points; break;
                            case 3 : score += HardRules.greenSize3Points; break;
                        }
                    }
                }
                break;
            }
            case TileType.Red:
            {
                foreach(var (coordinate, cell) in iterate())
                {
                    if(cell.currentLevel == 2 && cell.getTopTile() == TileType.Red)
                    {
                        uint differentNeighboors = 0;
                        uint flags = 0;
                        foreach(HexCoordinate c in getNeighboors(coordinate))
                        {
                            GridCell neighboor;
                            if(grid.TryGetValue(c, out neighboor))
                            {
                                TileType t = neighboor.getTopTile();
                                uint fl = (uint)1 << (int)t;
                                if((flags & fl) == 0)
                                {
                                    ++differentNeighboors;
                                    flags |= fl;
                                }
                            }
                        }

                        score += (differentNeighboors >= 3) ? HardRules.redValidated : HardRules.redNotValidated;
                    }
                }
                break;
            }
            case TileType.Gray:
            {
                foreach(var (coordinate, cell) in iterate())
                {
                    if(cell.getTopTile() == TileType.Gray)
                    {
                        bool hasGrayNeighboor = false;
                        foreach(HexCoordinate c in getNeighboors(coordinate))
                        {
                            GridCell neighboor;
                            if(grid.TryGetValue(c, out neighboor))
                            {
                                TileType t = neighboor.getTopTile();
                                if(t == TileType.Gray)
                                {
                                    hasGrayNeighboor = true;
                                    break;
                                }
                            }
                        }

                        if(hasGrayNeighboor)
                        {
                            switch(cell.currentLevel)
                            {
                                case 1 : score += HardRules.graySize1Points; break;
                                case 2 : score += HardRules.graySize2Points; break;
                                case 3 : score += HardRules.graySize3Points; break;
                            }
                        }
                    }
                }
                break;
            }
            case TileType.Yellow:
            {
                List<HexCoordinate> alreadyCounted = new List<HexCoordinate>();
                uint validatedFields = 0;
                foreach(var (coordinate, cell) in iterate())
                {
                    TileType t0 = cell.getTopTile();
                    if(t0 == TileType.Yellow)
                    {
                        if((alreadyCounted.Exists( c => c == coordinate) == false))
                        {
                            alreadyCounted.Add(coordinate);
                            bool validatedNeighboor = false;
                            foreach(HexCoordinate c in getNeighboors(coordinate))
                            {
                                GridCell neighboor;
                                if(grid.TryGetValue(c, out neighboor))
                                {
                                    TileType t = neighboor.getTopTile();
                                    if(t == TileType.Yellow && (alreadyCounted.Exists( c0 => c0 == c) == false))
                                    {
                                        if(validatedNeighboor == false)
                                        {
                                            ++validatedFields;
                                            validatedNeighboor = true;
                                        }

                                        alreadyCounted.Add(c);
                                        annotateAllYellowFields(c, alreadyCounted);
                                    }
                                }
                            }
                        }
                    }
                }

                score += validatedFields * HardRules.yellowPoints;
                break;
            }
        }
        return score;
    }

    private void annotateAllYellowFields(HexCoordinate coordinate, List<HexCoordinate> coords)
    {
        foreach(HexCoordinate c in getNeighboors(coordinate))
        {
            GridCell neighboor;
            if(grid.TryGetValue(c, out neighboor))
            {
                TileType t = neighboor.getTopTile();
                if(t == TileType.Yellow && (coords.Exists(c0 => c0 == c) == false))
                {
                    coords.Add(c);
                    annotateAllYellowFields(c, coords);
                }
            }
        }
    }

    public bool matchShape(HexCoordinate coordinate, TileType[] tiles)
    {
        GridCell cell;
        if(grid.TryGetValue(coordinate, out cell))
        {
            return CellOperations.sameTileArrays(tiles, cell.levels);
        }
        return false;
    }

    public Animals haveAnimal(HexCoordinate coordinate)
    {
        GridCell cell;
        if(grid.TryGetValue(coordinate, out cell))
        {
            return cell.animalOnTile;
        }
        return Animals.None;
    }

    public IEnumerable<(HexCoordinate, int)> getMatchingPositions(HexShape shape)
    {
        foreach (var (key, value) in grid)
        {
            var (matched, rotation) = matchShape(key, shape);
            if(matched)
            {
                yield return (key, rotation);
            }
        }
    }
};