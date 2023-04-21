using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WordBoardGame
{
    #nullable enable
    public class Tile
    {
        private static readonly Dictionary<char, int> letterValues = new()
        {
            { 'A', 1 },
            { 'B', 3 },
            { 'C', 3 },
            { 'D', 2 },
            { 'E', 1 },
            { 'F', 4 },
            { 'G', 2 },
            { 'H', 4 },
            { 'I', 1 },
            { 'J', 8 },
            { 'K', 5 },
            { 'L', 1 },
            { 'M', 3 },
            { 'N', 1 },
            { 'O', 1 },
            { 'P', 3 },
            { 'Q', 10 },
            { 'R', 1 },
            { 'S', 1 },
            { 'T', 1 },
            { 'U', 1 },
            { 'V', 4 },
            { 'W', 4 },
            { 'X', 8 },
            { 'Y', 4 },
            { 'Z', 10 }
        };

        public char Letter { get; }

        // The ? signifies that coord should be nullable
        public Coord? Coord { get; set; }

        //private Bonus letterBonus;
        public Bonus LetterBonus
        {
            get
            {
                if (Coord is not null)
                {
                    return Coord.Bonus;
                }
                else
                {
                    throw new InvalidOperationException(message: "Coord is null");
                }
            }
        }

        public BitmapImage Image
        {
            get
            {
                return new BitmapImage(new Uri($"pack://application:,,,/letters/{Letter}.png", UriKind.Absolute));
            }
        }

        public int GetValue
        {
            get
            {
                int score = letterValues[Letter];

                if (LetterBonus == Bonus.DoubleLetter)
                {
                    score *= 2;
                }
                else if (LetterBonus == Bonus.TripleLetter)
                {
                    score *= 3;
                }
                /*else if (LetterBonus == Bonus.DoubleWord || LetterBonus == Bonus.TripleWord)
                {
                    throw new ArgumentOutOfRangeException(paramName: nameof(LetterBonus));
                }*/

                return score;
            }
        }

        public Tile(char letter, Coord coord)
        {
            Letter = letter;
            Coord = coord;
        }

        public Tile(char letter)
        {
            Letter = letter;
            Coord = null;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Letter + ", " + Coord + ", " + LetterBonus;
        }

        public Tile Clone()
        {
            Tile newTile;
            if (Coord is null)
            {
                newTile = new Tile(Letter);
            }
            else
            {
                newTile = new Tile(Letter, Coord with { });
            }
            //newTile.letterBonus = letterBonus;
            return newTile;
        }

    }
}
