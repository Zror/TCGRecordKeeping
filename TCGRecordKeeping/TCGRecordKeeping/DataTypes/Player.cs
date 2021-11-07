using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGRecordKeeping.DataTypes
{
    public class Player
    {
        public string PlayerName { get; set; }
        public int PlayerID { get; set; }
        public double ELOScore { get; set; }
    }
    public class PlayerGroup
    {
        public int PlayerGroupId { get; set; }
        public List<int> PlayerIds { get; set; }
    }
    public class PlayerHandicap
    {
        public int PlayerID { get; set; }
        public bool HasHandicap { get; set; }
    }
}
