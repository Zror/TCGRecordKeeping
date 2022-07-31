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
                        if (games.Any(g => (g.Team1.PlayerGroupID == j && g.Team2.PlayerGroupID == i) || (g.Team2.PlayerGroupID == j && g.Team1.PlayerGroupID == i)))
                        {
                            double[] currentEquation = new double[highestPlayerID+1];
                            double[] numberOfHandicaps = new double[highestPlayerID+1];
                            bool[] playerInTeamA = new bool[highestPlayerID + 1];
                            bool[] playerInTeamB = new bool[highestPlayerID + 1];
                            double currentEquationAnswer = 0;

                            int numberofGames = 0;
                            List<GameRecord> list1 = games.Where(g => g.Team1.PlayerGroupID == i && g.Team2.PlayerGroupID == j).ToList();
                            foreach (GameRecord record in list1)
                            {
                                numberofGames++;
                                foreach (PlayerHandicap handicap in record.Team1.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                    playerInTeamA[handicap.PlayerID] = true;
                                }
                                foreach (PlayerHandicap handicap in record.Team2.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                    playerInTeamB[handicap.PlayerID] = true;
                                }
                                currentEquationAnswer += record.Team1RemaingLife - record.Team2RemaingLife;
                            }
                            List<GameRecord> list2 = games.Where(g => g.Team1.PlayerGroupID == j && g.Team2.PlayerGroupID == i).ToList();
                            foreach (GameRecord record in list2)
                            {
                                numberofGames++;
                                foreach (PlayerHandicap handicap in record.Team1.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                    playerInTeamB[handicap.PlayerID] = true;
                                }
                                foreach (PlayerHandicap handicap in record.Team2.playerHandicaps)
                                {
                                    numberOfHandicaps[handicap.PlayerID] += handicap.HasHandicap ? 1 : 0;
                                    playerInTeamA[handicap.PlayerID] = true;
                                }
                                currentEquationAnswer += record.Team2RemaingLife - record.Team1RemaingLife;
                            }

                            currentEquationAnswer = currentEquationAnswer / numberofGames;

                            for(int k = 0; k<= highestPlayerID; k++)
                            {
                                if (playerInTeamA[k])
                                {
                                    currentEquation[k] +=  numberofGames / Math.Min(1, numberofGames - Math.Abs(numberOfHandicaps[k])); 
                                }
                                if(playerInTeamB[k])
                                {
                                    currentEquation[k] -= numberofGames / Math.Min(1, numberofGames - Math.Abs(numberOfHandicaps[k]));
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

            Matrix<double> pseudoInvers = equations.PseudoInverse();

            return pseudoInvers.Multiply(Answers);

        }

        public List<double[]> GetPlayerCombinations(int playercout)
        {
            List<double[]> entries = new List<double[]>();
            double[] array = new double[playercout];
            GenerateBinaryStep(playercout, 0, array, entries);

            return entries;

        }

        public void GenerateBinaryStep(int playercount, int index, double[] currentEntry, List<double[]> allEntries)
        {
            if(index == playercount)
            {
                allEntries.Add((double[])currentEntry.Clone());
                return;
            }
            currentEntry[index] = 1;
            GenerateBinaryStep(playercount, index + 1, currentEntry, allEntries);
            currentEntry[index] = -1;
            GenerateBinaryStep(playercount, index + 1, currentEntry, allEntries);
        }

        public List<Tuple<int, string>> CalulcatedScoreNoMinSize(List<Double> scores, List<int> PlayerIds, out double scoreDifference)
        {
            List<Tuple<int, string>> teamDefinitions = new List<Tuple<int, string>>();
            double teamAScore = 0.0;
            double teamBScore = 0.0;
            foreach(int id in PlayerIds)
            {
                double score = scores[id];
                double tempTeamAScore = teamAScore + score;
                double tempTeamBScore = teamBScore + score;
                if (tempTeamAScore >= tempTeamBScore)
                {
                    teamDefinitions.Add(new Tuple<int, string>(id, "Team A"));
                    teamAScore = tempTeamAScore;
                }
                else
                {
                    teamDefinitions.Add(new Tuple<int, string>(id, "Team B"));
                    teamBScore = tempTeamBScore;
                }
            }
            scoreDifference = Math.Abs(teamAScore - teamBScore);
            return teamDefinitions;
        }
        public List<Tuple<int, string>> CalulcatedScoreMinSize(List<double> scores, List<int> PlayerIds, int minTeamSize, out double scoreDifference)
        {
            List<Tuple<int, string>> teamDefinitions = new List<Tuple<int, string>>();
            List<double[]> combinations = GetPlayerCombinations(PlayerIds.Count);
            combinations = combinations.Where(c => !(c.Where(v => v == 1).Count() < minTeamSize) && !(c.Where(v => v == -1).Count() < minTeamSize)).ToList();

            Matrix<double> comboMatrix = Matrix<double>.Build.DenseOfRows(combinations);

            List<double> playerScores = new List<double>();
            foreach(int id in PlayerIds)
            {
                playerScores.Add(scores[id]);
            }
            Vector<double> scoresVector = Vector<double>.Build.DenseOfEnumerable(playerScores);

            Vector<double> teamScores = Vector<double>.Abs( comboMatrix.Multiply(scoresVector) );

            scoreDifference = teamScores.Min<double>();
            int indexOfMin = teamScores.AbsoluteMinimumIndex();

            double[] eqDefinition = combinations[indexOfMin];

            for(int i = 0; i< eqDefinition.Length; i++)
            {
                if(eqDefinition[i] == 1)
                {
                    teamDefinitions.Add(new Tuple<int, string>(PlayerIds[i], "Team A"));
                }
                else
                {
                    teamDefinitions.Add(new Tuple<int, string>(PlayerIds[i], "Team B"));
                }
            }

            return teamDefinitions;
        }


        public List<Tuple<int,string>> GetBestTeamCalcScore(List<int> playerIds, List<GameRecord> games, DataManager dataManager, out double scoreDifference, int minTeamSize = 1)
        {
            scoreDifference = 0.0;
            if (minTeamSize > (playerIds.Count / 2)) return null;
            List<double> scores = GetExpectedRemaingLife(games, dataManager).ToList();

            if (minTeamSize == 1) return CalulcatedScoreNoMinSize(scores, playerIds,out scoreDifference);
            return CalulcatedScoreMinSize(scores, playerIds, minTeamSize, out scoreDifference);
        }

        public List<Tuple<int,string>> GetBestTeamEloScore(int cardGameId, List<int> playerIds, List<GameRecord> games, DataManager dataManager, out double scoreDifference, int minTeamSize = 1)
        {
            scoreDifference = 0.0;
            if (minTeamSize > (playerIds.Count / 2)) return null;
            List<double> scores = dataManager.dataStorage.Players.Select(p => p.ratings.Where(r => r.CardGameId == cardGameId).Select(r => r.Rating).FirstOrDefault()).ToList();


            if (minTeamSize == 1) return CalulcatedScoreNoMinSize(scores, playerIds, out scoreDifference);
            return CalulcatedScoreMinSize(scores, playerIds, minTeamSize, out scoreDifference);
        }
    }
}
