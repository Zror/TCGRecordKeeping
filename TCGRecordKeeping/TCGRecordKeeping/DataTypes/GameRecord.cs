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
        Team2,
        Tie
    }
    public enum BoundUseDef
    {
        UseBothBounds,
        NoMinbound,
        NoMaxBound
    }
    public enum KFactorModification
    {
        NoMod,
        LifePoints,
        LifePointsAndTurnCount
    }
    public class KFactorRule
    {
        public int KFactor { get; set; }
        public int ScoreMinBound { get; set; }
        public int ScoreMaxBound { get; set; }
        public BoundUseDef BoundUse { get; set; }
    }
    public class TurnAdjustmentRule
    {
        public double AdjustMentValue;
        public int TurnMinBound { get; set; }
        public int TurnMaxBound { get; set; }
        public BoundUseDef BoundUse { get; set; }
    }
    public class CardGame
    {
        public int Id { get; set; }
        public string CardGameName { get; set; }
        public List<KFactorRule> KFactorRules { get; set; }
        public KFactorModification KFactorModification { get; set; }
        public List<TurnAdjustmentRule> TurnAdjustmentRules { get; set; }
    }
    public class Tournament
    {
        public int Id { get; set; }
        public string TournamentName { get; set; }
        public int MaxLifeValue { get; set; }
    }
    public class Team
    {
        public int PlayerGroupID { get; set; }
        public List <PlayerHandicap> playerHandicaps { get; set; }
    }
    class GameRecord
    {
        public int GameRecordId { get; set; }
        public int CardGameId { get; set; }
        public int TournamentId { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public int Team1RemaingLife { get; set; }
        public int Team2RemaingLife { get; set; }
        public int TurnCount { get; set; }
        public Winner Winner { get; set; }
        public bool WinnerUsedAlternateWinCondition { get; set; }

    }
}
