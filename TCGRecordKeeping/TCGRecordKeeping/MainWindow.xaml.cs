using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCGRecordKeeping.DataTypes;
using TCGRecordKeeping.Managers;

namespace TCGRecordKeeping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataManager manager;
        static string allTournaments = "All Tournaments";
        public MainWindow()
        {
            InitializeComponent();
            manager = new DataManager();
            Save.IsEnabled = false;
            AddPlayerButton.IsEnabled = false;
            IdFilter.IsEnabled = false;
            PlayerNameFilter.IsEnabled = false;
            DatabaseFileBox.IsEnabled = false;
            MaxPointsBox.IsEnabled = HasMaxPointValueChxBox.IsChecked.Value;
            HasMaxPointValueChxBox.IsEnabled = false;
            TournamentNameBox.IsEnabled = false;
            AddCardGamebtn.IsEnabled = false;
            AddTournamentButton.IsEnabled = false;
            AddGameRecordButton.IsEnabled = false;
            showCurrentELORating.IsEnabled = false;
            recalcuateELOBtn.IsEnabled = false;
            ELOTournyBox.IsEnabled = false;
            expectedScoreGameBox.IsEnabled = false;
            expectedScoreTournyBox.IsEnabled = false;
            showExpectedValueBtn.IsEnabled = false;
            IdCardGameFilter.IsEnabled = false;
            CardGameNameFilter.IsEnabled = false;
            TournamentIdFilter.IsEnabled = false;
            TournamentNameFilter.IsEnabled = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (manager == null) return;
            if (string.IsNullOrWhiteSpace(manager.FilePath)) return;
            manager.Save();
            MessageBox.Show("File Saved");
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (manager == null) return;
            if (!string.IsNullOrWhiteSpace(manager.FilePath))
            {
                MessageBoxResult result = MessageBox.Show("An existing file is loaded, would you like to save before continuing?", "Save file", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        manager.Save();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.No:
                    default:
                        break;
                }
            }
            OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Filter = "Json files (*.json)|*.json";
            FileDialog.FileName = "TCGDatafile";
            if (FileDialog.ShowDialog() == true)
            {
                DatabaseFileBox.Text = FileDialog.FileName;
            }
            if (string.IsNullOrWhiteSpace(DatabaseFileBox.Text))
            {
                MessageBox.Show("Please enter a file");
                return;
            }
            manager.Load(DatabaseFileBox.Text);
            Save.IsEnabled = true;
            AddPlayerButton.IsEnabled = true;
            IdFilter.IsEnabled = true;
            PlayerNameFilter.IsEnabled = true;

            HasMaxPointValueChxBox.IsEnabled = true;
            TournamentNameBox.IsEnabled = true;
            AddCardGamebtn.IsEnabled = true;
            AddTournamentButton.IsEnabled = true;
            AddGameRecordButton.IsEnabled = true;
            showCurrentELORating.IsEnabled = true;
            recalcuateELOBtn.IsEnabled = true;
            ELOTournyBox.IsEnabled = true;
            expectedScoreGameBox.IsEnabled = true;
            expectedScoreTournyBox.IsEnabled = true;
            showExpectedValueBtn.IsEnabled = true;
            IdCardGameFilter.IsEnabled = true;
            CardGameNameFilter.IsEnabled = true;
            TournamentIdFilter.IsEnabled = true;
            TournamentNameFilter.IsEnabled = true;
            PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
            CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
            TournamentListView.ItemsSource = manager.GetTournamentViewItems(TournamentIdFilter.Text, TournamentNameFilter.Text);
            List<string> eloTournyList = new List<string>()
            {
                allTournaments
            };
            eloTournyList.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
            ELOTournyBox.ItemsSource = eloTournyList;
            List<string> eloTournyList2 = new List<string>()
            {
                allTournaments
            };
            eloTournyList2.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
            expectedScoreTournyBox.ItemsSource = eloTournyList2;
            expectedScoreGameBox.ItemsSource = manager.dataStorage.CardGames.Select(c => c.Id.ToString() + ": " + c.CardGameName);
        }

        private void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog FileDialog = new SaveFileDialog();
            FileDialog.Filter = "Json files (*.json)|*.json";
            FileDialog.FileName = "TCGDatafile";
            if (FileDialog.ShowDialog() == true)
            {
                DatabaseFileBox.Text = FileDialog.FileName;
                if (!string.IsNullOrWhiteSpace(DatabaseFileBox.Text))
                {
                    if (File.Exists(DatabaseFileBox.Text))
                    {
                        MessageBoxResult result = MessageBox.Show("File already exists, continuing will delete all contents of existing file. would you like to continue?", "Warning", MessageBoxButton.YesNoCancel);

                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                break;
                            case MessageBoxResult.Cancel:
                            case MessageBoxResult.No:
                            default:
                                return;
                        }
                    }
                    manager = new DataManager();
                    manager.FilePath = DatabaseFileBox.Text;
                    manager.Save();
                    Save.IsEnabled = true;
                    AddPlayerButton.IsEnabled = true;
                    IdFilter.IsEnabled = true;
                    PlayerNameFilter.IsEnabled = true;
                    HasMaxPointValueChxBox.IsEnabled = true;
                    TournamentNameBox.IsEnabled = true;
                    AddCardGamebtn.IsEnabled = true;
                    AddTournamentButton.IsEnabled = true;
                    AddGameRecordButton.IsEnabled = true;
                    showCurrentELORating.IsEnabled = true;
                    recalcuateELOBtn.IsEnabled = true;
                    ELOTournyBox.IsEnabled = true;
                    expectedScoreGameBox.IsEnabled = true;
                    expectedScoreTournyBox.IsEnabled = true;
                    showExpectedValueBtn.IsEnabled = true;
                    IdCardGameFilter.IsEnabled = true;
                    CardGameNameFilter.IsEnabled = true;
                    TournamentIdFilter.IsEnabled = true;
                    TournamentNameFilter.IsEnabled = true;
                    PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
                    CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
                    TournamentListView.ItemsSource = manager.GetTournamentViewItems(TournamentIdFilter.Text, TournamentNameFilter.Text);
                    List<string> eloTournyList = new List<string>()
                    {
                        allTournaments
                    };
                    eloTournyList.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
                    ELOTournyBox.ItemsSource = eloTournyList;
                    List<string> eloTournyList2 = new List<string>()
                    {
                        allTournaments
                    };
                    eloTournyList2.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
                    expectedScoreTournyBox.ItemsSource = eloTournyList2;
                    expectedScoreGameBox.ItemsSource = manager.dataStorage.CardGames.Select(c => c.Id.ToString() + ": " + c.CardGameName);

                }
                return;
            }
        }

        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            string name = AddPlayerName.Text;
            AddPlayerName.Text = "";
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            if(manager.dataStorage.Players.Any(p=> p.PlayerName.Trim().ToUpper() == name.Trim().ToUpper()))
            {
                return;
            }
            manager.AddPlayer(name);
            PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text,PlayerNameFilter.Text);
        }

        private void filterTextChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
        }

        private void CardGameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
        }
        private void AddCardGamebtn_Click(object sender, RoutedEventArgs e)
        {
            AddCardGameWindow cardGameWindow = new AddCardGameWindow();
            cardGameWindow.ShowDialog();
            CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
            expectedScoreGameBox.ItemsSource = manager.dataStorage.CardGames.Select(c => c.Id.ToString() + ": " + c.CardGameName);
        }


        private void TournamentFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            TournamentListView.ItemsSource = manager.GetTournamentViewItems(TournamentIdFilter.Text, TournamentNameFilter.Text);
        }

        private void HasMaxPointValueChxBox_Checked(object sender, RoutedEventArgs e)
        {
            MaxPointsBox.IsEnabled = HasMaxPointValueChxBox.IsChecked.Value;
        }

        private void AddTournamentButton_Click(object sender, RoutedEventArgs e)
        {
            int maxPoints =-1;
            if (HasMaxPointValueChxBox.IsChecked.Value)
            {
                int.TryParse(MaxPointsBox.Text, out maxPoints);
            }
            manager.AddTournament(TournamentNameBox.Text, maxPoints, HasMaxPointValueChxBox.IsChecked.Value);
            TournamentListView.ItemsSource = manager.GetTournamentViewItems(TournamentIdFilter.Text, TournamentNameFilter.Text);

            List<string> eloTournyList = new List<string>()
            {
                allTournaments
            };
            eloTournyList.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
            ELOTournyBox.ItemsSource = eloTournyList;
            List<string> eloTournyList2 = new List<string>()
            {
                allTournaments
            };
            eloTournyList2.AddRange(manager.dataStorage.Tournaments.Select(t => t.Id.ToString() + ": " + t.TournamentName));
            expectedScoreTournyBox.ItemsSource = eloTournyList2;
        }

        private void AddGameRecordButton_Click(object sender, RoutedEventArgs e)
        {
            AddGameRecordWindow window = new AddGameRecordWindow();
            window.ShowDialog();
        }

        private void showCurrentELORating_Click(object sender, RoutedEventArgs e)
        {
            ELOScoreWindow window = new ELOScoreWindow();
            window.ShowDialog();
        }

        private void recalcuateELOBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ELOTournyBox.SelectedItem == null || string.IsNullOrWhiteSpace(ELOTournyBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Please select a tournament");
                return;
            }
            int tournyID;
            if (ELOTournyBox.SelectedItem.ToString().Equals(allTournaments))
            {
                tournyID = -1;
            }
            else
            {
                if(!int.TryParse(ELOTournyBox.SelectedItem.ToString().Split(':')[0], out tournyID))
                {
                    MessageBox.Show("Please select a valid tournament");
                    return;
                }
            }
            manager.RecalculateEloScore(tournyID);
        }

        private void showExpectedValueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (expectedScoreTournyBox.SelectedItem == null || string.IsNullOrWhiteSpace(expectedScoreTournyBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Please select a tournament");
                return;
            }
            if(expectedScoreGameBox.SelectedItem == null || string.IsNullOrWhiteSpace(expectedScoreGameBox.SelectedItem.ToString()))
            {
                MessageBox.Show("Please select a Game");
                return;
            }
            int tournyId, gameId;
            if (expectedScoreTournyBox.SelectedItem.ToString().Equals(allTournaments))
            {
                tournyId = -1;
            }
            else
            {
                if (!int.TryParse(expectedScoreTournyBox.SelectedItem.ToString().Split(':')[0], out tournyId))
                {
                    MessageBox.Show("Please select a valid tournament");
                    return;
                }
            }
            if (!int.TryParse(expectedScoreGameBox.SelectedItem.ToString().Split(':')[0], out gameId))
            {
                MessageBox.Show("Please select a valid tournament");
                return;
            }
            ExpectedValueWindow window = new ExpectedValueWindow(tournyId, gameId);
            window.ShowDialog();
        }
    }
}
