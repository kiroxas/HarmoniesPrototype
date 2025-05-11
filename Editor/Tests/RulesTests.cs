using NUnit.Framework;
using System;
using System.Collections.Generic;

public class TestRandomGenerator : IGameRandom
{
    public TileType getRandomTileType()
    {
        return TileType.Red;
    }

    public int getRandomValueInclusive(int min, int max)
    {
        return min;
    }
};

namespace MyGame.RulesTests
{
    public class TileTests
    {
        [Test]
        public void canPlaceAnyTileOnAnEmptyGridCell()
        {
            foreach (TileType type in Enum.GetValues(typeof(TileType)))
            {
                GridCell cell = new GridCell();
                Assert.IsTrue(cell.canPlace(type));
            }
        }

        [Test]
        public void respectGrayTilePlacementRules()
        {
            GridCell cell = new GridCell();
            cell.addTile(TileType.Gray);
            Assert.IsTrue(cell.canPlace(TileType.Gray));
            Assert.IsFalse(cell.canPlace(TileType.Blue));
            Assert.IsTrue(cell.canPlace(TileType.Red));
        }

        [Test]
        public void respectTreeTilePlacementRules()
        {
            GridCell cell = new GridCell();
            cell.addTile(TileType.Brown);
            Assert.IsFalse(cell.canPlace(TileType.Gray));
            Assert.IsFalse(cell.canPlace(TileType.Blue));
            Assert.IsTrue(cell.canPlace(TileType.Red));
            Assert.IsTrue(cell.canPlace(TileType.Brown));
            Assert.IsTrue(cell.canPlace(TileType.Green));

            cell.addTile(TileType.Brown);
            Assert.IsFalse(cell.canPlace(TileType.Red));
            Assert.IsFalse(cell.canPlace(TileType.Brown));
            Assert.IsTrue(cell.canPlace(TileType.Green));
        }

        [Test]
        public void animalBlockTilePlacement()
        {
            GridCell cell = new GridCell();
            cell.addTile(TileType.Brown);
            bool added = cell.placeAnimal(Animals.Bee);
            Assert.IsTrue(added);
            Assert.IsFalse(cell.canPlace(TileType.Gray));
            Assert.IsFalse(cell.canPlace(TileType.Blue));
            Assert.IsFalse(cell.canPlace(TileType.Red));
            Assert.IsFalse(cell.canPlace(TileType.Brown));
            Assert.IsFalse(cell.canPlace(TileType.Green));

            added = cell.placeAnimal(Animals.Crow);
            Assert.IsFalse(added);
        }
    }

    public class SpecialRulesTests
    {
        [Test]
        public void tilesWithRedOnTopShouldMatch()
        {
            TileType[] arrayOne = new TileType[2];
            TileType[] arrayTwo = new TileType[2];

            arrayOne[0] = TileType.Brown;
            arrayOne[1] = TileType.Red;

            arrayTwo[0] = TileType.Red;
            arrayTwo[1] = TileType.Red;

            Assert.IsTrue(CellOperations.sameTileArrays(arrayOne, arrayTwo));
        }
    }

    public class CentralGameboardTests
    {
        [Test]
        public void correctlyFillCentralGameBoard()
        {
            int enumCount = Enum.GetValues(typeof(TileType)).Length;
            uint[] tilesNumber = new uint[enumCount];

            Array.Fill(tilesNumber, (uint)100);

            TestRandomGenerator random = new TestRandomGenerator();
            CentralGameboard central = new CentralGameboard(2, random, 3, tilesNumber);

            Assert.IsFalse(central.contains(TileType.Gray));
            Assert.IsFalse(central.contains(TileType.Blue));
            Assert.IsFalse(central.contains(TileType.Green));
            Assert.IsFalse(central.contains(TileType.Yellow));
            Assert.IsTrue(central.contains(TileType.Red));
        }

        [Test]
        public void shouldBeAbleToEmptyOneIndex()
        {
            int enumCount = Enum.GetValues(typeof(TileType)).Length;
            uint[] tilesNumber = new uint[enumCount];
            Array.Fill(tilesNumber, (uint)100);

            TestRandomGenerator random = new TestRandomGenerator();
            uint nbPerCell = 3;
            CentralGameboard central = new CentralGameboard(2, random, nbPerCell, tilesNumber);

            Assert.IsTrue(central.getCellContent(0).Count == nbPerCell);
            central.empty(0);
            Assert.IsTrue(central.getCellContent(0).Count == 0);
            central.refill(0, tilesNumber);
            Assert.IsTrue(central.getCellContent(0).Count == nbPerCell);
        }

