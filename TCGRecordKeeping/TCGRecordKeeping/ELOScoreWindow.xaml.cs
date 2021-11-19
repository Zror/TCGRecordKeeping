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
    /// Interaction logic for ELOScoreWindow.xaml
    /// </summary>
    public partial class ELOScoreWindow : Window
    {
        public ELOScoreWindow()
        {
            InitializeComponent();
            GridView gridView = new GridView();
            GridViewColumn nameColumn = new GridViewColumn();
            nameColumn.DisplayMemberBinding = new Binding("name");
            nameColumn.Header = "FirstName";
            nameColumn.Width = 100;
            gridView.Columns.Add(nameColumn);
            var mainWindow = ((MainWindow)Application.Current.MainWindow);

            foreach(CardGame game in mainWindow.manager.dataStorage.CardGames)
            {
                GridViewColumn column = new GridViewColumn();
                column.DisplayMemberBinding = new Binding("ELOscore[" + game.Id + "]");
                column.Header = game.CardGameName;
                column.Width = 100;
                gridView.Columns.Add(column);
            }
            ratingListView.View = gridView;
            ratingListView.ItemsSource = GetEloRatings();
        }

        public List<EloRatingView> GetEloRatings()
        {
            return ((MainWindow)Application.Current.MainWindow).manager.dataStorage.Players.Select(p =>
           {
               EloRatingView rating = new EloRatingView
               {
                   name = p.PlayerName,
                   ELOscore = Enumerable.Repeat("-", ((MainWindow)Application.Current.MainWindow).manager.dataStorage.CardGames.Count).ToArray()
               };
               foreach (ELORating eLORating in p.ratings)
               {
                   rating.ELOscore[eLORating.CardGameId] = eLORating.Rating.ToString();
               }
               return rating;
           }).ToList();
        }
    }
    public class EloRatingView
    {
        public string name { get; set; }
        public string[] ELOscore { get; set; }
    }
}
