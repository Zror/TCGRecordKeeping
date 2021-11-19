using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCGRecordKeeping.DataTypes;


namespace TCGRecordKeeping.Managers
{
    public class DataManager
    {
        public DataStorage dataStorage { get; set; }
        public string FilePath { get; set; }
        public CalculationManager calcManager { get; set; }

        public DataManager()
        {
            dataStorage = new DataStorage()
            {
                CardGames = new List<CardGame>(),
                GameRecords = new List<GameRecord>(),
                PlayerGroups = new List<PlayerGroup>(),
                Players = new List<Player>(),
                Tournaments = new List<Tournament>()
            };
            calcManager = new CalculationManager();
        }
        public bool Save()
        {
            try
            {
                using (StreamWriter stream = new StreamWriter(FilePath))
                {
                    stream.Write(JsonConvert.SerializeObject(dataStorage));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Load(string FilePath)
        {
            this.FilePath = FilePath;
            if (File.Exists(FilePath))
            {
                using (StreamReader stream = new StreamReader(FilePath))
                {
                    dataStorage = JsonConvert.DeserializeObject<DataStorage>(stream.ReadToEnd());
                }
            }
            else
            {
                dataStorage = new DataStorage()
                {
                    CardGames = new List<CardGame>(),
                    GameRecords = new List<GameRecord>(),
                    PlayerGroups = new List<PlayerGroup>(),
                    Players = new List<Player>(),
                    Tournaments = new List<Tournament>()
                };
            }
        }


        public CardGame AddCardGame(string CardGameName, List<KFactorRule> KFactorRules, KFactorModification KFactorModification, List<TurnAdjustmentRule> turnAdjustmentRules)
        {
            int cardGameId = dataStorage.CardGames.Count;
            while (dataStorage.CardGames.Any(C => C.Id == cardGameId))
            {
                cardGameId++;
            }
            CardGame cardGame = new CardGame
            {
                Id = cardGameId,
                CardGameName = CardGameName,
                KFactorModification = KFactorModification,
                KFactorRules = KFactorRules,
                TurnAdjustmentRules = turnAdjustmentRules
            };
            dataStorage.CardGames.Add(cardGame);
            return cardGame;
        }
        public GameRecord AddGameRecord(List<PlayerHandicap> Team1Handicaps, List<PlayerHandicap> Team2Handicaps, int CardGameID, int TournamentID, int Team1RemaingLife, int Team2RemaingLife, int TurnCount, Winner Winner, bool WinnerUsedAlternateWinCondition)
        {
            int recordId = dataStorage.GameRecords.Count;
            while (dataStorage.GameRecords.Any(r => r.GameRecordId == recordId))
            {
                recordId++;
            }
            GameRecord record = new GameRecord
            {
                CardGameId = CardGameID,
                GameRecordId = recordId,
                TournamentId = TournamentID,
                Team1 = GetTeam(Team1Handicaps),
                Team2 = GetTeam(Team2Handicaps),
                Team1RemaingLife = Team1RemaingLife,
                Team2RemaingLife = Team2RemaingLife,
                TurnCount = TurnCount,
                Winner = Winner,
                WinnerUsedAlternateWinCondition = WinnerUsedAlternateWinCondition
            };
            dataStorage.GameRecords.Add(record);
            calcManager.updateELOScores(record, this);
            return record;
        }
        public Player AddPlayer(string playerName)
        {
            int playerid = dataStorage.Players.Count;
            while (dataStorage.Players.Any(p => p.PlayerID == playerid))
            {
                playerid++;
            }
            Player player = new Player
            {
                PlayerName = playerName,
                PlayerID = playerid,
                ratings = new List<ELORating>()
            };
            dataStorage.Players.Add(player);
            return player;
        }
        public PlayerGroup AddPlayergroup(List<int> PlayerIds)
        {
            int playerGroupId = dataStorage.PlayerGroups.Count();
            while (dataStorage.PlayerGroups.Any(p => p.PlayerGroupId == playerGroupId))
            {
                playerGroupId++;
            }
            PlayerGroup group = new PlayerGroup
            {
                PlayerGroupId = playerGroupId,
                PlayerIds = PlayerIds
            };
            dataStorage.PlayerGroups.Add(group);
            return group;
        }
        public Tournament AddTournament(string TournamentName, int MaxPoints, bool hasMaxPoint)
        {
            int tournamentId = dataStorage.Tournaments.Count;
            while (dataStorage.Tournaments.Any(T => T.Id == tournamentId))
            {
                tournamentId++;
            }
            Tournament tournament = new Tournament
            {
                Id = tournamentId,
                MaxPoints = MaxPoints,
                TournamentName = TournamentName,
                hasMaxPoints = hasMaxPoint
            };
            dataStorage.Tournaments.Add(tournament);
            return tournament;
        }
        public PlayerGroup FindOrCreatePlayerGroup(List<int> PlayerIds)
        {
            PlayerGroup group;

            group = dataStorage.PlayerGroups.Where(p => p.PlayerIds.All(PlayerIds.Contains) && PlayerIds.All(p.PlayerIds.Contains)).FirstOrDefault();

            if (group == null)
            {
                group = AddPlayergroup(PlayerIds);
            }

            return group;
        }
        public CardGame GetCardGame(int id)
        {
            return dataStorage.CardGames.Find(c => c.Id == id);
        }
        public Team GetTeam(List<PlayerHandicap> playerHandicaps)
        {
            List<int> playerIDs = playerHandicaps.Select(h => h.PlayerID).ToList();
            return new Team
            {
                playerHandicaps = playerHandicaps,
                PlayerGroupID = FindOrCreatePlayerGroup(playerIDs).PlayerGroupId
            };
        }
        public Tournament GetTournament(int tournamentId)
        {
            return dataStorage.Tournaments.Find(t => t.Id == tournamentId);
        }
        
        public List<Player> TranslateTeamToPlayerlist(Team team)
        {
            PlayerGroup group = dataStorage.PlayerGroups.Find(G => G.PlayerGroupId == team.PlayerGroupID);
            return dataStorage.Players.Where(p => group.PlayerIds.Contains(p.PlayerID)).ToList();
        }

        public List<SimpleViewItem> GetPlayerViewItems(string idStr, string namSubStr)
        {
            IEnumerable<SimpleViewItem> data = dataStorage.Players.Select(p => new SimpleViewItem
            {
                Id = p.PlayerID,
                Name = p.PlayerName
            });
            if (!string.IsNullOrWhiteSpace(idStr))
            {
                if(int.TryParse(idStr, out int id))
                {
                    data = data.Where(p => p.Id == id);
                }
            }
            if(!string.IsNullOrWhiteSpace(namSubStr))
            {
                data = data.Where(p => p.Name.ToUpper().Contains(namSubStr.ToUpper()));
            }
            return data.ToList();
        }
        public List<SimpleViewItem> GetCardGameViewItems(string idStr, string namSubStr)
        {
            IEnumerable<SimpleViewItem> data = dataStorage.CardGames.Select(p => new SimpleViewItem
            {
                Id = p.Id,
                Name = p.CardGameName
            });
            if (!string.IsNullOrWhiteSpace(idStr))
            {
                if (int.TryParse(idStr, out int id))
                {
                    data = data.Where(p => p.Id == id);
                }
            }
            if (!string.IsNullOrWhiteSpace(namSubStr))
            {
                data = data.Where(p => p.Name.ToUpper().Contains(namSubStr.ToUpper()));
            }
            return data.ToList();
        }
        public List<SimpleViewItem> GetTournamentViewItems(string idStr, string namSubStr)
        {
            IEnumerable<SimpleViewItem> data = dataStorage.Tournaments.Select(p => new SimpleViewItem
            {
                Id = p.Id,
                Name = p.TournamentName,
                MaxPoints = p.hasMaxPoints? p.MaxPoints.ToString(): "N/A",
            });
            if (!string.IsNullOrWhiteSpace(idStr))
            {
                if (int.TryParse(idStr, out int id))
                {
                    data = data.Where(p => p.Id == id);
                }
            }
            if (!string.IsNullOrWhiteSpace(namSubStr))
            {
                data = data.Where(p => p.Name.ToUpper().Contains(namSubStr.ToUpper()));
            }
            return data.ToList();
        }
        public void RecalculateEloScore(int tournyID)
        {
            IEnumerable<GameRecord> games = dataStorage.GameRecords;
            if(tournyID >= 0)
            {
                games = games.Where(g => g.TournamentId == tournyID);
            }
            foreach(Player p in dataStorage.Players)
            {
                p.ratings = new List<ELORating>();
            }
            foreach(GameRecord game in games)
            {
                calcManager.updateELOScores(game, this);
            }
        }
    }
}
