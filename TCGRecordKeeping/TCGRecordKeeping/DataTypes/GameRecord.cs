using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGRecordKeeping.DataTypes
{
    public enum Winner
    {
        Team1,
        Team2
    }
    public class CardGame
    {
        public int Id { get; set; }
        public string CardGameName { get; set; }
        public int MaxLifeValue { get; set; }
    }
    public class Team
    {
        public int PlayerListID { get; set; }
        public List <PlayerHandicap> playerHandicaps { get; set; }
    }
    class GameRecord
    {
        public int GameRecordId { get; set; }
        public int CardGameId { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public int Team1RemaingLife { get; set; }
        public int Team2RemaingLife { get; set; }
        public int TurnCount { get; set; }
        public Winner Winner { get; set; }
        public bool WinnerUsedAlternateWinCondition { get; set; }

    }
}
