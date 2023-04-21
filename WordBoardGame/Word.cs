using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WordBoardGame
{
    public class Word
    {
        private const string sowpodsLocation = @"Sowpods.txt";

        private static HashSet<string> sowpods;

        private static readonly HttpClient client = new();

        public List<Tile> word { get; }

        public int WordMultiplier
        {
            get
            {
                int multiplier = 1;
                foreach (Tile tile in word)
                {
                    if (tile.LetterBonus == Bonus.DoubleWord)
                    {
                        multiplier *= 2;
                    }
                    else if (tile.LetterBonus == Bonus.TripleWord)
                    {
                        multiplier *= 3;
                    }
                }
                return multiplier;
            }
        }

        public int Value
        {
            get
            {
                int score = 0;
                foreach (Tile tile in word)
                {
                    score += tile.GetValue;
                }
                score *= WordMultiplier;
                return score;

            }
        }

        static Word()
        {
            InitSowpods();
        }

        public Word(List<Tile> word)
        {
            this.word = word;
        }

        public Word(Tile tile)
        {
            this.word = new() { tile };
        }

        public Word Clone()
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Tile tile in this.word)
            {
                tiles.Add(tile.Clone());
            }
            Word word =  new(tiles);
            //word.WordBonus = WordBonus;
            return word;
        }

        public bool Validate()
        {
            return sowpods.Contains(ToString());
        }

        private static void InitSowpods()
        {
            sowpods = new HashSet<string>();
            StreamReader sr = new StreamReader(sowpodsLocation);
            while (sr.Peek() != -1)
            {
                sowpods.Add(sr.ReadLine());
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (Tile tile in word)
            {
                str += tile.Letter;
            }
            return str.ToLower();
        }

        //returns the way a group of tiles are oriented
        public Orientation GetOrientation()
        {
            return GetOrientation(word);
        }

        private static Orientation GetOrientation(List<Tile> tiles)
        {
            if (tiles.Count < 2)
            {
                // one or zero tiles placed, so orientation doesn't matter
                return Orientation.Horizontal;
            }
            else if (tiles[0].Coord.X == tiles[1].Coord.X)
            {
                //all x values should be the same
                return Orientation.Vertical;
            }
            else if (tiles[0].Coord.Y == tiles[1].Coord.Y)
            {
                //all y values should be the same
                return Orientation.Horizontal;
            }
            else
            {
                throw new ArgumentException(message: $"The contents of {nameof(tiles)} are arranged in an invalid way.", paramName: nameof(tiles));
            }
        }

        public void AppendWord(Tile tile)
        {
            word.Add(tile);
        }

        public void PrependWord(Tile tile)
        {
            word.Insert(0, tile);
        }

        public async Task<float> GetPopularity()
        {
            string url = $"https://books.google.com/ngrams/json?content={ToString()}&year_start=2018&year_end=2019&corpus=26&smoothing=0";

            client.DefaultRequestHeaders.Accept.Clear();

            Task<Stream> streamTask = Request(url);
            List<Ngram> ngrams;
            try
            {
                ngrams = await JsonSerializer.DeserializeAsync<List<Ngram>>(await streamTask);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }

            if (ngrams.Count == 0)
            {
                return 0.0F;
            }

            Debug.WriteLine(ngrams[0].Timeseries[1].ToString());
            return ngrams[0].Timeseries[1];
        }

        private async Task<Stream> Request(string url)
        {
            try
            {
                return await client.GetStreamAsync(url);
            }
            catch (HttpRequestException)
            {
                Thread.Sleep(2000);
                Debug.WriteLine("Waiting for Google");
                return await Request(url);
            }
        }

        /*private static async Task<Ngram> RequestApiData(string url)
        {
            Debug.WriteLine(url);
            client.DefaultRequestHeaders.Accept.Clear();

            var streamTask = client.GetStreamAsync(url);
            List<Ngram> ngrams = new();
            try
            {
                ngrams = await JsonSerializer.DeserializeAsync<List<Ngram>>(await streamTask);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }

            foreach (var ngram in ngrams)
            {
                Debug.WriteLine(ngram.Timeseries[1].ToString());
            }
            return ngrams[0];

        }*/





        //Ensures letters are placed in valid locations
        public static bool CheckLetterLocation(List<Tile> tiles, List<Tile> previousTiles)
        {
            if (tiles.Count == 0)
            {
                return true;
            }


            if (previousTiles.Count == 0)
            {
                bool centre = false;
                foreach (Tile tile in tiles)
                {
                    if (tile.Coord == new Coord(7, 7))
                    {
                        centre = true;
                        break;
                    }
                }
                if (!centre)
                {
                    return false;
                }
            }

            Orientation orientation;
            int same;
            orientation = GetOrientation(tiles);
            if (orientation == Orientation.Vertical)
            {
                //all x values should be the same
                same = tiles[0].Coord.X;
                foreach (Tile tile in tiles)
                {
                    if (tile.Coord.X != same)
                    {
                        //Not all in a line, so arrangement not valid
                        return false;
                    }
                }
            }
            else if (orientation == Orientation.Horizontal)
            {
                //all y values should be the same
                same = tiles[0].Coord.Y;
                foreach (Tile tile in tiles)
                {
                    if (tile.Coord.Y != same)
                    {
                        //not all in a line, so arrangement not valid.
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            //Check tiles are adjacent
            List<Tile> sortedTiles = SortTiles(tiles, orientation);
            List<Tile> interlinkingTiles = GetInterlinkedTiles(tiles, previousTiles);

            Word tempWord = IterateWord(sortedTiles[0].Coord with { }, orientation, previousTiles, tiles);

            bool tryBool = sortedTiles.All(letter => tempWord.word.Contains(letter)) && (interlinkingTiles.Count >= 1 || previousTiles.Count == 0);
            return tryBool;
        }

        //Sorts tiles vertically downwards or across to the right
        private static List<Tile> SortTiles(List<Tile> tiles, Orientation orientation)
        {
            List<Tile> sortedTiles;
            if (orientation == Orientation.Horizontal)
            {
                sortedTiles = tiles.OrderBy(tile => tile.Coord.X).ToList();
            }
            else
            {
                sortedTiles = tiles.OrderBy(tile => tile.Coord.Y).ToList();
            }
            return sortedTiles;
        }

        //returns all the Tiles that are directly connected to a letter placed on the current turn
        private static List<Tile> GetInterlinkedTiles(List<Tile> checkTiles, List<Tile> previousTiles)
        {
            //gets relevent coords, to make it easier to check adjecency
            Orientation orientation = GetOrientation(checkTiles);
            HashSet<Coord> checkCoords = new();
            HashSet<Coord> previousCoords = new();
            foreach (Tile tile in previousTiles)
            {
                previousCoords.Add(tile.Coord);
            }
            foreach (Tile tile in checkTiles)
            {
                checkCoords.Add(tile.Coord);
            }

            //gets all adjecent coords
            List<Coord> adjecentCoords = new List<Coord>();
            foreach (Coord coord in checkCoords)
            {
                List<Coord> surroundingCoords = coord.GetSurroundingCoords();
                foreach (Coord surroundingCoord in surroundingCoords)
                {
                    if (!checkCoords.Contains(surroundingCoord))
                    {
                        adjecentCoords.Add(surroundingCoord);
                    }
                }
            }

            //filters out coords where a letter hasn't been placed
            List<Coord> interlinkingCoords = new();
            foreach (Coord coord in adjecentCoords)
            {
                if (previousCoords.Contains(coord))
                {
                    interlinkingCoords.Add(coord);
                }
            }

            //converts the coords back to tiles
            List<Tile> interlinkingTiles = new();
            foreach (Coord coord in interlinkingCoords)
            {
                interlinkingTiles.Add(previousTiles.Find(tile => tile.Coord == coord));
            }

            //removes duplicates
            return interlinkingTiles.Distinct().ToList();
        }

        //iterates forwards and backwards in a given direction, starting from a given coord, finding all letters to make a word
        private static Word IterateWord(Coord coord, Orientation direction, List<Tile> previousTiles, List<Tile> currentTiles)
        {
            Coord startCoord = coord with { };  // store starting point for use when going backwards
            Word returnWord = new(new List<Tile>());

            for (int change = 1; change >= -1; change -= 2)
            {
                if (change == 1) //forwards
                {
                    // start tile
                    returnWord = new(new List<Tile>() { currentTiles.Find(tile => tile.Coord == coord) });
                    if (returnWord.word[0] is null)
                    {
                        returnWord = new(new List<Tile>() { previousTiles.Find(tile => tile.Coord == coord) });
                    }
                }
                else if (change == -1) //backwards
                {
                    coord = startCoord;
                }

                bool loop = true;
                while (loop)
                {
                    if (direction == Orientation.Horizontal)
                    {
                        coord.X += change;
                    }
                    else
                    {
                        coord.Y += change;
                    }

                    //try and get the next tile along
                    Tile nextTile = previousTiles.Find(nextTile => nextTile.Coord == coord);
                    if (nextTile is null)
                    {
                        nextTile = currentTiles.Find(tile => tile.Coord == coord);
                    }

                    //add tile to word
                    if (nextTile is not null)
                    {
                        if (change == 1)
                        {
                            returnWord.AppendWord(nextTile);
                        }
                        else if (change == -1)
                        {
                            returnWord.PrependWord(nextTile);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(paramName: nameof(change), message: "Only 1 or -1 is allowed");
                        }
                    }
                    else //end of word reached
                    {
                        loop = false;
                    }
                }
            }
            return returnWord;
        }

        //Returns all the words created on a turn
        public static List<Word> GetInterLinkedWords(List<Tile> checkTiles, List<Tile> previousTiles)
        {
            if (checkTiles.Count == 0)
            {
                return new List<Word>();
            }
            else if (previousTiles.Count == 0)
            {
                return new List<Word>() { new Word(Word.SortTiles(checkTiles, GetOrientation(checkTiles))) };
            }

            Orientation orientation = GetOrientation(checkTiles);
            List<Tile> sortedTiles = SortTiles(checkTiles, orientation);
            List<Tile> interlinkingTiles = GetInterlinkedTiles(checkTiles, previousTiles);
            bool doneMainWord = false;

            List<Word> words = new();

            // first turn
            if (interlinkingTiles.Count == 0 && sortedTiles.Count >= 2)
            {
                words.Add(new Word(sortedTiles));
                doneMainWord = true;
            }
            foreach (Tile tile in interlinkingTiles)
            {
                Word word;
                bool duplicate = false;

                //Tile is part of main word
                if ((orientation == Orientation.Vertical && tile.Coord.X == checkTiles[0].Coord.X) || (orientation == Orientation.Horizontal && tile.Coord.Y == checkTiles[0].Coord.Y))
                {
                    if (!doneMainWord)
                    {
                        Coord coord = checkTiles[0].Coord with { };
                        word = IterateWord(coord, orientation, previousTiles, checkTiles);
                        doneMainWord = true;
                    }
                    else
                    {
                        duplicate = true;
                        word = null;
                    }
                }
                //Tile forms a new word by crossing over the created word
                else
                {
                    Coord coord = tile.Coord with { };
                    Orientation oppositeOrientation;
                    if (orientation == Orientation.Vertical)
                    {
                        oppositeOrientation = Orientation.Horizontal;
                    }
                    else
                    {
                        oppositeOrientation = Orientation.Vertical;
                    }
                    word = IterateWord(coord, oppositeOrientation, previousTiles, checkTiles);

                    //check for duplicates
                    foreach (Word listWord in words)
                    {
                        if (listWord.ToString() == word.ToString() && listWord.word[0].Coord == word.word[0].Coord && listWord.GetOrientation() == word.GetOrientation())
                        {
                            duplicate = true;
                        }
                        
                    }
                }

                if (!duplicate && word.word.Count >= 2)
                {
                    words.Add(word);
                }
            }
            //no interlinking tiles are needed to create main word
            if (!doneMainWord && sortedTiles.Count >= 2)
            {
                words.Insert(0, new(sortedTiles));
            }
            return words;
        }

    }
}
