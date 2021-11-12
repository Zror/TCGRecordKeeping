using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGRecordKeeping.DataTypes
{
    public class DataStorage
    {
        public List<Player> Players { get ;set; }
        public List<PlayerGroup> PlayerGroups { get; set; }
        public List <CardGame> CardGames { get; set; }
        public List<GameRecord> GameRecords { get; set; }
        public List<Tournament> Tournaments { get; set; }
    }
}
