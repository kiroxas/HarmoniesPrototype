public class CellOperations
{
    static public bool canBePlacedAbove(TileType type, TileType below, int level)
    {
        switch(type)
        {
            case TileType.Gray : return below == TileType.Gray;
            case TileType.Red  : return (level == 1) && ((below == TileType.Gray) || (below == TileType.Red) || (below == TileType.Brown));
            case TileType.Brown  : return (below == TileType.Brown) && (level == 1);
            case TileType.Green  : return below == TileType.Brown;
            default : return false;
        }
    }

    static public bool sameTileArrays(TileType[] l0, TileType[] l1)
    {
        if(l0.Length == l1.Length)
        {
            if(l0.Length == 2 && l0[1] == TileType.Red && l1[1] == TileType.Red) // @Note Special rule for red, as the one below is not important, could be different colors
            {
                return true;
            }

            for(int i = 0; i < l0.Length; ++i)
            {
                if(l0[i] != l1[i])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
   
};

public class GridCell
{
    public int       currentLevel { get; private set; }
    public Animals   animalOnTile { get; private set; }
    public TileType[] levels { get; private set; }

    public GridCell()
    {
        levels = new TileType[HardRules.maxElevation];
        clear();
    }

    public void clear()
    {
        currentLevel = 0;
        animalOnTile = Animals.None;
    }

    public TileType getTileAtLevel(int level)
    {
        if(level < 0 || level >= currentLevel )
        {
            return TileType.None;
        }
        return levels[level];
    }

    public bool placeAnimal(Animals animal)
    {
        bool result = animalOnTile == Animals.None;
        if(result)
        {
            animalOnTile = animal;
        }
        return result;
    }
    
    public int addTile(TileType type)
    {
        int result = 0;
        if(currentLevel < levels.Length)
        {
            result = currentLevel;
            levels[currentLevel] = type;
            ++currentLevel;
        }

        return result;
    }

    public bool canPlace(TileType type)
    {
        if(animalOnTile != Animals.None)
        {
            return false;
        }

        switch(currentLevel)
        {
            case 0 : return true;
            case 1 : return CellOperations.canBePlacedAbove(type, getTileAtLevel(currentLevel - 1), currentLevel);
            case 2 : return CellOperations.canBePlacedAbove(type, getTileAtLevel(currentLevel - 1), currentLevel);
            default : return false;
        }
    }
};