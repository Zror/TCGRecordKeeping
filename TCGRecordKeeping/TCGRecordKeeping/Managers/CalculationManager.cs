using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCGRecordKeeping.DataTypes;
using MathNet.Numerics.LinearAlgebra;

namespace TCGRecordKeeping.Managers
{
    public class CalculationManager
    {
        public void updateELOScores(GameRecord record, DataManager dataManager)
        {
            List<Player> team1Players = dataManager.TranslateTeamToPlayerlist(record.Team1);
            List<Player> team2Players = dataManager.TranslateTeamToPlayerlist(record.Team2);
            double team1Rating = 0;
            double team2Rating = 0;
            foreach (Player p in team1Players)
            {
                ELORating rating = p.ratings.Find(r => r.CardGameId == record.CardGameId);
                if (rating == null)
                {
                    p.ratings.Add(new ELORating
                    {
                        CardGameId = record.CardGameId,
                        Rating = 1500
                    });
                    team1Rating += 1500;
                }
                else
                {
                    team1Rating += rating.Rating;
                }
            }
            foreach (Player p in team2Players)
            {
                ELORating rating = p.ratings.Find(r => r.CardGameId == record.CardGameId);
                if (rating == null)
                {
                    p.ratings.Add(new ELORating
                    {
                        CardGameId = record.CardGameId,
                        Rating = 1500
                    });
                    team2Rating += 1500;
                }
                else
                {
                    team2Rating += rating.Rating;
                }
            }
            double RTeam1 = Math.Pow(10, team1Rating / 400);
            double RTeam2 = Math.Pow(10, team2Rating / 400);
            double ETeam1 = RTeam1 / (RTeam1 + RTeam2);
            double ETeam2 = RTeam2 / (RTeam1 + RTeam2);
            double STeam1, STeam2;
            switch (record.Winner)
            {
                case Winner.Team1:
                    STeam1 = 1;
                    STeam2 = 0;
                    break;
                case Winner.Team2:
                    STeam1 = 0;
                    STeam2 = 1;
                    break;
                case Winner.Tie:
                    STeam1 = 0.5;
                    STeam2 = 0.5;
                    break;
                default:
                    STeam1 = 0.5;
                    STeam2 = 0.5;
                    break;
            }

            CardGame game = dataManager.GetCardGame(record.CardGameId);
            Tournament tournament = dataManager.GetTournament(record.TournamentId);
            double LifeWeight = 1;
            if (record.WinnerUsedAlternateWinCondition)
            {
                LifeWeight = 1;
            }
            else
            {
                double maxPoints = tournament.hasMaxPoints ? (double)tournament.MaxPoints : Math.Max(record.Team1RemaingLife, record.Team2RemaingLife);
                switch (game.KFactorModification)
                {
                    case KFactorModification.NoMod:
                        LifeWeight = 1;
                        break;
                    case KFactorModification.LifePoints:
                        LifeWeight = ((double)Math.Abs(record.Team1RemaingLife - record.Team2RemaingLife)) / maxPoints;
                        break;
                    case KFactorModification.LifePointsAndTurnCount:
                        double turnweight = 0;
                        foreach (TurnAdjustmentRule rule in game.TurnAdjustmentRules)
                        {
                            switch (rule.BoundUse)
                            {
                                case BoundUseDef.NoMaxBound:
                                    if (rule.TurnMinBound <= record.TurnCount)
                                    {
                                        turnweight = rule.AdjustMentValue;
                                        continue;
                                    }
                                    break;
                                case BoundUseDef.NoMinbound:
                                    if(record.TurnCount < rule.TurnMaxBound)
                                    {
                                        turnweight = rule.AdjustMentValue;
                                        continue;
                                    }
                                    break;
                                case BoundUseDef.UseBothBounds:
                                    if(rule.TurnMinBound <= record.TurnCount && record.TurnCount < rule.TurnMaxBound )
                                    {
                                        turnweight = rule.AdjustMentValue;
                                        continue;
                                    }
                                    break;
                            }
                        }
                        LifeWeight = 1- ((1- ((double)Math.Abs(record.Team1RemaingLife - record.Team2RemaingLife)) / maxPoints) *turnweight);
                        break;

                }
            }
            double handicapAdjustment = 1;

            int team1Handicap = record.Team1.playerHandicaps.Where(h => h.HasHandicap).Count();
            int team2Handicap = record.Team2.playerHandicaps.Where(h => h.HasHandicap).Count();

            if((record.Winner == Winner.Team1 && team1Handicap > team2Handicap)|| (record.Winner == Winner.Team2 && team1Handicap < team2Handicap))
            {
                handicapAdjustment = 0.5;
            }

            foreach(Player p in team1Players)
            {
                int k = 1;
                ELORating rating = p.ratings.Find(r => r.CardGameId == record.CardGameId);
                foreach (KFactorRule rule in game.KFactorRules)
                {
                    switch (rule.BoundUse)
                    {
                        case BoundUseDef.NoMaxBound:
                            if (rule.ScoreMinBound <= rating.Rating)
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                        case BoundUseDef.NoMinbound:
                            if (rating.Rating < rule.ScoreMaxBound)
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                        case BoundUseDef.UseBothBounds:
                            if (rule.ScoreMinBound<= rating.Rating && rating.Rating < rule.ScoreMaxBound )
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                    }
                }
                rating.Rating = Math.Floor(rating.Rating + k * (STeam1 - ETeam1) * LifeWeight * handicapAdjustment);
            }
            foreach (Player p in team2Players)
            {
                int k = 1;
                ELORating rating = p.ratings.Find(r => r.CardGameId == record.CardGameId);
                foreach (KFactorRule rule in game.KFactorRules)
                {
                    switch (rule.BoundUse)
                    {
                        case BoundUseDef.NoMaxBound:
                            if (rule.ScoreMinBound <= rating.Rating)
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                        case BoundUseDef.NoMinbound:
                            if (rating.Rating < rule.ScoreMaxBound)
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                        case BoundUseDef.UseBothBounds:
                            if (rule.ScoreMinBound <= rating.Rating && rating.Rating < rule.ScoreMaxBound)
                            {
                                k = rule.KFactor;
                                continue;
                            }
                            break;
                    }
                }
                rating.Rating = Math.Floor(rating.Rating + k * (STeam2 - ETeam2) * LifeWeight * handicapAdjustment);
            }
        }