        [Test]
        public void ResourceChoiceCanBeRemoved()
        {
            int enumCount = Enum.GetValues(typeof(TileType)).Length;
            uint[] tilesNumber = new uint[enumCount];
            Array.Fill(tilesNumber, (uint)100);

            TestRandomGenerator random = new TestRandomGenerator();
            ResourcesChoice c = new ResourcesChoice(random, 3, tilesNumber);

            Assert.IsTrue(c.contains(TileType.Red));
            Assert.IsTrue(c.tilesRemaining() == 3);
            Assert.IsTrue(c.isValid());

            c.consume(TileType.Red);

            Assert.IsTrue(c.contains(TileType.Red));
            Assert.IsTrue(c.tilesRemaining() == 2);
            Assert.IsTrue(c.isValid());

            c.consume(TileType.Red);

            Assert.IsTrue(c.contains(TileType.Red));
            Assert.IsTrue(c.tilesRemaining() == 1);
            Assert.IsTrue(c.isValid());

            c.consume(TileType.Red);

            Assert.IsFalse(c.contains(TileType.Red));
            Assert.IsTrue(c.tilesRemaining() == 0);
            Assert.IsFalse(c.isValid());
        }
    }

    public class GameTests
    {
        [Test]
        public void SimpleGameflow()
        {
            CardData[] cardsToSpawn = null;
            Resources[] resources = new Resources[2];
            resources[0] = new Resources(TileType.Blue, 25);

            GameRules grid = new GameRules(resources, cardsToSpawn);

            // @Note Invalid Color
            PlaceTileResult result = grid.placeTile(new HexCoordinate(0,0,0), TileType.Red);
            Assert.AreEqual(PlaceTileReturnTypes.InvalidPhase, result.result);
            bool selected = grid.selectTokenBoard(0);
            Assert.IsTrue(selected);

            // @Note Invalid Color
            PlaceTileResult result1 = grid.placeTile(new HexCoordinate(0,0,0), TileType.Red);
            Assert.AreEqual(PlaceTileReturnTypes.InvalidType, result1.result);

            // @Note Invalid Coord
            PlaceTileResult result2 = grid.placeTile(new HexCoordinate(-5,-10,15), TileType.Red);
            Assert.AreEqual(PlaceTileReturnTypes.InvalidCoordinate, result2.result);

            // @Note Valid move
            PlaceTileResult result3 = grid.placeTile(new HexCoordinate(0,0,0), TileType.Blue);
            Assert.AreEqual(PlaceTileReturnTypes.Validated, result3.result);
        }
    }

    public class HexaGridTests
    {
        [Test]
        public void greenTilesScores()
        {
            HexaGrid grid = new HexaGrid();

            grid.placeTile(new HexCoordinate(0,0), TileType.Brown);
            grid.placeTile(new HexCoordinate(0,0), TileType.Red);
            grid.placeTile(new HexCoordinate(0,2), TileType.Brown);
            grid.placeTile(new HexCoordinate(0,2), TileType.Green);
            grid.placeTile(new HexCoordinate(1,3), TileType.Blue);
            grid.placeTile(new HexCoordinate(2,3), TileType.Green);
            grid.placeTile(new HexCoordinate(3,2), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,1), TileType.Green);
            grid.placeTile(new HexCoordinate(3,0), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,-1), TileType.Blue);

