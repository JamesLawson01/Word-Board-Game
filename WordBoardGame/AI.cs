using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WordBoardGame
{
    public class AI : User
    {
        Random rnd = new();

        public enum Difficulty
        {
            Low,
            Medium,
            High
        }

        private Dictionary<Difficulty, double> difficultyThreshold = new()
        {
            { Difficulty.Low, 5.0E-05 },
            { Difficulty.Medium, 1.2E-06 },
            { Difficulty.High, 0}
        };

        private readonly Dictionary<Difficulty, int> invalidWordChance = new()
        {
            { Difficulty.Low, 5 },
            { Difficulty.Medium, 3 },
            { Difficulty.High, 1 }
        };

        //chance that the AI chooses to challenge a word that is in the dictionary
        private readonly Dictionary<Difficulty, int> challengeValidWordChance = new()
        {
            { Difficulty.Low, 10 },
            { Difficulty.Medium, 5 },
            { Difficulty.High, 0 }
        };

        //chance that the AI chooses to challenge a word that is not in the dictionary
        private readonly Dictionary<Difficulty, int> challengeInvalidWordChance = new()
        {
            { Difficulty.Low, 20 },
            { Difficulty.Medium, 50 },
            { Difficulty.High, 100 }
        };

        internal Difficulty difficulty;

        //private List<Word> temp;

        public AI(string username, List<Tile> tiles, Difficulty difficulty, Label scoreLabel) : base(username, tiles, scoreLabel)
        {
            this.difficulty = difficulty;
            //this.playerType = PlayerType.AI;
        }

        public async Task<List<Tile>> GetTilesToPlaceAsync(List<Tile> previousTiles)
        {
            List<Word> wordLocations = new();

            //gets all words of all lengths with all possible starting coords
            for (int i=0; i<15; i++)
            {
                for (int j=0; j<15; j++)
                {
                    Coord coord = new (i, j);
                    List<Word> iterationWords = new();

                    iterationWords.AddRange(IterateWord(coord with { }, Orientation.Horizontal, previousTiles));
                    iterationWords.AddRange(IterateWord(coord with { }, Orientation.Vertical, previousTiles));

                    //add Word objects that aren't blank to words
                    foreach (Word word in iterationWords)
                    {
                        bool add = false;
                        int numBlanks = 0;
                        foreach (Tile tile in word.word)
                        {
                            if (tile.Letter != ' ' || previousTiles.Count == 0)
                            {
                                add = true;
                            }
                            if (tile.Letter == ' ')
                            {
                                numBlanks++;
                            }
                        }
                        if (add && numBlanks > 0 && numBlanks <= 7)
                        {
                            wordLocations.Add(word);
                        }
                    }
                }
            }

            //removes words that have a placed tile before or after them, as that tile would form part of the word.
            //the word with that tile included is already elsewhere in the words list
            if (previousTiles.Count > 0)
            {
                int t = 0;
                bool removed;
                bool loop = true;
                while (loop)
                {
                    removed = false;
                    Word word = wordLocations[t];
                    List<Tile> boundingTiles = new();
                    Orientation orientation = word.GetOrientation();

                    Coord firstCoord = word.word[0].Coord;
                    Coord lastCoord = word.word.Last().Coord;
                    List<Coord> boundingCoords;

                    //gets the coord immediately before and immediately after the word
                    if (orientation == Orientation.Horizontal)
                    {
                        boundingCoords = new()
                        {
                            new Coord(firstCoord.X - 1, firstCoord.Y),
                            new Coord(lastCoord.X + 1, lastCoord.Y)
                        };
                    }
                    else
                    {
                        boundingCoords = new()
                        {
                            new Coord(firstCoord.X, firstCoord.Y - 1),
                            new Coord(lastCoord.X, lastCoord.Y + 1)
                        };
                    }

                    //removes the word if a tile has been placed at a boundingCoord
                    foreach (Coord boundingCoord in boundingCoords)
                    {
                        if (!boundingCoord.IsOutsideBounds())
                        {
                            Tile boundingTile = previousTiles.Find(tile => tile.Coord == boundingCoord);
                            if (boundingTile is not null)
                            {
                                wordLocations.Remove(word);
                                removed = true;
                                break;
                            }
                        }
                    }

                    //moves the counter along, if the word hasn't been removed
                    if (!removed)
                    {
                        t++;
                    }
                    //reached the end of the words list
                    if (t >= wordLocations.Count)
                    {
                        loop = false;
                    }
                }
            }
            else
            {
                List<Word> tempWordLocations = new();
                foreach (Word word in wordLocations)
                {
                    bool centre = false;
                    foreach (Tile tile in word.word)
                    {
                        if (tile.Coord == new Coord(7, 7))
                        {
                            centre = true;
                            break;
                        }
                    }
                    if (centre)
                    {
                        tempWordLocations.Add(word);
                    }
                }
                wordLocations = tempWordLocations;
            }

            //temp = new();

            //generate all ways of arranging the 7 tiles available to the AI
            List<Word> permutations = new();
            List<Word> combinations = Combinations(new Word(Tiles.ToList()));
            foreach (Word combination in combinations)
            {
                permutations.AddRange(Permutate(combination));

                /*foreach (Word newPermutation in Permutate(combination))
                {
                    //remove duplicate permutations caused by having several letters that are the same
                    bool duplicate = false;
                    foreach (Word permutation in permutations)
                    {
                        if (newPermutation.ToString() == permutation.ToString())
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate)
                    {
                        permutations.Add(newPermutation);
                    }
                }*/
            }
            
            //generate all possible words using parallel processing
            ConcurrentBag<(List<Tile>, int)> parallelWords = new();
            Parallel.ForEach(wordLocations, location =>
            {
                foreach(Word permutation in permutations)
                {
                    Word newWord = location.Clone();
                    //Word tempPermutation = permutation.Clone();
                    //List<Tile> lettersPlaced = new();
                    List<Tile> blanks = newWord.word.FindAll(tile => tile.Letter == ' ');
                    if (permutation.word.Count == blanks.Count)
                    {
                        List<Tile> lettersPlaced = new();
                        int i = 0;
                        int j = 0;
                        foreach (Tile tile in location.word)
                        {
                            if (tile.Letter == ' ')
                            {
                                newWord.word[i] = permutation.word[j];
                                lettersPlaced.Add(new Tile(permutation.word[j].Letter, tile.Coord));
                                //tempPermutation.word.RemoveAt(0);
                                newWord.word[i].Coord = tile.Coord;
                                j++;
                            }
                            i++;
                        }
                        //check word is in dictionary
                        if (newWord.Validate())
                        {
                            List<Word> newWords = Word.GetInterLinkedWords(lettersPlaced, previousTiles);
                            int score = 0;
                            bool invalid = false;
                            int allowedInvalid = 0;
                            foreach (Word tempNewWord in newWords)
                            {
                                int randomNum = rnd.Next(0, 101);
                                if (!tempNewWord.Validate())
                                {
                                    if (randomNum >= invalidWordChance[difficulty] || allowedInvalid >= 1)
                                    {
                                        invalid = true;
                                        break;
                                    }
                                    else
                                    {
                                        allowedInvalid++;
                                        Debug.WriteLine($"{tempNewWord} is allowed to be invalid");
                                    }
                                }
                                score += tempNewWord.Value;
                            }
                            if (!invalid)
                            {
                                parallelWords.Add((lettersPlaced, score));
                            }
                        }
                    }
                }
            });
            if (parallelWords.Count == 0)
            {
                return new List<Tile>();
            }
            List<(List<Tile>, int)> words = parallelWords.ToList();

            Difficulty tempDifficulty = difficulty;
            while (true)
            {
                if (tempDifficulty == Difficulty.High)
                {
                    (List<Tile>, int) bestMove = words[0];

                    foreach ((List<Tile>, int) tuple in words)
                    {
                        if (tuple.Item2 > bestMove.Item2)
                        {
                            bestMove = tuple;
                        }
                    }

                    return bestMove.Item1;
                }
                else
                {
                    List<(List<Tile>, int)> sortedWords = words.OrderByDescending(tuple => tuple.Item2).ToList();
                    bool valid = false;
                    int i = 0;

                    while (!valid)
                    {
                        List<Word> interlinkedWords = Word.GetInterLinkedWords(sortedWords[i].Item1, previousTiles);
                        List<float> popularities = new();
                        foreach (Word interlinkedWord in interlinkedWords)
                        {
                            popularities.Add(await interlinkedWord.GetPopularity());
                        }

                        float minPopularity = popularities.Min();
                        if (minPopularity > difficultyThreshold[tempDifficulty])
                        {
                            valid = true;
                            return sortedWords[i].Item1;
                        }
                        else
                        {
                            Debug.WriteLine(interlinkedWords[0] + " is not popular enough");
                        }
                        i++;
                        if (i > sortedWords.Count - 1)
                        {
                            break;
                        }
                    }

                    if (tempDifficulty == Difficulty.Low)
                    {
                        tempDifficulty = Difficulty.Medium;
                    }
                    else
                    {
                        tempDifficulty = Difficulty.High;
                    }
                }
            }
        }

        private List<Word> IterateWord(Coord coord, Orientation direction, List<Tile> previousTiles)
        {
            //get starting tile
            Tile startTile = previousTiles.Find(tile => tile.Coord == coord);
            if (startTile is null)
            {
                startTile = new(' ', coord with { });
            }

            Word word = new(startTile);
            List<Word> returnWords = new();
            bool loop = true;

            while (loop)
            {
                //move to next coord
                coord = IterateCoord(coord, direction);
                if (coord.IsOutsideBounds())
                {
                    loop = false;
                }
                else
                {
                    //convert coord into tile
                    Tile tile = previousTiles.Find(tile => tile.Coord == coord);
                    if (tile is null)
                    {
                        tile = new(' ', coord with { });
                    }

                    //add tile to word and add a copy of word to list of words
                    word.AppendWord(tile);
                    if (word.word.Count > 1)
                    {
                        returnWords.Add(word.Clone());
                    }
                }
            }
            return returnWords;
        }

        private Coord IterateCoord(Coord coord, Orientation direction)
        {
            if (direction == Orientation.Horizontal)
            {
                coord.X += 1;
            }
            else
            {
                coord.Y += 1;
            }
            return coord;
        }

        private List<Word> Combinations(Word input)
        {
            int n = input.word.Count;
            // Power set contains 2^N subsets.
            int powerSetCount = 1 << n;
            List<Word> output = new();

            for (int setMask = 0; setMask < powerSetCount; setMask++)
            {
                List<Tile> tiles = new();
                for (int i = 0; i < n; i++)
                {
                    // Checking whether i'th element of input collection should go to the current subset.
                    if ((setMask & (1 << i)) > 0)
                    {
                        tiles.Add(input.word[i]);
                    }
                }
                if (tiles.Count > 0)
                {
                    output.Add(new Word(tiles));
                }
            }
            return output;
        }

        private IEnumerable<Word> Permutate(Word input)
        {
            if (input.word.Count == 1)
            {
                yield return input;
            }
            /*if (input.word.Count == 2) //base
            {
                yield return input;
                yield return new Word(new List<Tile>() { input.word[1], input.word[0] } );
            }*/
            else
            {
                foreach (Tile tile in input.word)
                {
                    //create sub-word
                    var subWord = new Word (new List<Tile>(input.word));
                    subWord.word.Remove(tile);

                    //find permutations of sub-word
                    foreach (Word word in Permutate(subWord))
                    {
                        //temp.Add(word);
                        //yield return new Word(new List<Tile>(word.word));
                        
                        //get permutations of original word
                        word.word.Insert(0, tile);
                        yield return word;
                    }
                }
            }
        }

        //choose a word to challenge
        public Word ChooseChallengeWord(List<Word> words)
        {
            List<Word> ChallengeWords = new();

            foreach (Word word in words)
            {
                int rndNum = rnd.Next(0, 100);
                if (word.Validate())
                {
                    if (rndNum < challengeValidWordChance[difficulty])
                    {
                        ChallengeWords.Add(word);
                    }
                }
                else
                {
                    if (rndNum < challengeInvalidWordChance[difficulty])
                    {
                        ChallengeWords.Add(word);
                    }
                }
            }

            if (ChallengeWords.Count > 0)
            {
                return ChallengeWords[rnd.Next(0, ChallengeWords.Count)];
            }
            else
            {
                return null;
            }

        }
    }
}
