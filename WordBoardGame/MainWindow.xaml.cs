using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WordBoardGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Used when generating the Tiles. The space is used for the blank tiles
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";// ";

        private const string uriPrefix = "pack://application:,,,/";

        private static readonly Dictionary<char, int> letterNums = new()
        {
            { 'A', 9 },
            { 'B', 2 },
            { 'C', 2 },
            { 'D', 4 },
            { 'E', 12 },
            { 'F', 2 },
            { 'G', 3 },
            { 'H', 2 },
            { 'I', 9 },
            { 'J', 1 },
            { 'K', 1 },
            { 'L', 4 },
            { 'M', 2 },
            { 'N', 6 },
            { 'O', 8 },
            { 'P', 2 },
            { 'Q', 1 },
            { 'R', 6 },
            { 'S', 4 },
            { 'T', 6 },
            { 'U', 4 },
            { 'V', 2 },
            { 'W', 2 },
            { 'X', 1 },
            { 'Y', 2 },
            { 'Z', 1 },
            { ' ', 2 },
        };

        private List<Tile> letterPool = new();

        private List<Tile> playedTiles = new();

        private bool deleteTiles = false;

        private readonly List<User> players;

        private User currentPlayer;

        private readonly BitmapImage nullImage = new(new Uri($"{uriPrefix}Blank Image.png", UriKind.Absolute));

        private List<Tile> turnTiles = new();

        private List<Tile> previousTurnTiles = new();

        private bool gameOver = false;

        private int timeElapsed;

        private List<(Button, Word, StackPanel)> possibleChallengeWords = new();

        public MainWindow()
        {
            InitializeComponent();

            //Generate gameboard
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    Coord coord = new(i, j);

                    Border border = new();
                    border.BorderBrush = Brushes.White;
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                    border.CornerRadius = new CornerRadius(5);
                    //border.AllowDrop = true;
                    //border.Drop = new DragEventHandler()

                    Image image = new();
                    image.Margin = new Thickness(0);
                    image.Stretch = Stretch.Uniform;
                    image.AllowDrop = true;
                    image.Drop += DropLetter;
                    

                    Viewbox viewbox = new();
                    viewbox.Stretch = Stretch.Uniform;

                    //check for bonuses
                    image.Source = GetBonusImageFromCoord(coord);
                    border.Background = GetBonusColourFromCoord(coord);

                    viewbox.Child = image;
                    border.Child = viewbox;
                    playGrid.Children.Add(border);
                }
            }

            //generate letters
            foreach (char letter in alphabet)
            {
                int numToCreate = letterNums[letter];
                for (int j=0; j<numToCreate; j++)
                {
                    letterPool.Add(new Tile(letter));
                }
            }

            letterPool = Shuffle(letterPool);

            //Create the player and AI
            players = new();
            players.Add(new User("Player", letterPool.GetRange(0, 7), userScoreLabel));
            letterPool.RemoveRange(0, 7);
            players.Add(new AI("Computer", letterPool.GetRange(0, 7), AI.Difficulty.Medium, computerScoreLabel));
            letterPool.RemoveRange(0, 7);
            currentPlayer = players[0];

            //display the user's tiles on the screen
            AddTilesToDock(currentPlayer);

            //sets the timer going
            DispatcherTimer timer = new(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += IncrementTimer;
            timer.Start();
        }

        private void IncrementTimer(object sender, EventArgs e)
        {
            timeLabel.Content = new TimeSpan(0, 0, timeElapsed).ToString();
            timeElapsed += 1;
        }

        //add user's tiles to the tile dock
        private void AddTilesToDock(User user)
        {
            tileDock.Children.Clear();
            foreach (Tile tile in user.Tiles)
            {
                char letter = tile.Letter;
                Image image = new();
                image.Source = tile.Image;
                //image.AllowDrop = true;
                image.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragLetter);
                tileDock.Children.Add(image);
            }
        }

        /*private BitmapImage GetImageSourceFromLetter(char letter)
        {
            return new BitmapImage(new Uri($"{uriPrefix}letters/{letter}.png", UriKind.Absolute));
        }*/

        private BitmapImage GetBonusImageFromCoord(Coord coord)
        {
            if (coord.X == 7 && coord.Y == 7)
            {
                return new BitmapImage(new Uri($"{uriPrefix}Star.png", UriKind.Absolute));
            }

            Bonus bonus = coord.Bonus;
            return bonus switch
            {
                Bonus.TripleWord => new BitmapImage(new Uri($"{uriPrefix}3x word.png", UriKind.Absolute)),
                Bonus.DoubleWord => new BitmapImage(new Uri($"{uriPrefix}2x word.png", UriKind.Absolute)),
                Bonus.TripleLetter => new BitmapImage(new Uri($"{uriPrefix}3x letter.png", UriKind.Absolute)),
                Bonus.DoubleLetter => new BitmapImage(new Uri($"{uriPrefix}2x letter.png", UriKind.Absolute)),
                _ => nullImage,
            };

            /*if (bonusLocations.TryGetValue(coord, out Bonus bonus))
            {
                return bonus switch
                {
                    Bonus.TripleWord => new BitmapImage(new Uri($"{uriPrefix}3x word.png", UriKind.Absolute)),
                    Bonus.DoubleWord => new BitmapImage(new Uri($"{uriPrefix}2x word.png", UriKind.Absolute)),
                    Bonus.TripleLetter => new BitmapImage(new Uri($"{uriPrefix}3x letter.png", UriKind.Absolute)),
                    Bonus.DoubleLetter => new BitmapImage(new Uri($"{uriPrefix}2x letter.png", UriKind.Absolute)),
                    _ => nullImage,
                };
            }
            else
            {
                return nullImage;
            }*/
        }

        private SolidColorBrush GetBonusColourFromCoord(Coord coord)
        {
            Bonus bonus = coord.Bonus;

            return bonus switch
            {
                Bonus.TripleWord => Brushes.Tomato,
                Bonus.DoubleWord => Brushes.LightPink,
                Bonus.TripleLetter => Brushes.CornflowerBlue,
                Bonus.DoubleLetter => Brushes.LightBlue,
                _ => new SolidColorBrush(Color.FromRgb(0x79, 0x97, 0xB9)),
            };

            /*if (bonusLocations.TryGetValue(coord, out Bonus bonus))
            {
                return bonus switch
                {
                    Bonus.TripleWord => Brushes.Red,
                    Bonus.DoubleWord => Brushes.LightPink,
                    Bonus.TripleLetter => Brushes.DarkBlue,
                    Bonus.DoubleLetter => Brushes.LightBlue,
                    _ => null,
                };
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(0x79, 0x97, 0xB9));
            }*/
        }

        /*private void GiveDragCursorFeedback(object sender, GiveFeedbackEventArgs e)
        {
            //canvas.Children.Clear();
            //Image image = new Image((Image)sender).Source);
            Image image = new();
            image.Source = ((Image)sender).Source;
            Point point = Mouse.GetPosition(canvas);
            Canvas.SetLeft(image, point.X);
            Canvas.SetTop(image, point.Y);
            canvas.Children.Add(image);
            Debug.WriteLine("fired");
        }*/

        //start drag
        private void DragLetter(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            DataObject data = new();
            data.SetData(image.Source);

            ImageSource originalImage = image.Source;
            image.AllowDrop = true;

            //GiveFeedbackEventHandler handler = new(GiveDragCursorFeedback);
            //image.GiveFeedback += handler;

            //drag from tile dock
            if (image.Parent == tileDock)
            {
                tileDock.Children.Remove(image);
                if (DragDrop.DoDragDrop(image, data, DragDropEffects.Move) == DragDropEffects.None)
                {
                    //drag operation failed
                    tileDock.Children.Add(image);
                }
                else
                {
                    data.SetData(true);
                }
            }
            else    //drag from gameboard
            {
                Tile tile = GetTileFromImage(image);
                image.Source = nullImage;
                Debug.WriteLine($"col: {Grid.GetColumn(image)} row: {Grid.GetRow(image)}");
                if (DragDrop.DoDragDrop(image, data, DragDropEffects.Move) == DragDropEffects.None)
                {
                    //drag operation failed
                    image.Source = originalImage;
                    image.AllowDrop = false;
                }
                else
                {
                    image.PreviewMouseLeftButtonDown -= DragLetter;
                    foreach (Tile loopTile in turnTiles)
                    {
                        if (loopTile.Coord == GetGridCoord(image))
                        {
                            turnTiles.Remove(loopTile);
                            break;
                        }
                    }
                }
            }

            //image.GiveFeedback -= handler;
        }

        //end drag
        private void DropLetter(object sender, DragEventArgs e)
        {
            Image image = (Image)sender;
            image.Source = (ImageSource)e.Data.GetData("System.Windows.Media.Imaging.BitmapImage");

            //image.PreviewMouseLeftButtonDown -= DragLetter;
            image.PreviewMouseLeftButtonDown += DragLetter;

            image.AllowDrop = false;

            //match dragged letter to tile
            Tile tile = GetTileFromImage(image);
            turnTiles.Add(tile);

            DeleteTileImage.AllowDrop = false;
        }

        //Gets the letter represented by an Image
        private char GetLetterFromImage(Image image)
        {
            ImageSource imageSource = image.Source;
            Uri imageUri = ((BitmapImage)imageSource).UriSource;
            string uriString = imageUri.ToString();
            string checkString = $"{uriPrefix}letters/ .png";

            //validate that the image is of a letter
            if (uriString.StartsWith($"{uriPrefix}letters/") && uriString.EndsWith(".png") && uriString.Length == checkString.Length)
            {
                return uriString[uriPrefix.Length + "letters.".Length];
            }
            else
            {
                throw new ArgumentException(message: $"{nameof(image)} does not represent a letter");
            }
        }

        //Gets the location of an image on the gameboard
        private Coord GetGridCoord(Image matchImage)
        {
            int columns = playGrid.Columns;
            int index;

            if (matchImage.Parent is Viewbox viewbox)
            {
                index = playGrid.Children.IndexOf((Border)viewbox.Parent);
            }
            else
            {
                return null;
            }

            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(matchImage), message: "Image not found on the gameboard");
            }

            int row = index / columns;
            int column = index % columns;
            Debug.WriteLine($"Found at ({column}, {row})");
            return new Coord(column, row);
        }

        //returns the image at a given coord
        private Image GetImageFromCoord(Coord coord)
        {
            int index = coord.Y * 15 + coord.X;
            Border border = (Border)playGrid.Children[index];
            Viewbox viewbox = (Viewbox)border.Child;
            return (Image)viewbox.Child;
        }

        //Generates a Tile from an image
        private Tile GetTileFromImage(Image image)
        {
            return new Tile(GetLetterFromImage(image), GetGridCoord(image));
        }

        //Called when the button to finish a turn is clicked
        private async void FinishTurnAsync(object sender, RoutedEventArgs e)
        {
            if (deleteTiles || Word.CheckLetterLocation(turnTiles, playedTiles))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                playGrid.IsEnabled = false;
                tileDock.IsEnabled = false;
                controlButtons.IsEnabled = false;
                difficultyComboBox.IsEnabled = false;

                Debug.WriteLine("button pressed");
                await FinishTurnAsync();

                difficultyComboBox.IsEnabled = true;
                controlButtons.IsEnabled = true;
                tileDock.IsEnabled = true;
                playGrid.IsEnabled = true;
                Mouse.OverrideCursor = null;

                DeleteTileImage.AllowDrop = true;
            }
        }

        private async Task FinishTurnAsync()
        {
            if (currentPlayer is AI ai)
            {
                List<Word> challengeWords = new();
                foreach ((Button, Word, StackPanel) tuple in possibleChallengeWords)
                {
                    challengeWords.Add(tuple.Item2);
                }
                Word challengeWord = ai.ChooseChallengeWord(challengeWords);
                if (challengeWord is not null)
                {
                    ChallengeWord(challengeWord);
                }
            }

            foreach ((Button, Word, StackPanel) removeTuple in possibleChallengeWords)
            {
                Button removeButton = removeTuple.Item1;
                removeButton.IsEnabled = false;
                removeButton.Visibility = Visibility.Collapsed;
            }
            possibleChallengeWords.Clear();
            if (!deleteTiles)
            {
                currentPlayer.IncrementTurns();
                List<Word> words;
                if (currentPlayer is AI)
                {
                    turnTiles = await Task.Run(() => ((AI)currentPlayer).GetTilesToPlaceAsync(playedTiles));
                    words = Word.GetInterLinkedWords(turnTiles, playedTiles);
                    if (words.Count == 0)
                    {
                        gameOver = true;
                    }
                }
                else
                {
                    words = Word.GetInterLinkedWords(turnTiles, playedTiles);
                }

                currentPlayer.SaveScore();

                if (Word.CheckLetterLocation(turnTiles, playedTiles) && words.Count > 0)
                {
                    int score = 0;
                    foreach (Word word in words)
                    {
                        currentPlayer.AddWord(word);    //add created word to the player's list of words
                        AddWordToPanel(word);   //add word to side panel
                                                //word.GetPopularity();
                        score += word.Value;
                    }

                    //lock letters in place
                    foreach (Tile tile in turnTiles)
                    {
                        playedTiles.Add(tile);
                        Image image = GetImageFromCoord(tile.Coord);
                        image.AllowDrop = false;
                        image.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(DragLetter);

                        if (currentPlayer is AI)
                        {
                            image.Source = tile.Image;
                        }
                    }

                    //check if game over
                    if (letterPool.Count == 0 && currentPlayer.IsOutOfTiles)
                    {
                        gameOver = true;
                    }
                }

                if (gameOver)
                {
                    GameOverWindow gameOverWindow = new(players);
                    gameOverWindow.Owner = this;
                    gameOverWindow.ShowDialog();
                }
                else
                {
                    //remove used bonuses
                    foreach (Word word in words)
                    {
                        foreach (Tile tile in word.word)
                        {
                            tile.Coord.RemoveBonus();
                        }
                    }
                }
            }
            else //delete tiles
            {
                currentPlayer.IncrementTurns();
                letterPool.AddRange(turnTiles);
                letterPool = Shuffle(letterPool);
                deleteTiles = false;
            }

            

            List<Tile> newTiles = letterPool.Take(turnTiles.Count).ToList();
            letterPool.RemoveRange(0, newTiles.Count);
            currentPlayer.ChangeTiles(turnTiles, newTiles);

            previousTurnTiles.Clear();
            previousTurnTiles.AddRange(turnTiles);
            turnTiles.Clear();

            //update UI
            if (currentPlayer is not AI)
            {
                AddTilesToDock(currentPlayer);
            }
            //currentPlayer.IncrementTurns();

            SwitchPlayer();
            if (currentPlayer is AI)
            {
                await FinishTurnAsync();
            }
        }

        private static List<Tile> Shuffle(List<Tile> tiles)
        {
            int length = tiles.Count;
            Random rnd = new();
            for (int i = 0; i < length; i++)        // for each letter in the list, swap it with another one
            {
                int dest = rnd.Next(0, length);
                Tile shuffleTile = tiles[dest];
                tiles[dest] = tiles[i];
                tiles[i] = shuffleTile;
            }
            return tiles;
        }

        private void SwitchPlayer()
        {
            int index = players.IndexOf(currentPlayer);
            index++;
            if (index > players.Count - 1)
            {
                index = 0;
            }
            currentPlayer = players[index];
        }

        //Gets the user who just finished their turn
        private User GetPreviousPlayer()
        {
            int index = players.IndexOf(currentPlayer);
            index--;
            if (index < 0)
            {
                index = players.Count - 1;
            }
            return players[index];

        }

        //Adds a word to the panel of created words
        private void AddWordToPanel(Word word)
        {
            StackPanel stackPanel = new();
            stackPanel.Orientation = Orientation.Horizontal;

            Label label = new();
            label.Content = word.ToString();
            label.Style = (Style)FindResource("smallTextStyle");

            Button button = new();
            button.Content = "🔍";
            button.Click += ChallengeWord;
            ToolTip toolTip = new();
            toolTip.Content = "Challenge";

            button.ToolTip = toolTip;
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            wordList.Children.Add(stackPanel);

            possibleChallengeWords.Add((button, word, stackPanel));
        }

        //called when the user challenges a word
        private void ChallengeWord(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            (Button, Word, StackPanel) tuple = possibleChallengeWords.Find(tuple => tuple.Item1 == button);
            Word word = tuple.Item2;

            ChallengeWord(word);
        }

        private void ChallengeWord(Word word)
        {
            bool valid = word.Validate();

            ChallengeWordWindow challengeWindow = new(valid, word, currentPlayer);
            challengeWindow.Owner = this;
            challengeWindow.ShowDialog();

            User challengedPlayer = GetPreviousPlayer();

            if (valid)
            {
                //remove all challenge buttons, as only one word can be challenged per turn
                foreach ((Button, Word, StackPanel) removeTuple in possibleChallengeWords)
                {
                    Button removeButton = removeTuple.Item1;
                    removeButton.IsEnabled = false;
                    removeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                //remove all words the opponent played
                foreach ((Button, Word, StackPanel) removeTuple in possibleChallengeWords)
                {
                    StackPanel stackpanel = removeTuple.Item3;
                    stackpanel.IsEnabled = false;
                    stackpanel.Visibility = Visibility.Collapsed;
                }

                //remove letters from the gameboard
                foreach (Tile tile in previousTurnTiles)
                {
                    //letterPool.Add(tile);
                    Image image = GetImageFromCoord(tile.Coord);
                    image.Source = nullImage;
                    image.AllowDrop = true;
                    playedTiles.Remove(tile);
                }

                previousTurnTiles.Clear();
                letterPool.AddRange(challengedPlayer.Revert());
            }

            Debug.WriteLine($"{word} is valid? {valid}");
        }

        private void DeleteTile(object sender, DragEventArgs e)
        {
            deleteTiles = true;
            Image image = new();
            image.Source = (ImageSource)e.Data.GetData("System.Windows.Media.Imaging.BitmapImage");
            Tile tileToDelete = GetTileFromImage(image);
            turnTiles.Add(tileToDelete);
            playGrid.IsEnabled = false;
        }

        //change difficulty
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (players is not null)
            {
                ComboBox comboBox = sender as ComboBox;
                ((AI)players[1]).difficulty = (AI.Difficulty)comboBox.SelectedIndex;
            }
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            Instructions instructions = new();
            instructions.Owner = this;
            instructions.ShowDialog();
        }

        /*private void DragOverCanvas(object sender, DragEventArgs e)
        {
            Image image = (Image)sender;
            Point point = e.GetPosition(canvas);
            Canvas.SetLeft(image, point.X);
            Canvas.SetTop(image, point.Y);
            canvas.Children.Add(image);
            Debug.WriteLine("fired");
        }

        private void DragLeaveCanvas(object sender, DragEventArgs e)
        {

        }
        */
    }
}
