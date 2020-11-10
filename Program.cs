// Made by: Kat9_123 & KeremEskicinar


using System;
using System.Threading;
using System.Diagnostics;

namespace Tetris
{


    class Program
    {
        // Map / BG 
        const int mapSizeX = 10;
        const int mapSizeY = 20;
        static char[,] bg = new char[mapSizeY, mapSizeX];


        static int score = 0;

        // Hold variables
        const int holdSizeX = 6;
        const int holdSizeY = mapSizeY;
        static int holdIndex = -1;
        static char holdChar;

        const int upNextSize = 6;


        static ConsoleKeyInfo input;


        // Current info
        static int currentX = 0;
        static int currentY = 0;
        static char currentChar = 'O';

        static int currentRot = 0;



        // Block and Bogs        
        static int[] bag;
        static int[] nextBag;

        static int bagIndex;
        static int currentIndex;


        // misc
        static int maxTime = 20;
        static int timer = 0;
        static int amount = 0;


        #region Assets


        /* Possible modification
        readonly static ConsoleColor[] colours = 
        {
            ConsoleColor.Red,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.White,
            ConsoleColor.Cyan
        };
        */
        readonly static string characters = "OILJSZT";
        readonly static int[,,,] positions =
        {
        {
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}}
        },

        {
        {{2,0},{2,1},{2,2},{2,3}},
        {{0,2},{1,2},{2,2},{3,2}},
        {{1,0},{1,1},{1,2},{1,3}},
        {{0,1},{1,1},{2,1},{3,1}},
        },
        {
        {{1,0},{1,1},{1,2},{2,2}},
        {{1,2},{1,1},{2,1},{3,1}},
        {{1,1},{2,1},{2,2},{2,3}},
        {{2,1},{2,2},{1,2},{0,2}}
        },

        {
        {{2,0},{2,1},{2,2},{1,2}},
        {{1,1},{1,2},{2,2},{3,2}},
        {{2,1},{1,1},{1,2},{1,3}},
        {{0,1},{1,1},{2,1},{2,2}}
        },

        {
        {{2,1},{1,1},{1,2},{0,2}},
        {{1,0},{1,1},{2,1},{2,2}},
        {{2,1},{1,1},{1,2},{0,2}},
        {{1,0},{1,1},{2,1},{2,2}}
        },
        {
        {{0,1},{1,1},{1,2},{2,2}},
        {{1,0},{1,1},{0,1},{0,2}},
        {{0,1},{1,1},{1,2},{2,2}},
        {{1,0},{1,1},{0,1},{0,2}}
        },

