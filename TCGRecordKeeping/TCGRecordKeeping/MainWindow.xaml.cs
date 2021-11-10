using Microsoft.Win32;
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
        DataManager manager;
        public MainWindow()
        {
            InitializeComponent();
            Save.IsEnabled = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (manager == null) return;
            manager.Save();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (manager != null)
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
            if (string.IsNullOrWhiteSpace(DatabaseFileBox.Text))
            {
                MessageBox.Show("Please enter a file");
                return;
            }
            manager = new DataManager(DatabaseFileBox.Text);
            Save.IsEnabled = true;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog FileDialog = new SaveFileDialog();
            FileDialog.Filter = "Json files (*.json)|*.json";
            if (FileDialog.ShowDialog() == true)
            {
                DatabaseFileBox.Text = FileDialog.FileName;
                return;
            }
        }
    }
}
