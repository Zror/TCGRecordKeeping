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
    /// Interaction logic for AddCardGameWindow.xaml
    /// </summary>
    public partial class AddCardGameWindow : Window
    {
        List<KFactorRule> kFactorRules;
        List<TurnAdjustmentRule> turnRules;
        public AddCardGameWindow()
        {
            InitializeComponent();
            kFactorRules = new List<KFactorRule>();
            turnRules = new List<TurnAdjustmentRule>();
            TurnCountCheckBox.IsChecked = false;
            TurnCountCheckBox.IsEnabled = LifePointCheckBox.IsChecked.Value;
        }

        private void CreateCardGameButton_Click(object sender, RoutedEventArgs e)
        {
            KFactorModification mod;
            if (LifePointCheckBox.IsChecked.Value)
            {
                if (TurnCountCheckBox.IsChecked.Value)
                {
                    mod = KFactorModification.LifePointsAndTurnCount;
                }
                else
                {
                    mod = KFactorModification.LifePoints;
                }
            }
            else
            {
                mod = KFactorModification.NoMod;
            }
            if (kFactorRules.Count() < 1)
            {
                MessageBox.Show("At least 1 kfactor rules are required");
                return;
            }
            if (TurnCountCheckBox.IsChecked.Value && turnRules.Count() < 1)
            {
                MessageBox.Show("At least 1 turn rules are required");
                return;
            }
            ((MainWindow)Application.Current.MainWindow).manager.AddCardGame(CardGameTextBox.Text, kFactorRules, mod, turnRules);
            Close();
        }

        private void LifePointCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TurnCountCheckBox.IsEnabled = LifePointCheckBox.IsChecked.Value;
            if (!LifePointCheckBox.IsChecked.Value)
            {
                TurnCountCheckBox.IsChecked = false;
            }
        }

        private void KScore_Checked(object sender, RoutedEventArgs e)
        {
            if (KScoreNoMax.IsChecked.Value)
            {
                MaxEloTextBox.Text = "";
                MaxEloTextBox.IsEnabled = false;
                MinEloTextBox.IsEnabled = true;
            }
            if (KScoreNoMin.IsChecked.Value)
            {
                MinEloTextBox.Text = "";
                MinEloTextBox.IsEnabled = false;
                MaxEloTextBox.IsEnabled = true;
            }
            if (KScoreBoth.IsChecked.Value)
            {
                MinEloTextBox.IsEnabled = true;
                MaxEloTextBox.IsEnabled = true;
            }
        }
        private void Turn_Checked(object sender, RoutedEventArgs e)
        {
            if (TurnNoMax.IsChecked.Value)
            {
                MaxTurnTextBox.Text = "";
                MaxTurnTextBox.IsEnabled = false;
                MinTurnTextBox.IsEnabled = true;
            }
            if (TurnNoMin.IsChecked.Value)
            {
                MinTurnTextBox.Text = "";
                MinTurnTextBox.IsEnabled = false;
                MaxTurnTextBox.IsEnabled = true;
            }
            if (TurnBoth.IsChecked.Value)
            {
                MinTurnTextBox.IsEnabled = true;
                MaxTurnTextBox.IsEnabled = true;
            }
        }

        private void AddKScoreTuleButton_Click(object sender, RoutedEventArgs e)
        {
            BoundUseDef kscoreBound = BoundUseDef.UseBothBounds; ;
            if (KScoreNoMax.IsChecked.Value)
            {
                kscoreBound = BoundUseDef.NoMaxBound;
            }
            if (KScoreNoMin.IsChecked.Value)
            {
                kscoreBound = BoundUseDef.NoMinbound;
            }
            if(!int.TryParse(KValueTextBox.Text,out int kscore) || kscore<=0)
            {
                MessageBox.Show("K Value needs to be an integer greater than 0");
                return;
            }
            if (!int.TryParse(MinEloTextBox.Text, out int minelo) || minelo < 0)
            {
                if (!KScoreNoMin.IsChecked.Value)
                {
                    MessageBox.Show("Min ELO Value needs to be an integer greater than 0");
                    return;
                }
                minelo = -1;
            }
            if (!int.TryParse(MaxEloTextBox.Text, out int maxelo) || maxelo <= 0)
            {
                if (!KScoreNoMax.IsChecked.Value)
                {
                    MessageBox.Show("max ELO Value needs to be an integer greater than 0");
                    return;
                }
                maxelo = -1;
            }
            kFactorRules.Add(new KFactorRule()
            {
                KFactor = kscore,
                ScoreMinBound = minelo,
                ScoreMaxBound = maxelo,
                BoundUse = kscoreBound
            }) ;
            KFactorListView.ItemsSource = GetKFactorRuleListViews();
            KValueTextBox.Text = "";
            MinEloTextBox.Text = "";
            MaxEloTextBox.Text = "";
        }

        private List<KFactorRuleListView> GetKFactorRuleListViews()
        {
            return kFactorRules.Select(k => new KFactorRuleListView
            {
                kFactor = k.KFactor,
                Range = k.BoundUse == BoundUseDef.UseBothBounds ? string.Format("{0}<=x<{1}", k.ScoreMinBound, k.ScoreMaxBound) :
                        k.BoundUse == BoundUseDef.NoMaxBound ? string.Format("{0}<=x", k.ScoreMinBound) : string.Format("x<{0}", k.ScoreMaxBound)
            }).ToList();
        }
        private List<TurnAdjustmentRuleView> GetTurnAdjustmentRuleViews()
        {
            return turnRules.Select(t => new TurnAdjustmentRuleView
            {
                AdjustmentValue = t.AdjustMentValue,
                Range = t.BoundUse == BoundUseDef.UseBothBounds ? string.Format("{0}<=x<{1}", t.TurnMinBound, t.TurnMaxBound) :
                        t.BoundUse == BoundUseDef.NoMaxBound ? string.Format("{0}<=x", t.TurnMinBound) : string.Format("x<{0}", t.TurnMaxBound)
            }).ToList();
        }

        private void AddTurnRuleButton_Click(object sender, RoutedEventArgs e)
        {
            BoundUseDef TurnBound = BoundUseDef.UseBothBounds; ;
            if (TurnNoMax.IsChecked.Value)
            {
                TurnBound = BoundUseDef.NoMaxBound;
            }
            if (TurnNoMin.IsChecked.Value)
            {
                TurnBound = BoundUseDef.NoMinbound;
            }
            if (!double.TryParse(LifePointWeightTextBox.Text, out double weight) || weight < 0 || weight > 1)
            {
                MessageBox.Show("The Life Point wieght value must be between 0 and 1");
                return;
            }
            if (!int.TryParse(MinTurnTextBox.Text, out int minTurn) || minTurn <= 0)
            {
                if (!TurnNoMin.IsChecked.Value)
                {
                    MessageBox.Show("Min Turn Value needs to be an integer greater than 0");
                    return;
                }
                minTurn = -1;
            }
            if (!int.TryParse(MaxTurnTextBox.Text, out int maxturn) || maxturn <= 0)
            {
                if (!TurnNoMax.IsChecked.Value)
                {
                    MessageBox.Show("Max Turn Value needs to be an integer greater than 0");
                    return;
                }
                maxturn = -1;
            }
            turnRules.Add(new TurnAdjustmentRule()
            {
                AdjustMentValue = weight,
                BoundUse = TurnBound,
                TurnMaxBound = maxturn,
                TurnMinBound = minTurn,
            });
            TurnRuleListView.ItemsSource = GetTurnAdjustmentRuleViews();
            LifePointWeightTextBox.Text = "";
            MinTurnTextBox.Text = "";
            MaxTurnTextBox.Text = "";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