        {
        {{0,1},{1,1},{1,0},{2,1}},
        {{1,0},{1,1},{2,1},{1,2}},
        {{0,1},{1,1},{1,2},{2,1}},
        {{1,0},{1,1},{0,1},{1,2}}
        }
        };
        #endregion
        static void Main()
        {
            // Make the console cursor invisible
            Console.CursorVisible = false;

            // Title
            Console.Title = "Tetris | By: Kat9_123 & KeremEskicinar";

            // Start the inputthread to get live inputs
            Thread inputThread = new Thread(Input);
            inputThread.Start();

            // Generate bag / current block
            bag = GenerateBag();
            nextBag = GenerateBag();
            NewBlock();

            // Generate an empty bg
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < mapSizeX; x++)
                    bg[y, x] = '-';

            while (true)
            {

                // Force block down
                if (timer >= maxTime)
                {
                    // If it doesn't collide, just move it down. If it does call BlockDownCollision
                    if (!Collision(currentIndex, bg, currentX, currentY + 1, currentRot)) currentY++;
                    else BlockDownCollision();

                    timer = 0;
                }
                timer++;




                // INPUT
                InputHandler(); // Call InputHandler
                input = new ConsoleKeyInfo(); // Reset input var


                // RENDER CURRENT
                char[,] view = RenderView(); // Render view (Playing field)

                // RENDER HOLD
                char[,] hold = RenderHold(); // Render hold (the current held block)


                //RENDER UP NEXT
                char[,] next = RenderUpNext(); // Render the next three blocks as an 'up next' feature

                // PRINT VIEW
                Print(view, hold, next); // Print everything to the screen

                Thread.Sleep(20); // Wait to not overload the processor (I think it's better because it has no impact on game feel)
            }


        }


        static void InputHandler()
        {
            switch (input.Key)
            {
                // Left arrow = move left (if it doesn't collide)
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (!Collision(currentIndex, bg, currentX - 1, currentY, currentRot)) currentX -= 1;
                    break;

                // Right arrow = move right (if it doesn't collide)
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (!Collision(currentIndex, bg, currentX + 1, currentY, currentRot)) currentX += 1;
                    break;

                // Rotate block (if it doesn't collide)
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    int newRot = currentRot + 1;
                    if (newRot >= 4) newRot = 0;
                    if (!Collision(currentIndex, bg, currentX, currentY, newRot)) currentRot = newRot;

                    break;

                // Move the block instantly down (hard drop)
                case ConsoleKey.Spacebar:
                    int i = 0;
                    while (true)
                    {
                        i++;
                        if (Collision(currentIndex, bg, currentX, currentY + i, currentRot))
                        {
                            currentY += i - 1;
                            break;
                        }

                    }
                    score += i + 1;
                    break;

                // Quit
                case ConsoleKey.Escape:
                    Environment.Exit(1);
                    break;

                // Hold block
                case ConsoleKey.Enter:

                    // If there isnt a current held block:
                    if (holdIndex == -1)
                    {
                        holdIndex = currentIndex;
                        holdChar = currentChar;
                        NewBlock();
                    }
                    // If there is:
                    else
                    {
                        if (!Collision(holdIndex, bg, currentX, currentY, 0)) // Check for collision
                        {

                            // Switch current and hold
                            int c = currentIndex;
                            char ch = currentChar;
                            currentIndex = holdIndex;
                            currentChar = holdChar;
                            holdIndex = c;
                            holdChar = ch;
                        }

                    }
                    break;

                // Move down faster
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    timer = maxTime;
                    break;

                case ConsoleKey.R:
                    Restart();
                    break;

                default:
                    break;
            }
        }
        static void BlockDownCollision()
        {

            // Add blocks from current to background
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                bg[positions[currentIndex, currentRot, i, 1] + currentY, positions[currentIndex, currentRot, i, 0] + currentX] = currentChar;
            }

            // Loop 
            while (true)
            {
                // Check for line
                int lineY = Line(bg);

                // If a line is detected
                if (lineY != -1)
                {
                    ClearLine(lineY);

                    continue;
                }
                break;
            }
            // New block
            NewBlock();

        }


            static void Restart()
        {
            var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Process.Start(applicationPath);
            Environment.Exit(Environment.ExitCode);
        }

        static void ClearLine(int lineY)
        {
            score += 40;
            // Clear said line
            for (int x = 0; x < mapSizeX; x++) bg[lineY, x] = '-';

            // Loop through all blocks above line
            for (int y = lineY - 1; y > 0; y--)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    // Move each character down
                    char character = bg[y, x];
                    if (character != '-')
                    {
                        bg[y, x] = '-';
                        bg[y + 1, x] = character;
                    }

                }
            }
        }

        static char[,] RenderView()
        {
            char[,] view = new char[mapSizeY, mapSizeX];

            // Make view equal to bg
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < mapSizeX; x++)
                    view[y, x] = bg[y, x];



            // Overlay current
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                view[positions[currentIndex, currentRot, i, 1] + currentY, positions[currentIndex, currentRot, i, 0] + currentX] = currentChar;
            }
            return view;
        }

        static char[,] RenderHold()
        {
            char[,] hold = new char[holdSizeY, holdSizeX];
            // Hold = ' ' array
            for (int y = 0; y < holdSizeY; y++)
                for (int x = 0; x < holdSizeX; x++)
                    hold[y, x] = ' ';


            // If there is a held block
            if (holdIndex != -1)
            {
                // Overlay blocks from hold
                for (int i = 0; i < positions.GetLength(2); i++)
                {
                    hold[positions[holdIndex, 0, i, 1] + 1, positions[holdIndex, 0, i, 0] + 1] = holdChar;
                }
            }
            return hold;
        }
        static char[,] RenderUpNext()
        {
            // Up next = ' ' array   
            char[,] next = new char[mapSizeY, upNextSize];
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < upNextSize; x++)
                    next[y, x] = ' ';


            int nextBagIndex = 0;
            for (int i = 0; i < 3; i++) // Next 3 blocks
            {

                for (int l = 0; l < positions.GetLength(2); l++)
                {
                    if (i + bagIndex >= 7) // If we need to acces the next bag
                        next[positions[nextBag[nextBagIndex], 0, l, 1] + 5 * i, positions[nextBag[nextBagIndex], 0, l, 0] + 1] = characters[nextBag[nextBagIndex]];
                    else
                        next[positions[bag[bagIndex + i], 0, l, 1] + 5 * i, positions[bag[bagIndex + i], 0, l, 0] + 1] = characters[bag[bagIndex + i]];


                }
                if (i + bagIndex >= 7) nextBagIndex++;
            }
            return next;

        }

        static void Print(char[,] view, char[,] hold, char[,] next)
        {
            for (int y = 0; y < mapSizeY; y++)
            {

                for (int x = 0; x < holdSizeX + mapSizeX + upNextSize; x++)
                {
                    char i = ' ';
                    // Add hold + Main View + up next to view (basically dark magic)
                    if (x < holdSizeX) i = hold[y, x];
                    else if (x >= holdSizeX + mapSizeX) i = next[y, x - mapSizeX - upNextSize];
                    else i = view[y, (x - holdSizeX)];


                    // Colours
                    switch (i)
                    {
                        case 'O':
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(i);
                            break;
                        case 'I':
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(i);
                            break;

                        case 'T':
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(i);
                            break;

                        case 'S':
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.Write(i);
                            break;
                        case 'Z':
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(i);
                            break;
                        case 'L':
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(i);
                            break;

                        case 'J':
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(i);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write(i);
                            break;
                    }

                }
                if (y == 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("   " + score);
                }
                Console.WriteLine();
            }

            // Reset console cursor position
            Console.SetCursorPosition(0, Console.CursorTop - mapSizeY);
        }
        static int[] GenerateBag()
        {
            // Not my code, source https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
            Random random = new Random();
            int n = 7;
            int[] ret = { 0, 1, 2, 3, 4, 5, 6, 7 };
            while (n > 1)
            {
                int k = random.Next(n--);
                int temp = ret[n];
                ret[n] = ret[k];
                ret[k] = temp;

            }
            return ret;

        }
        static bool Collision(int index, char[,] bg, int x, int y, int rot)
        {

            for (int i = 0; i < positions.GetLength(2); i++)
            {
                // Check if out of bounds
                if (positions[index, rot, i, 1] + y >= mapSizeY || positions[index, rot, i, 0] + x < 0 || positions[index, rot, i, 0] + x >= mapSizeX)
                {
                    return true;
                }
                // Check if not '-'
                if (bg[positions[index, rot, i, 1] + y, positions[index, rot, i, 0] + x] != '-')
                {
                    return true;
                }
            }

            return false;
        }

        static int Line(char[,] bg)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                bool i = true;
                for (int x = 0; x < mapSizeX; x++)
                {
                    if (bg[y, x] == '-')
                    {
                        i = false;
                    }
                }
                if (i) return y;
            }

            // If no line return -1
            return -1;
        }

        static void NewBlock()
        {
            // Check if new bag is necessary
            if (bagIndex >= 7)
            {
                bagIndex = 0;
                bag = nextBag;
                nextBag = GenerateBag();
            }

            // Reset everything
            currentY = 0;
            currentX = 4;
            currentChar = characters[bag[bagIndex]];
            currentIndex = bag[bagIndex];

            // Check if the next block position collides. If it does its gameover
            if (Collision(currentIndex, bg, currentX, currentY, currentRot) && amount > 0)
            {
                GameOver();
            }
            bagIndex++;
            amount++;
        }


        static void GameOver()
        {
            Console.Clear();
            Console.WriteLine("\tGAME OVER");
            Console.WriteLine("\n\tWant to try again?");
            Console.Write("\tType 'Y' and hit Enter to try again: ");
            if (String.Compare(Console.ReadLine(), "Y", true) == 0)
            {
                // Restart game
                Restart();
            }
            else
            {
                // Or exit
                Environment.Exit(1);
            }
        }
        static void Input()
        {
            while (true)
            {
                // Get input
                input = Console.ReadKey(true);
            }
        }
    }
}
