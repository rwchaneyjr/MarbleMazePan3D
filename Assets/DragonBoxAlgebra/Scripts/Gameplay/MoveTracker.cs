namespace DragonBoxAlgebra.Gameplay
{
    public class MoveTracker
    {
        public int Moves { get; set; }
        public int CardsPlayed { get; set; }

        public void Reset()
        {
            Moves = 0;
            CardsPlayed = 0;
        }

        public void RegisterCombine()
        {
            Moves++;
        }

        public void RegisterBalancedPlay()
        {
            Moves++;
            CardsPlayed++;
        }

        public int CalculateStars(LevelDefinition level)
        {
            int stars = 1;
            if (Moves <= level.ParMoves)
            {
                stars++;
            }

            if (CardsPlayed <= level.ParCards)
            {
                stars++;
            }

            return stars;
        }
    }
}
