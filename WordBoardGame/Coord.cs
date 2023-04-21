using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WordBoardGame
{
    public record Coord
    {
        private static readonly Dictionary<Coord, Bonus> bonusLocations = new()
        {
            { new Coord(0, 0), Bonus.TripleWord },
            { new Coord(7, 0), Bonus.TripleWord },
            { new Coord(14, 0), Bonus.TripleWord },
            { new Coord(0, 7), Bonus.TripleWord },
            { new Coord(14, 7), Bonus.TripleWord },
            { new Coord(0, 14), Bonus.TripleWord },
            { new Coord(7, 14), Bonus.TripleWord },
            { new Coord(14, 14), Bonus.TripleWord },

            { new Coord(1, 1), Bonus.DoubleWord },
            { new Coord(2, 2), Bonus.DoubleWord },
            { new Coord(3, 3), Bonus.DoubleWord },
            { new Coord(4, 4), Bonus.DoubleWord },
            { new Coord(1, 13), Bonus.DoubleWord },
            { new Coord(2, 12), Bonus.DoubleWord },
            { new Coord(3, 11), Bonus.DoubleWord },
            { new Coord(4, 10), Bonus.DoubleWord },
            { new Coord(13, 1), Bonus.DoubleWord },
            { new Coord(12, 2), Bonus.DoubleWord },
            { new Coord(11, 3), Bonus.DoubleWord },
            { new Coord(10, 4), Bonus.DoubleWord },
            { new Coord(13, 13), Bonus.DoubleWord },
            { new Coord(12, 12), Bonus.DoubleWord },
            { new Coord(11, 11), Bonus.DoubleWord },
            { new Coord(10, 10), Bonus.DoubleWord },

            { new Coord(5, 1), Bonus.TripleLetter },
            { new Coord(9, 1), Bonus.TripleLetter },
            { new Coord(1, 5), Bonus.TripleLetter },
            { new Coord(5, 5), Bonus.TripleLetter },
            { new Coord(9, 5), Bonus.TripleLetter },
            { new Coord(13, 5), Bonus.TripleLetter },
            { new Coord(1, 9), Bonus.TripleLetter },
            { new Coord(5, 9), Bonus.TripleLetter },
            { new Coord(9, 9), Bonus.TripleLetter },
            { new Coord(13, 9), Bonus.TripleLetter },
            { new Coord(5, 13), Bonus.TripleLetter },
            { new Coord(9, 13), Bonus.TripleLetter },

            { new Coord(3, 0), Bonus.DoubleLetter },
            { new Coord(11, 0), Bonus.DoubleLetter },
            { new Coord(6, 2), Bonus.DoubleLetter },
            { new Coord(8, 2), Bonus.DoubleLetter },
            { new Coord(0, 3), Bonus.DoubleLetter },
            { new Coord(7, 3), Bonus.DoubleLetter },
            { new Coord(14, 3), Bonus.DoubleLetter },
            { new Coord(2, 6), Bonus.DoubleLetter },
            { new Coord(6, 6), Bonus.DoubleLetter },
            { new Coord(8, 6), Bonus.DoubleLetter },
            { new Coord(12, 6), Bonus.DoubleLetter },
            { new Coord(3, 7), Bonus.DoubleLetter },
            { new Coord(11, 7), Bonus.DoubleLetter },
            { new Coord(2, 8), Bonus.DoubleLetter },
            { new Coord(6, 8), Bonus.DoubleLetter },
            { new Coord(8, 8), Bonus.DoubleLetter },
            { new Coord(12, 8), Bonus.DoubleLetter },
            { new Coord(0, 11), Bonus.DoubleLetter },
            { new Coord(7, 11), Bonus.DoubleLetter },
            { new Coord(14, 11), Bonus.DoubleLetter },
            { new Coord(6, 12), Bonus.DoubleLetter },
            { new Coord(8, 12), Bonus.DoubleLetter },
            { new Coord(3, 14), Bonus.DoubleLetter },
            { new Coord(11, 14), Bonus.DoubleLetter },
        };

        public Bonus Bonus
        { 
            get
            {
                bool hasBonus = bonusLocations.TryGetValue(this, out Bonus bonus);
                if (hasBonus)
                {
                    return bonus;
                }
                else
                {
                    return Bonus.None;
                }
            }
        }

        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type

        /// <summary>
        /// Provides a string representation of the Coord object, used for debugging purposes
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return base.ToString() + ": (" + X + ", " + Y + ")";
        }

        /// <summary>
        /// Returns the 4 Coords that border this object
        /// </summary>
        /// <returns>A list of Coords</returns>
        public List<Coord> GetSurroundingCoords()
        {
            List<Coord> surroundingCoords = new();
            if (X > 0)
            {
                surroundingCoords.Add(new Coord(X - 1, Y));
            }
            if (X < 14)
            {
                surroundingCoords.Add(new Coord(X + 1, Y));
            }
            if (Y > 0)
            {
                surroundingCoords.Add(new Coord(X, Y - 1));
            }
            if (Y < 14)
            {
                surroundingCoords.Add(new Coord(X, Y + 1));
            }
            return surroundingCoords;
        }

        /// <summary>
        /// Generates a random Coord
        /// </summary>
        /// <returns>A random Coord</returns>
        public static Coord RandomCoord()
        {
            Random rnd = new();
            Coord coord = new(rnd.Next(0, 14), rnd.Next(0, 14));
            return coord;
        }

        public bool IsOutsideBounds()
        {
            return (X < 0 || X > 14 || Y < 0 || Y > 14);
        }

        public void RemoveBonus()
        {
            try
            {
                bonusLocations.Remove(this);
            }
            catch
            {
                throw;
            }
        }

    }
}
