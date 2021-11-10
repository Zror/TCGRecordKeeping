using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCGRecordKeeping.DataTypes;


namespace TCGRecordKeeping.Managers
{
    class DataManager
    {
        public DataStorage dataStorage { get; set; }
        public string FilePath { get; set; }

        public DataManager(string FilePath)
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
        public bool Save()
        {
            try
            {
                using(StreamWriter stream = new StreamWriter(FilePath))
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
        public Player AddPlayer(string playerName)
        {
            int playerid = dataStorage.Players.Count;
            while(dataStorage.Players.Any(p => p.PlayerID == playerid))
            {
                playerid++;
            }
            Player player = new Player
            {
                PlayerName=playerName,
                PlayerID = playerid,
                ELORating = 1500
            };
            dataStorage.Players.Add(player);
            return player;
        }

        public Tournament GetTournament(int tournamentId)
        {
            return dataStorage.Tournaments.Find(t => t.Id == tournamentId);
        }

        public PlayerGroup AddPlayergroup(List<int> PlayerIds)
        {
            int playerGroupId = dataStorage.PlayerGroups.Count();
            while (dataStorage.Players.Any(p => p.PlayerID == playerGroupId))
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
        public PlayerGroup FindOrCreatePlayerGroup(List<int> PlayerIds)
        {
            PlayerGroup group;

            group = dataStorage.PlayerGroups.Where(p => p.PlayerIds.All(PlayerIds.Contains) && PlayerIds.All(p.PlayerIds.Contains)).FirstOrDefault();

            if (group.Equals(null))
            {
                group = AddPlayergroup(PlayerIds);
            }

            return group;
        }
        public Team GetTeam(List<PlayerHandicap>playerHandicaps)
        {
            List<int> playerIDs = playerHandicaps.Select(h => h.PlayerID).ToList();
            return new Team
            {
                playerHandicaps = playerHandicaps,
                PlayerGroupID = FindOrCreatePlayerGroup(playerIDs).PlayerGroupId
            };
        }
        public GameRecord AddGameRecord(List<PlayerHandicap> Team1Handicaps, List<PlayerHandicap> Team2Handicaps, int CardGameID, int TournamentID,  int Team1RemaingLife, int Team2RemaingLife, int TurnCount, Winner Winner, bool WinnerUsedAlternateWinCondition)
        {
            int recordId = dataStorage.GameRecords.Count;
            while (dataStorage.GameRecords.Any(r => r.GameRecordId == recordId))
            {
                recordId++;
            }
            GameRecord record = new GameRecord
            {
                CardGameId=CardGameID,
                GameRecordId=recordId,
                TournamentId = TournamentID,
                Team1=GetTeam(Team1Handicaps),
                Team2=GetTeam(Team2Handicaps),
                Team1RemaingLife=Team1RemaingLife,
                Team2RemaingLife=Team2RemaingLife,
                TurnCount=TurnCount,
                Winner=Winner,
                WinnerUsedAlternateWinCondition=WinnerUsedAlternateWinCondition
            };
            dataStorage.GameRecords.Add(record);
            return record;
        }
        public CardGame AddCardGame(string CardGameName, int MaxLifeValue, List<KFactorRule> KFactorRules, KFactorModification KFactorModification, List<TurnAdjustmentRule> turnAdjustmentRules)
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
                TurnAdjustmentRules=turnAdjustmentRules
            };
            dataStorage.CardGames.Add(cardGame);
            return cardGame;
        }
        public Tournament AddTournament(string TournamentName, int MaxLifeValue)
        {
            int tournamentId = dataStorage.Tournaments.Count;
            while (dataStorage.Tournaments.Any(T => T.Id == tournamentId))
            {
                tournamentId++;
            }
            Tournament tournament = new Tournament
            {
                Id = tournamentId,
                MaxLifeValue = MaxLifeValue,
                TournamentName = TournamentName
            };
            dataStorage.Tournaments.Add(tournament);
            return tournament;
        }

        public List<Player> TranslateTeamToPlayerlist(Team team)
        {
            PlayerGroup group = dataStorage.PlayerGroups.Find(G => G.PlayerGroupId == team.PlayerGroupID);
            return dataStorage.Players.Where(p => group.PlayerIds.Contains(p.PlayerID)).ToList();
        }
        public CardGame GetCardGame(int id)
        {
            return dataStorage.CardGames.Find(c => c.Id == id);
        }
    }
}
