using System;

public enum TileType
{
    None,
    Blue,
    Gray,
    Brown,
    Green,
    Yellow,
    Red,
};

public enum Animals
{
    None,
    Kingfisher,
    Parrot,
    Crow,
    Bee,
};

public enum Phase 
{
    SelectTokens,
    PlaceTokens,
};

// @Note We can move this to be editable in Game Rules if we need to
public class HardRules
{
    public const int maxElevation  = 3;
    public const int choiceOptions = 5;
    public const int tokenNumberPerSlot = 3;

    public const uint greenSize1Points = 1;
    public const uint greenSize2Points = 3;
    public const uint greenSize3Points = 7;

    public const uint redValidated = 5;
    public const uint redNotValidated = 0;

    public const uint graySize1Points = 1;
    public const uint graySize2Points = 3;
    public const uint graySize3Points = 7;

    public const uint normalRows = 5;
    public const uint normalColumns = 5;
};

public class VisualValues
{
    public const float tokenHeight = 0.25f;
};

public class Card
{
    Animals type;

};

public interface IGameRandom
{
    public TileType getRandomTileType();
    public int getRandomValueInclusive(int min, int max);
}

public class RandomGenerator : IGameRandom
{
    private System.Random random;

    public RandomGenerator(int seed)
    {
        random = new System.Random(seed);
    }

    public int getRandomValueInclusive(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    public TileType getRandomTileType()
    {
        Array values = Enum.GetValues(typeof(TileType));
        return (TileType)random.Next( 1, values.Length); 
    }
};