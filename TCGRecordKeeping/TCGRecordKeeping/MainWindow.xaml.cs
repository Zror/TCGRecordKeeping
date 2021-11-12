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
        public MainWindow()
        {
            InitializeComponent();
            manager = new DataManager();
            Save.IsEnabled = false;
            AddPlayerButton.IsEnabled = false;
            IdFilter.IsEnabled = false;
            PlayerNameFilter.IsEnabled = false;
            DatabaseFileBox.IsEnabled = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (manager == null) return;
            if (string.IsNullOrWhiteSpace(manager.FilePath)) return;
            manager.Save();
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
            PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
            CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
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
                    PlayerListView.ItemsSource = manager.GetPlayerViewItems(IdFilter.Text, PlayerNameFilter.Text);
                    CardGameListView.ItemsSource = manager.GetCardGameViewItems(IdCardGameFilter.Text, CardGameNameFilter.Text);
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
        }
    }
}
