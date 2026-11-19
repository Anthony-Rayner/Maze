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

            int numberOfLevels = File.ReadAllLines(levelData).Length / 8;
            int levelNumber = 1;
            string[,] tiles = GetTiles(levelData, levelNumber);
            Vec2 startPoint = FindTile(tiles, "O");
            Vec2 endPoint = FindTile(tiles, "X");

            // Set up entities (including player). 
            Entity[] entities = new Entity[100];
            Entity player = new Entity("0", startPoint.x, startPoint.y);
            entities[99] = player;

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
                else if (key == 'r')
                {
                    tiles = GetTiles(levelData, levelNumber);
                    player.position = startPoint;
                }
                Console.WriteLine("ha");

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
                        startPoint = FindTile(tiles, "O");
                        endPoint = FindTile(tiles, "X");
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
                    tiles[x, y % 8] = Convert.ToString(lines[y][x]);
                }
            }
            return tiles;
        }


        static void DisplayTiles(string[,] tiles, Entity[] entities, string title = "")
        {
            // Clear console so it can be updated. 
            Console.Clear();

            // Write stage number.
            if (title != "")
            {
                Console.WriteLine(title + "\n");
            }

            // Draw each tile, iterating through them. 
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
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
                Console.WriteLine();
            }
        }

        static string IsEntityInSpace(Entity[] entities, int x, int y)
        {
            foreach (Entity entity in entities)
            {
                if (entity != null)
                {
                    if (entity.position.x == x && entity.position.y == y)
                    {
                        return entity.sprite;
                    }
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
                    // Collide block with boundaries.
                    if (position.x + amount.x < 0 || position.x + amount.x > 15 || position.y + amount.y < 0 || position.y + amount.y > 7)
                    {
                        position.x -= amount.x;
                        position.y -= amount.y;
                    }
                    // Push block forwards.
                    else if (tiles[position.x + amount.x, position.y + amount.y] == ".")
                    {
                        tiles[position.x, position.y] = ".";
                        tiles[position.x + amount.x, position.y + amount.y] = "░";
                    }
                    // Collide block with walls.
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


        record struct Vec2(int x, int y);
    }
}