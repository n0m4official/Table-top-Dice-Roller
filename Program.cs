using System;
using System.Security.Cryptography;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

class DiceRoller
{
    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    // Roll multiple dice and return results
    public static int[] RollMultiple(int count, int sides)
    {
        int[] results = new int[count];
        for (int i = 0; i < count; i++)
            results[i] = RandomInt(1, sides);
        return results;
    }

    private static int RandomInt(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max + 1);
    }

    // ASCII faces for D6
    private static readonly string[][] DiceFaces = new string[][]
    {
        null,
        new string[]{ "+-----+", "|     |", "|  *  |", "|     |", "+-----+" },
        new string[]{ "+-----+", "|*    |", "|     |", "|    *|", "+-----+" },
        new string[]{ "+-----+", "|*    |", "|  *  |", "|    *|", "+-----+" },
        new string[]{ "+-----+", "|*   *|", "|     |", "|*   *|", "+-----+" },
        new string[]{ "+-----+", "|*   *|", "|  *  |", "|*   *|", "+-----+" },
        new string[]{ "+-----+", "|*   *|", "|*   *|", "|*   *|", "+-----+" },
    };

    // Animate D6 dice
    public static void AnimateDice(int[] finalResults)
    {
        int diceCount = finalResults.Length;
        int[] current = new int[diceCount];

        for (int i = 0; i < diceCount; i++)
            current[i] = RandomInt(1, 6);

        // Stop dice left to right
        for (int stopIndex = 0; stopIndex < diceCount; stopIndex++)
        {
            int frames = 10 + stopIndex * 2;
            for (int f = 0; f < frames; f++)
            {
                Console.Clear();
                for (int i = stopIndex; i < diceCount; i++)
                    current[i] = RandomInt(1, 6);
                DrawDiceGrid(current, 6); // 6 per row
                Thread.Sleep(RandomInt(50, 120));
            }
            current[stopIndex] = finalResults[stopIndex]; // lock final value
        }

        // Final display
        Console.Clear();
        DrawDiceGrid(finalResults, 6);
    }

    // Draw dice in a grid with n dice per row
    private static void DrawDiceGrid(int[] dice, int perRow)
    {
        int totalDice = dice.Length;
        int rowsNeeded = (int)Math.Ceiling(totalDice / (double)perRow);

        for (int r = 0; r < rowsNeeded; r++)
        {
            int start = r * perRow;
            int end = Math.Min(start + perRow, totalDice);

            for (int line = 0; line < 5; line++) // each die has 5 lines
            {
                for (int d = start; d < end; d++)
                {
                    string[] face = DiceFaces[dice[d]];
                    Console.Write(face[line]);
                    Console.Write("  "); // spacing
                }
                Console.WriteLine();
            }
            Console.WriteLine(); // space between rows
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Dynamic Tabletop Dice Roller!");
        Console.WriteLine("Format: NDx+M (e.g., 3D6+2) or 'q' to quit.");
        Console.WriteLine("Supported dice: D3, D4, D6, D8, D10, D12, D20, D100\n");

        while (true)
        {
            Console.Write("Enter your roll: ");
            string input = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(input) || input == "Q") break;

            var match = System.Text.RegularExpressions.Regex.Match(input, @"^(\d+)D(\d+)([+-]\d+)?$");
            if (!match.Success)
            {
                Console.WriteLine("Invalid format. Example: 2D6+1");
                continue;
            }

            if (!int.TryParse(match.Groups[1].Value, out int count) ||
                !int.TryParse(match.Groups[2].Value, out int sides))
            {
                Console.WriteLine("Invalid numbers entered.");
                continue;
            }

            if (count <= 0 || count > 100)
            {
                Console.WriteLine("You can roll between 1 and 100 dice at a time.");
                continue;
            }

            int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

            int[] allowedDice = { 3, 4, 6, 8, 10, 12, 20, 100 };
            if (Array.IndexOf(allowedDice, sides) == -1)
            {
                Console.WriteLine("Unsupported dice type. Use 3,4,6,8,10,12,20,100.");
                continue;
            }

            int[] rolls = DiceRoller.RollMultiple(count, sides);

            // Animate D6 dice if applicable
            if (sides == 6)
                DiceRoller.AnimateDice(rolls);
            else
            {
                Console.Clear();
                Console.WriteLine("Dice results: " + string.Join(", ", rolls));
            }

            // Calculate total
            int total = rolls.Sum() + modifier;
            Console.WriteLine($"\nTotal (with modifier): {total}");

            // Breakdown of each number rolled
            var breakdown = rolls.GroupBy(x => x)
                                 .OrderBy(g => g.Key)
                                 .Select(g => $"{g.Key}: {g.Count()}");
            Console.WriteLine("Roll breakdown: " + string.Join(", ", breakdown));

            Console.WriteLine("\nPress Enter to roll again...");
            Console.ReadLine();
        }

        Console.WriteLine("Thanks for rolling! Goodbye.");
    }
}