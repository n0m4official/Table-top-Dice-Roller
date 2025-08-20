using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

class DiceRoller
{
	private static Random rng;

	static DiceRoller()
	{
		long seed = DateTime.Now.Ticks
					^ Environment.TickCount
					^ System.Diagnostics.Process.GetCurrentProcess().Id
					^ (Environment.WorkingSet * 17);

		using (var crypto = RandomNumberGenerator.Create())
		{
			byte[] bytes = new byte[8];
			crypto.GetBytes(bytes);
			seed ^= BitConverter.ToInt64(bytes, 0);
		}

		rng = new Random((int)(seed & 0xFFFFFFFF));
	}

	public static int Roll(int sides)
	{
		// Shake animation
		Console.Write("Rolling");
		for (int i = 0; i < 5; i++)
		{
			Console.Write(".");
			Thread.Sleep(rng.Next(50, 200));
		}
		Console.WriteLine();

		return rng.Next(1, sides + 1);
	}

	public static void ShowDiceFace(int sides, int value)
	{
		// Only ASCII faces for D6; other dice types just show the number
		if (sides == 6)
		{
			string[] diceFace = value switch
			{
				1 => new string[] { "+-----+", "|     |", "|  *  |", "|     |", "+-----+" },
				2 => new string[] { "+-----+", "|*    |", "|     |", "|    *|", "+-----+" },
				3 => new string[] { "+-----+", "|*    |", "|  *  |", "|    *|", "+-----+" },
				4 => new string[] { "+-----+", "|*   *|", "|     |", "|*   *|", "+-----+" },
				5 => new string[] { "+-----+", "|*   *|", "|  *  |", "|*   *|", "+-----+" },
				6 => new string[] { "+-----+", "|*   *|", "|*   *|", "|*   *|", "+-----+" },
				_ => null
			};

			if (diceFace != null)
			{
				foreach (var line in diceFace)
					Console.WriteLine(line);
			}
		}
		else
		{
			Console.WriteLine($"Result: {value}");
		}
	}
}

class Program
{
	static void Main()
	{
		Console.WriteLine("Advanced Tabletop Dice Roller!");
		Console.WriteLine("Format: NDx+M (e.g., 3D20+5) or 'q' to quit.");
		Console.WriteLine("Supported dice: D3, D4, D6, D8, D10, D12, D20, D100\n");

		while (true)
		{
			Console.Write("Enter your roll: ");
			string input = Console.ReadLine().Trim().ToUpper();

			if (input == "Q") break;

			var match = Regex.Match(input, @"^(\d+)D(\d+)([+-]\d+)?$");
			if (!match.Success)
			{
				Console.WriteLine("Invalid format. Example: 2D6+1");
				continue;
			}

			int count = int.Parse(match.Groups[1].Value);
			int sides = int.Parse(match.Groups[2].Value);
			int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

			if (!new int[] { 3, 4, 6, 8, 10, 12, 20, 100 }.Contains(sides))
			{
				Console.WriteLine("Unsupported dice type. Use 3, 4, 6, 8, 10, 12, 20, or 100.");
				continue;
			}

			int total = 0;
			Console.WriteLine($"\nRolling {count}D{sides}{(modifier != 0 ? modifier.ToString() : "")}:");

			for (int i = 0; i < count; i++)
			{
				int roll = DiceRoller.Roll(sides);
				total += roll;
				DiceRoller.ShowDiceFace(sides, roll);
			}

			total += modifier;
			Console.WriteLine($"\nTotal (with modifier): {total}");
		}

		Console.WriteLine("Thanks for rolling! Goodbye.");
	}
}
