using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TCGRecordKeeping.Managers;

namespace TCGRecordKeeping
{
    /// <summary>
    /// Interaction logic for GetBestTeamWindow.xaml
    /// </summary>
    public partial class GetBestTeamWindow : Window
    {
        public CalculationManager calcManager { get; set; }
        public GetBestTeamWindow()
        {
            InitializeComponent();
            PlayerListView.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.GetPlayerViewItems("", "")
                                    .Select(i => new PlayersInGameList()
                                    {
                                        Id = i.Id,
                                        Name = i.Name,
                                        InGame =false
                                    });
            GameBox.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.CardGames.Select(c => c.Id.ToString() + ": " + c.CardGameName);
            TournyBox.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName);
            calcManager = new CalculationManager();
        }

        private void UseELOScoreBtn_Click(object sender, RoutedEventArgs e)
        {
            int MinTeamSize = 1;
            if (NoMinTeam.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(MinTeamSizeTextBox.Text) || !int.TryParse(MinTeamSizeTextBox.Text, out MinTeamSize) || MinTeamSize <= 0)
                {
                    MessageBox.Show("Min Team Size needs to be need to be an integer greater than 0");
                    return;
                }
            }

            if (!int.TryParse(TournyBox.SelectedItem.ToString().Split(':')[0], out int tournamentId))
            {
                MessageBox.Show("Please choose valid Tournament");
                return;
            }
            if (!int.TryParse(GameBox.SelectedItem.ToString().Split(':')[0], out int gameId))
            {
                MessageBox.Show("Please choose valid Game");
                return;
            }

            List<int> playerIds = PlayerListView.Items.Cast<PlayersInGameList>().Select(g=>g.Id).OrderBy(i=>i).ToList();


            List<Tuple<int, double>> scores = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Where(p => playerIds.Any(i => i == p.PlayerID)).Select(p =>
            {

                double score = p.ratings.Where(r => r.CardGameId == gameId).Select(r => r.Rating).FirstOrDefault();
                return new Tuple<int, double>(p.PlayerID, score);

            }).OrderBy(t=>t.Item1).ToList();


            List<Tuple<int, string>> output;


            if (NoMinTeam.IsChecked.Value)
            {
                output = calcManager.CalulcatedScoreMinSize(scores.OrderBy(t => t.Item1).Select(t => t.Item2).ToList(), scores.OrderBy(t => t.Item1).Select(t => t.Item1).ToList(), MinTeamSize, out double scoreDifference);
            }
            else
            {
                output = calcManager.CalulcatedScoreMinSize(scores.OrderBy(t => t.Item1).Select(t => t.Item2).ToList(), scores.OrderBy(t => t.Item1).Select(t => t.Item1).ToList(), MinTeamSize, out double scoreDifference);
            }
            List<int> teamAIds = output.Where(t => t.Item2.Equals("Team A")).Select(t => t.Item1).ToList();
            List<string> teamA = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Where(p => teamAIds.Any(i => i == p.PlayerID)).Select(p => p.PlayerName).ToList();
            List<int> teamBIds = output.Where(t => t.Item2.Equals("Team B")).Select(t => t.Item1).ToList();
            List<string> teamB = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Where(p => teamBIds.Any(i => i == p.PlayerID)).Select(p => p.PlayerName).ToList();
            TeamResults.Items.Add( new TeamDefinition()
            {
                TeamA = string.Concat(teamA,","),
                TeamB = string.Concat(teamB,","),
                ScoreDifference = 
            });


        }
    }
    public class PlayersInGameList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool InGame { get; set; }
    }
    public class TeamDefinition
    {
        public string TeamA;
        public string TeamB;
        public string ScoreDifference;
    }
}
