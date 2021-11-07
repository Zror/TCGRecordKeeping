using Newtonsoft.Json;
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
                dataStorage = new DataStorage();
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
                ELOScore = 1500
            };
            dataStorage.Players.Add(player);
            return player;
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
        public PlayerGroup findOrCreatePlayerGroup(List<int> PlayerIds)
        {
            PlayerGroup group;

            group = dataStorage.PlayerGroups.Where(p => p.PlayerIds.All(PlayerIds.Contains) && PlayerIds.All(p.PlayerIds.Contains)).FirstOrDefault();

            if (group.Equals(null))
            {
                group = AddPlayergroup(PlayerIds);
            }

            return group;
        }
        public Team GetTeam (List<player>)
    }
}
