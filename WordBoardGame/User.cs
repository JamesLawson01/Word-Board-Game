using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace WordBoardGame
{
    public class User
    {
        //words the user has played
        private List<Word> words;
        public List<Word> Words { get { return words; } }

        //user's name
        public string Name { get; }

        //tiles the user can play
        private List<Tile> tiles;
        public List<Tile> Tiles { get { return tiles; } }

        private List<Tile> previousTiles;
        private List<Tile> newlyAddedTiles;

        public bool IsOutOfTiles { get { return Tiles.Count == 0; } }

        private int numTurns;
        public int NumTurns { get { return numTurns; } }

        private int score;
        public int Score { get { return score; } }

        private int previousScore;
        private List<Word> previousWords;

        public Label scoreLabel;

        public User(string username, List<Tile> tiles, Label scoreLabel)
        {
            Name = username;

            if (tiles.Count != 7)
            {
                throw new ArgumentException(paramName: nameof(tiles), message: "Must be a list of length 7");
            }

            words = new();
            this.tiles = tiles;
            this.scoreLabel = scoreLabel;
        }

        /// <summary>
        /// Add a word to the user's list of played words
        /// </summary>
        /// <param name="word"></param>
        public void AddWord(Word word)
        {
            Words.Add(word);
            score += word.Value;
            scoreLabel.Content = score;
        }

        public void IncrementTurns()
        {
            numTurns++;
            //scoreLabel.Content = score;

            previousTiles = new();
            previousTiles.AddRange(Tiles);
            previousWords = new();
            previousWords.AddRange(Words);
        }

        //removes tiles that have been placed and adds new ones
        public void ChangeTiles(List<Tile> oldTiles, List<Tile> newTiles)
        {
            foreach (Tile oldTile in oldTiles)
            {
                Tiles.Remove(Tiles.Find(tile => tile.Letter == oldTile.Letter));
            }

            Tiles.AddRange(newTiles);
            newlyAddedTiles = newTiles;
        }

       /* public List<Tile> RemoveWord(Word word)//not correct
        {
            Words.Remove(word);
            score = previousScore;

            List<Tile> returnTiles = new();
            foreach (Tile tile in tiles)
            {
                returnTiles.Add(tile);
            }

            foreach (Tile tile in previouseTiles)
            {
                tiles.Add(tile);
            }
            return returnTiles;
        }*/


        public void SaveScore()
        {
            previousScore = score;
        }

        //revert to previous state after being successfully challenged
        public List<Tile> Revert()
        {
            tiles.Clear();
            tiles.AddRange(previousTiles);
            words.Clear();
            words.AddRange(previousWords);

            score = previousScore;
            scoreLabel.Content = score;

            return newlyAddedTiles;
        }

    }
}
