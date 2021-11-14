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
using TCGRecordKeeping.DataTypes;

namespace TCGRecordKeeping
{
    /// <summary>
    /// Interaction logic for AddGameRecordWindow.xaml
    /// </summary>
    public partial class AddGameRecordWindow : Window
    {
        public List<PlayerHandicapListView> Team1;
        public List<PlayerHandicapListView> Team2;
        public AddGameRecordWindow()
        {
            InitializeComponent();
            Team1 = new List<PlayerHandicapListView>();
            Team2 = new List<PlayerHandicapListView>();
            PlayerlistView.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
            GameComboBox.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.CardGames.Select(c => c.Id.ToString() + ": " + c.CardGameName);
            TournamentComboBox.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Tournaments.Select(t=>t.Id.ToString() + ": " + t.TournamentName);
        }

        private void Team1Button_Click(object sender, RoutedEventArgs e)
        {
            int id = ((SimpleViewItem)(((Button)e.OriginalSource).DataContext)).Id;
            Player player = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Find(p => p.PlayerID == id);
            if (Team1.Any(p => p.Id == id)) return;
            Team1ListView.Items.Add(new PlayerHandicapListView
            {
                Id = id,
                Name = player.PlayerName,
            });
            Team1 = Team1ListView.Items.Cast<PlayerHandicapListView>().ToList();
        }

        private void Team2Button_Click(object sender, RoutedEventArgs e)
        {
            int id = ((SimpleViewItem)(((Button)e.OriginalSource).DataContext)).Id;
            Player player = ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Find(p => p.PlayerID == id);
            if (Team2.Any(p => p.Id == id)) return;
            Team2ListView.Items.Add(new PlayerHandicapListView
            {
                Id = id,
                Name = player.PlayerName,
            });
            Team2 = Team2ListView.Items.Cast<PlayerHandicapListView>().ToList();
        }

        private void filterTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            PlayerlistView.ItemsSource = ((MainWindow)Application.Current.MainWindow).manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            Winner winner;

            if (Team1RadioButton.IsChecked.Value)
            {
                winner = Winner.Team1;
            }
            else if (Team2RadioButton.IsChecked.Value)
            {
                winner = Winner.Team2;
            }
            else
            {
                winner = Winner.Tie;
            }

            int team1score, team2score, turncount;


            if(string.IsNullOrWhiteSpace(Team1Points.Text) && string.IsNullOrWhiteSpace(Team1Points.Text))
            {
                switch (winner)
                {
                    case Winner.Team1:
                        team1score = 1;
                        team2score = 0;
                        break;
                    case Winner.Team2:
                        team1score = 0;
                        team2score = 1;
                        break;
                    case Winner.Tie:
                    default:
                        team1score = 0;
                        team2score = 0;
                        break;
                }
            }
            else if(!int.TryParse(Team1Points.Text, out team1score))
            {
                MessageBox.Show("Team 1 Points needs to be need to be an integer or both team1 and team 2 need to be empty");
                return;
            }
            else if (!int.TryParse(Team2Points.Text, out team2score))
            {
                MessageBox.Show("Team 2 Points needs to be need to be an integer or both team1 and team 2 need to be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(TurnCount.Text))
            {
                turncount = 0;
            }
            else if(!int.TryParse(TurnCount.Text, out turncount))
            {
                MessageBox.Show("Turn Count needs to be need to be an integer or the value needs to be empty");
                return;
            }

            int.TryParse(TournamentComboBox.SelectedItem.ToString().Split(':')[0], out int tournamentId);
            int.TryParse(GameComboBox.SelectedItem.ToString().Split(':')[0], out int gameId);

            ((MainWindow)Application.Current.MainWindow).manager.AddGameRecord(
                                Team1ListView.Items.Cast<PlayerHandicapListView>().Select(p => new PlayerHandicap
                                {
                                    HasHandicap = p.HasHandicap,
                                    PlayerID = p.Id
                                }).ToList(),
                                Team2ListView.Items.Cast<PlayerHandicapListView>().Select(p => new PlayerHandicap
                                {
                                    HasHandicap = p.HasHandicap,
                                    PlayerID = p.Id
                                }).ToList(),
                                gameId,
                                tournamentId,
                                team1score,
                                team2score,
                                turncount,
                                winner,
                                UsedAlternateWinCondition.IsChecked.Value);
            Close();
        }
    }
    public class PlayerHandicapListView
    {
        public int Id { get; set; }
        public string Name { get; set;}
        public bool HasHandicap { get; set; }
    }
} 
