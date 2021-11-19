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

namespace TCGRecordKeeping.DataTypes
{
    /// <summary>
    /// Interaction logic for ExpectedValueWindow.xaml
    /// </summary>
    public partial class ExpectedValueWindow : Window
    {
        public ExpectedValueWindow(int tournamentId, int GameId)
        {
            InitializeComponent();
            var manager = ((MainWindow)Application.Current.MainWindow).manager;

            IEnumerable<GameRecord> records = manager.dataStorage.GameRecords;
            if(tournamentId >= 0)
            {
                records = records.Where(r => r.TournamentId == tournamentId);
            }
            if(GameId >= 0)
            {
                records = records.Where(r => r.CardGameId == GameId);
            }
            var scores = manager.calcManager.GetExpectedRemaingLife(records.ToList(), manager);

            expectedScoreListView.ItemsSource = manager.dataStorage.Players.Select(p => new expectedScoreListView
            {
                Name = p.PlayerName,
                Score = scores[p.PlayerID]
            });
        }
    }
    public class expectedScoreListView
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }
}
