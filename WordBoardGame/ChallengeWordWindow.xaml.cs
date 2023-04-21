using System.Windows;

namespace WordBoardGame
{
    /// <summary>
    /// Interaction logic for ChallengeWordWindow.xaml
    /// </summary>
    public partial class ChallengeWordWindow : Window
    {
        public ChallengeWordWindow(bool valid, Word word, User currentPlayer)
        {
            InitializeComponent();

            if (currentPlayer is AI ai)
            {
                if (valid)
                {
                    outcome.Text = $"{ai.Name} unsuccessfully challenged '{word}'";
                }
                else
                {
                    outcome.Text = $"{ai.Name} successfully challenged '{word}'";
                }
            }
            else
            {
                if (valid)
                {
                    outcome.Text = $"Your challenge of {word} was unsuccessful";
                }
                else
                {
                    outcome.Text = $"You successfully challenged {word}";
                }
            }
        }
    }
}
