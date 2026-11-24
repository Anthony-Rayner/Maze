using System.Net;
using System.Runtime.CompilerServices;

namespace Maze
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Get the path to the level data file. 
            string basePath = AppContext.BaseDirectory;
            string levelData = Path.Combine(basePath, @"..\..\..\Levels.txt");

            const string startChar = "O";
            const string endChar = "E";

            int numberOfLevels = File.ReadAllLines(levelData).Length / 8;
            int levelNumber = 1;
            string[,] tiles = GetTiles(levelData, levelNumber);
            Vec2 startPoint = FindTile(tiles, startChar);
            Vec2 endPoint = FindTile(tiles, endChar);

            // Set up entities (including player). 
            List<Entity> entities = GetEntities(levelData, levelNumber);
            Entity player = new Entity("0", startPoint.x, startPoint.y);
            entities.Add(player);

            while (true)
            {
                // Draw and get input. 
                DisplayTiles(tiles, entities, "Stage: " + levelNumber + "/" + numberOfLevels);
                char key = Console.ReadKey().KeyChar;

                // Move player. 
                Vec2 moveAmount = new Vec2(0, 0);
                if (key == 'a')
                {
                    moveAmount.x -= 1;
                }
                else if (key == 'd')
                {
                    moveAmount.x += 1;
                }
                else if (key == 'w')
                {
                    moveAmount.y -= 1;
                }
                else if (key == 's')
                {
                    moveAmount.y += 1;
                }
                // Reset level.
                else if (key == 'r')
                {
                    tiles = GetTiles(levelData, levelNumber);
                    player.position = startPoint;
                }
                // Debug mode.
                else if (key == 'z')
                {
                    // Get input.
                    Console.Clear();
                    string? input = "";
                    int result = 0;
                    while (!int.TryParse(input, out result) || result < 0 || result > numberOfLevels)
                    {
                        Console.Write("Enter level to warp to: ");
                        input = Console.ReadLine();
                    }

                    // Warp.
                    levelNumber = result;
                    tiles = GetTiles(levelData, levelNumber);
                    startPoint = FindTile(tiles, startChar);
                    endPoint = FindTile(tiles, endChar);
                    player.position = startPoint;
                    entities = GetEntities(levelData, levelNumber);
                    entities.Add(player);
                }

                player.Move(moveAmount, tiles);

                // Check if player has beaten the level.
                if (Equals(player.position, endPoint))
                {
                    levelNumber += 1;
                    if (levelNumber > numberOfLevels)
                    {
                        EndGame();
                    }
                    else
                    {
                        tiles = GetTiles(levelData, levelNumber);
                        entities = GetEntities(levelData, levelNumber);
                        entities.Add(player);
                        startPoint = FindTile(tiles, startChar);
                        endPoint = FindTile(tiles, endChar);
                        player.position = startPoint;
                    }
                }
            }
        }


        static void EndGame()
        {
            Console.Clear();
            Console.WriteLine("Congratulations!\nYou have escaped the maze.");
            Environment.Exit(0);
        }


        static Vec2 FindTile(string[,] tiles, string tile)
        {
            for (int x = 0; x < tiles.GetLength(0); x += 1)
            {
                for (int y = 0; y < tiles.GetLength(1); y += 1)
                {
                    if (tiles[x, y] == tile)
                    {
                        return new Vec2(x, y);
                    }
                }
            }
            return new Vec2(-1, -1);
        }


        static string[,] GetTiles(string levelData, int levelNumber)
        {
            string[,] tiles = new string[16, 8];
            string[] lines = File.ReadAllLines(levelData);
            for (int y = (levelNumber - 1) * 8; y < (levelNumber) * 8; y += 1)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    string tile = Convert.ToString(lines[y][x]);
                    if (tile == "*")
                    {
                        tiles[x, y % 8] = ".";
                    }
                    else
                    {
                        tiles[x, y % 8] = tile;
                    }
                }
            }
            return tiles;
        }


        static List<Entity> GetEntities(string levelData, int levelNumber)
        {
            List<Entity> entities = new List<Entity>();
            string[] lines = File.ReadAllLines(levelData);
            for (int y = (levelNumber - 1) * 8; y < (levelNumber) * 8; y += 1)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    string tile = Convert.ToString(lines[y][x]);
                    if (tile == "*")
                    {
                        entities.Add(new Entity("*", x, y));
                    }
                }
            }
            return entities;
        }


        static void DisplayTiles(string[,] tiles, List<Entity> entities, string title = "")
        {
            // Clear console so it can be updated. 
            Console.Clear();

            // Write stage number.
            if (title != "")
            {
                Console.WriteLine(title + "\n");
            }

            Console.WriteLine("+------------------+");
            Console.WriteLine("|▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|");

            // Draw each tile, iterating through them. 
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Console.Write("|▓");
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    string entitySprite = IsEntityInSpace(entities, x, y);
                    if (entitySprite == "")
                    {
                        Console.Write(tiles[x, y]);
                    }
                    else
                    {
                        Console.Write(entitySprite);
                    }
                }
                Console.Write("▓|\n");
            }

            Console.WriteLine("|▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|");
            Console.WriteLine("+------------------+");
        }


        static string IsEntityInSpace(List<Entity> entities, int x, int y)
        {
            foreach (Entity entity in entities)
            {
                if (entity.position.x == x && entity.position.y == y)
                {
                    return entity.sprite;
                }
            }
            return "";
        }


        static bool Equals(Vec2 a, Vec2 b)
        {
            return a.x == b.x && a.y == b.y;
        }


        class Entity
        {
            public string sprite;
            public Vec2 position = new Vec2(0, 0);

            public void Move(Vec2 amount, string[,] tiles)
            {
                position.x += amount.x;
                position.y += amount.y;

                if (position.x > 15)
                {
                    position.x = 15;
                }
                else if (position.x < 0)
                {
                    position.x = 0;
                }

                if (position.y > 7)
                {
                    position.y = 7;
                }
                else if (position.y < 0)
                {
                    position.y = 0;
                }

                // Collide player with walls.
                if (tiles[position.x, position.y] == "▓")
                {
                    position.x -= amount.x;
                    position.y -= amount.y;
                }
                else if (tiles[position.x, position.y] == "░")
                {
                    if (CanPush(tiles, position, amount))
                    {
                        Push(tiles, position, amount);
                    }
                    else
                    {
                        position.x -= amount.x;
                        position.y -= amount.y;
                    }
                }
            }

            public Entity(string sprite, int x, int y)
            {
                this.sprite = sprite;
                this.position.x = x;
                this.position.y = y;
            }
        }


        static void Push(string[,] tiles, Vec2 position, Vec2 amount)
        {
            Vec2 logicPosition = new Vec2(position.x + amount.x, position.y + amount.y);
            while (tiles[logicPosition.x, logicPosition.y] == "░")
            {
                logicPosition = new Vec2(logicPosition.x + amount.x, logicPosition.y + amount.y);
            }
            while (logicPosition != position)
            {
                tiles[logicPosition.x, logicPosition.y] = "░";
                logicPosition = new Vec2(logicPosition.x - amount.x, logicPosition.y - amount.y);
            }
            tiles[logicPosition.x, logicPosition.y] = ".";
        }


        static bool CanPush(string[,] tiles, Vec2 position, Vec2 amount)
        {
            Vec2 pushPosition = new Vec2(position.x + amount.x, position.y + amount.y);
            // World boundaries.
            if (pushPosition.x < 0 || pushPosition.x > 15 || pushPosition.y < 0 || pushPosition.y > 7)
            {
                return false;
            }
            // Solid wall.
            else if (tiles[pushPosition.x, pushPosition.y] == "▓")
            {
                return false;
            }
            // Chain of blocks.
            else if (tiles[pushPosition.x, pushPosition.y] == "░")
            {
                return CanPush(tiles, pushPosition, amount);
            }
            // Empty space.
            else
            {
                return true;
            }
        }


        record struct Vec2(int x, int y);
    }
}