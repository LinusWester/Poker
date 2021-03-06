using System;
namespace Poker
{
    class Card : ICard
    {
        private Suite suite{ get; set; }
        public Suite Suite { get => suite; }
        private Rank rank{ get; set; }
        public Rank Rank { get => rank; }

        public Card(Suite suite, Rank rank)
        {
            this.suite = suite;
            this.rank = rank;
        }

        public static implicit operator Card((Suite suite, Rank rank) Card)
            => new Card(Card.suite, Card.rank);

        public override string ToString()
        {
            return (Suite) switch
            {
                Suite.Hearts => "♥",
                Suite.Spades => "♠",
                Suite.Clubs => "♣",
                _ => "♦",
            } + (Rank) switch
            {
                Rank.Ace => "A",
                Rank.King => "K",
                Rank.Queen => "Q",
                Rank.Jack => "J",
                Rank r => ((int)r).ToString(),
            };
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Card c = (Card)obj;
                return (Suite == c.Suite) && (Rank == c.Rank);
            }
        }
    }
}