            uint score = grid.getScoreFromBoard(TileType.Green);
            uint expectedScore = HardRules.greenSize1Points * 2 + HardRules.greenSize2Points;
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void redTilesScores()
        {
            HexaGrid grid = new HexaGrid();

            HexCoordinate origin = new HexCoordinate(2,2);
            grid.placeTile(origin, TileType.Brown);
            grid.placeTile(origin, TileType.Red);
            grid.placeTile(origin + new HexCoordinate(1,0), TileType.Brown);
            grid.placeTile(origin + new HexCoordinate(0,1), TileType.Green);
            grid.placeTile(origin + new HexCoordinate(1,-1), TileType.Blue); // @Note One validated red

            grid.placeTile(new HexCoordinate(2,3), TileType.Red); // @Note should not count as only on level 0
            grid.placeTile(new HexCoordinate(2,4), TileType.Yellow);
            grid.placeTile(new HexCoordinate(2,2), TileType.Green);
            grid.placeTile(new HexCoordinate(3,3), TileType.Blue);
            grid.placeTile(new HexCoordinate(3,2), TileType.Gray);
            grid.placeTile(new HexCoordinate(3,2), TileType.Red); // @Note One validated red

            uint score = grid.getScoreFromBoard(TileType.Red);
            uint expectedScore = HardRules.redValidated * 2 + HardRules.redNotValidated;
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void grayTilesScores()
        {
            HexaGrid grid = new HexaGrid(10, 10);

            HexCoordinate origin = new HexCoordinate(5,5);
            grid.placeTile(origin, TileType.Brown);
            grid.placeTile(origin, TileType.Red);
            grid.placeTile(origin + new HexCoordinate(1,0), TileType.Gray); // @Note no neighboors
            grid.placeTile(origin + new HexCoordinate(1,0), TileType.Gray); // @Note no neighboors
            grid.placeTile(origin + new HexCoordinate(0,1), TileType.Green);
            grid.placeTile(origin + new HexCoordinate(1,-1), TileType.Blue); 

            grid.placeTile(new HexCoordinate(3,3), TileType.Gray);
            grid.placeTile(new HexCoordinate(3,2), TileType.Gray);
            grid.placeTile(new HexCoordinate(3,2), TileType.Gray);
            grid.placeTile(new HexCoordinate(3,2), TileType.Gray);

            uint score = grid.getScoreFromBoard(TileType.Gray);
            uint expectedScore = HardRules.graySize1Points + HardRules.graySize3Points;
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void yellowTilesScores()
        {
            HexaGrid grid = new HexaGrid(10, 10);

            int p;
            HexCoordinate origin = new HexCoordinate(2,2);
            grid.placeTile(origin, TileType.Yellow);
            grid.placeTile(origin + new HexCoordinate(1,0), TileType.Yellow);
            grid.placeTile(origin + new HexCoordinate(0,1), TileType.Yellow);
            grid.placeTile(origin + new HexCoordinate(1,-1), TileType.Yellow); 
            grid.placeTile(origin + new HexCoordinate(-1,1), TileType.Yellow); // @Note All should just be one

            grid.placeTile(new HexCoordinate(5,5), TileType.Yellow); // @Note should not count

            p = grid.placeTile(new HexCoordinate(7,0), TileType.Yellow);
            Assert.AreEqual(0, p);
            p = grid.placeTile(new HexCoordinate(7,1), TileType.Yellow);// @Note All should just be one
            Assert.AreEqual(0, p);

            uint score = grid.getScoreFromBoard(TileType.Yellow);
            uint expectedScore = HardRules.yellowPoints * 2;
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void blueTilesScoresRing()
        {
            HexaGrid grid = new HexaGrid(10, 10);
            HexCoordinate origin = new HexCoordinate(1,1);

            foreach(HexCoordinate coord in HexaGrid.hexDirections)
            {
                grid.placeTile(origin + coord, TileType.Blue);
            }

            uint score = grid.getScoreFromBoard(TileType.Blue);
            uint expectedScore = RulesOperations.getCorrespondingBlueScore(6);
            Assert.AreEqual(expectedScore, score);
        }

        [Test]
        public void blueTilesScores()
        {
            HexaGrid grid = new HexaGrid(10, 10);

            HexCoordinate origin = new HexCoordinate(1,2);

            grid.placeTile(origin, TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(0, -1), TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(0, -2), TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(1, -2), TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(2, -2), TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(2, -1), TileType.Blue);
            grid.placeTile(origin + new HexCoordinate(3, -1), TileType.Blue);
            
            uint score = grid.getScoreFromBoard(TileType.Blue);
            uint expectedScore = RulesOperations.getCorrespondingBlueScore(6);
            Assert.AreEqual(expectedScore, score);
        }


        [Test]
        public void neighboorsShouldNotContainCenter()
        {
            HexaGrid grid = new HexaGrid();

            HexCoordinate coord = new HexCoordinate(0, 2);

            foreach(HexCoordinate c in grid.getNeighboors(coord))
            {
                Assert.IsFalse(coord == c);
            }
           
        }

        [Test]
        public void shouldMatchLine()
        {
            HexaGrid grid = new HexaGrid();

            HexCoordinate coord = new HexCoordinate(0,2);
            grid.placeTile(new HexCoordinate(0,0), TileType.Brown);
            grid.placeTile(new HexCoordinate(0,0), TileType.Red);
            grid.placeTile(coord, TileType.Red);
            grid.placeTile(coord, TileType.Green);
            grid.placeTile(new HexCoordinate(1,3), TileType.Blue);
            grid.placeTile(new HexCoordinate(2,3), TileType.Green);
            grid.placeTile(new HexCoordinate(3,2), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,1), TileType.Green);
            grid.placeTile(new HexCoordinate(3,0), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,-1), TileType.Blue);

            HexShape shape = new HexShape(TileType.Green);
            shape.addTile(new HexCoordinate(-1,0), TileType.Yellow);
            shape.addTile(new HexCoordinate(-2,0), TileType.Blue);

            int matchingCount = 0;
            foreach(var (key, rotation) in grid.getMatchingPositions(shape))
            {
                ++matchingCount; 
                Assert.IsTrue(key == new HexCoordinate(3,1));
            }


            Assert.IsTrue(matchingCount == 1);
        }

        [Test]
        public void shouldNotMatchPartialLine()
        {
            HexaGrid grid = new HexaGrid();

            HexCoordinate coord = new HexCoordinate(0,2);
            grid.placeTile(new HexCoordinate(0,0), TileType.Brown);
            grid.placeTile(new HexCoordinate(0,0), TileType.Red);
            grid.placeTile(coord, TileType.Red);
            grid.placeTile(coord, TileType.Green);
            grid.placeTile(new HexCoordinate(1,3), TileType.Blue);
            grid.placeTile(new HexCoordinate(2,3), TileType.Green);
            grid.placeTile(new HexCoordinate(3,2), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,1), TileType.Green);
            grid.placeTile(new HexCoordinate(3,0), TileType.Yellow);

            HexShape shape = new HexShape(TileType.Green);
            shape.addTile(new HexCoordinate(-1,0), TileType.Yellow);
            shape.addTile(new HexCoordinate(-2,0), TileType.Green);

            int matchingCount = 0;
            foreach(var (key, rotation) in grid.getMatchingPositions(shape))
            {
                ++matchingCount; 
            }


            Assert.IsTrue(matchingCount == 0);
        }

