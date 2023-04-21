using System.Collections.Generic;
using System.Windows;

namespace WordBoardGame
{
    /// <summary>
    /// Interaction logic for GameOver.xaml
    /// </summary>
    public partial class GameOverWindow : Window
    {
        public GameOverWindow(List<User> players)
        {
            InitializeComponent();
            foreach (var player in players)
            {
                if (player is AI)
                {
                    computerScore.Content = player.Score;
                }
                else
                {
                    userScore.Content = player.Score;
                }
            }
        }
    }
}