        public Matrix<double> GetSystemOfEquations(List<GameRecord> games, DataManager dataManager, out Vector<double> Answers)
        {
            int highestPlayerID = dataManager.dataStorage.Players.Select(p => p.PlayerID).Max();
            int highestPlayerGroupId = dataManager.dataStorage.PlayerGroups.Select(p => p.PlayerGroupId).Max();
            List < double[]> equationList = new List<double[]>();
            List<double> equationAnswers = new List<double>();
            for(int i = 0; i<= highestPlayerGroupId; i++)
            {
                if(games.Any(g=>g.Team1.PlayerGroupID == i || g.Team2.PlayerGroupID == i))
                {
                    for(int j = i+1; j <= highestPlayerGroupId; j++)
                    {
                        if (games.Any(g => g.Team1.PlayerGroupID == j || g.Team2.PlayerGroupID == j))
                        {
                            double[] currentEquation = new double[highestPlayerID+1];
                            double[] numberOfHandicaps = new double[highestPlayerID+1];
                            double currentEquationAnswer = 0;

                            int numberofGames = 0;
                            List<GameRecord> list1 = games.Where(g => g.Team1.PlayerGroupID == i && g.Team2.PlayerGroupID == j).ToList();
                            foreach (GameRecord record in list1)
                            {
                                numberofGames++;
                                foreach (PlayerHandicap handicap in record.Team1.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                }
                                foreach (PlayerHandicap handicap in record.Team2.playerHandicaps)
                                {
                                    currentEquation[handicap.PlayerID] -= handicap.HasHandicap ? 1 : 0;
                                }
                                currentEquationAnswer += record.Team1RemaingLife - record.Team2RemaingLife;
                            }
                            List<GameRecord> list2 = games.Where(g => g.Team1.PlayerGroupID == j && g.Team2.PlayerGroupID == i).ToList();
                            foreach (GameRecord record in list2)
                            {
                                numberofGames++;
                                foreach (PlayerHandicap handicap in record.Team1.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] -= handicap.HasHandicap ? 1 : 0;
                                }
                                foreach (PlayerHandicap handicap in record.Team2.playerHandicaps)
                                {
                                    currentEquation[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                }
                                currentEquationAnswer += record.Team2RemaingLife - record.Team1RemaingLife;
                            }

                            currentEquationAnswer = currentEquationAnswer / numberofGames;

                            for(int k = 0; i<= highestPlayerID; i++)
                            {
                                if (numberOfHandicaps[k] < 0)
                                {
                                    currentEquation[k] = (-1) * numberofGames / Math.Min(1, numberofGames - Math.Abs(numberOfHandicaps[k])); 
                                }
                                else
                                {
                                    currentEquation[k] = numberofGames / Math.Min(1, numberofGames - Math.Abs(numberOfHandicaps[k]));
                                }
                            }

                            equationAnswers.Add(currentEquationAnswer);
                            equationList.Add(currentEquation);

                        }
                    }
                }
            }

            Answers = Vector<double>.Build.DenseOfEnumerable(equationAnswers);
            return Matrix<double>.Build.DenseOfRows(equationList);
        }
        public Vector<Double> GetExpectedRemaingLife(List<GameRecord> games, DataManager dataManager)
        {
            Matrix<double> equations = GetSystemOfEquations(games, dataManager, out Vector<double> Answers);

            Matrix<double> pseudoInvers = equations.Inverse();

            return pseudoInvers.Multiply(Answers);

        }
    }
}