        [Test]
        public void shouldMatchComplex()
        {
            HexaGrid grid = new HexaGrid();

            grid.placeTile(new HexCoordinate(2,2), TileType.Brown);
            grid.placeTile(new HexCoordinate(2,2), TileType.Green);
            grid.placeTile(new HexCoordinate(2,1), TileType.Yellow);
            grid.placeTile(new HexCoordinate(3,1), TileType.Yellow);
            grid.placeTile(new HexCoordinate(1,2), TileType.Yellow);

            HexShape shape = new HexShape(TileType.Brown, TileType.Green);
            shape.addTile(new HexCoordinate(0, -1), TileType.Yellow);
            shape.addTile(new HexCoordinate(1, -1), TileType.Yellow);
            shape.addTile(new HexCoordinate(-1, 0), TileType.Yellow);

            int matchingCount = 0;
            foreach(var (key, rotation) in grid.getMatchingPositions(shape))
            {
                ++matchingCount; 
            }

            Assert.IsTrue(matchingCount == 1);
        }

        [Test]
        public void crowShapeShouldMatch()
        {
            HexaGrid grid = new HexaGrid();

            HexCoordinate origin = new HexCoordinate(2,2);
            grid.placeTile(origin, TileType.Yellow);
            grid.placeTile(origin + new HexCoordinate(1,-1), TileType.Brown);
            grid.placeTile(origin + new HexCoordinate(1,-1), TileType.Red);
            grid.placeTile(origin + new HexCoordinate(-1,0), TileType.Red);
            grid.placeTile(origin + new HexCoordinate(-1,0), TileType.Red);

            HexShape shape = new HexShape(TileType.Yellow);
            shape.addTile(new HexCoordinate(1, -1), TileType.Red, TileType.Red);
            shape.addTile(new HexCoordinate(-1, 0), TileType.Red, TileType.Red);

            int matchingCount = 0;
            foreach(var (key, rotation) in grid.getMatchingPositions(shape))
            {
                ++matchingCount; 
            }

            Assert.IsTrue(matchingCount == 1);
        }

        [Test]
        public void shouldMatchComplexRotated()
        {
            HexaGrid grid = new HexaGrid();

            grid.placeTile(new HexCoordinate(2,2), TileType.Brown);
            grid.placeTile(new HexCoordinate(2,2), TileType.Green);
            grid.placeTile(new HexCoordinate(3,2), TileType.Yellow);
            grid.placeTile(new HexCoordinate(2,3), TileType.Yellow);
            grid.placeTile(new HexCoordinate(1,3), TileType.Yellow);

            HexShape shape = new HexShape(TileType.Brown, TileType.Green);
            shape.addTile(new HexCoordinate(0, -1), TileType.Yellow);
            shape.addTile(new HexCoordinate(1, -1), TileType.Yellow);
            shape.addTile(new HexCoordinate(-1, 0), TileType.Yellow);

            int matchingCount = 0;
            foreach(var (key, rotation) in grid.getMatchingPositions(shape))
            {
                ++matchingCount; 
            }

            Assert.IsTrue(matchingCount == 1);
        }
    }

}