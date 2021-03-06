using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Poker
{
    class Deck : IEnumerable<Card>
    {
        private List<Card> cards = new List<Card>();
        public List<Card> Cards {get=> cards;}

        public Deck()
        {
            foreach (Suite s in Enum.GetValues(typeof(Suite)))
            {
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card((Suite)s, (Rank)r));
                }
            }
        }

        public Card GetTopCard()
        {
            if (!cards.Any())
            {
                throw new NullReferenceException("Leken är tom!");
            }
            
            Card drawnCard;

            drawnCard = this.cards.First();
            this.cards.Remove(drawnCard);

            return drawnCard;
        }

        public void PutBackCard(Card card)
        {
            if(cards.Count>=52)
            {
                throw new ArgumentException("The deck is full, can not add more cards", nameof(cards));
            }
            
            this.cards.Add(card);
        }

        public void Shuffle()//Fisher-Yates
        {
            Random random = new Random();

            for (int i = cards.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);

                Card temp = cards[i];// temporärt kort = 1a index
                cards[i] = cards[randomIndex]; // kort på 1a index blir kortet på random index
                cards[randomIndex] = temp; // kort på random index blir kortet från 1a index
            }
        }
        public IEnumerator GetEnumerator() => cards.GetEnumerator();

        IEnumerator<Card> IEnumerable<Card>.GetEnumerator()
        {
            return cards.GetEnumerator();
        }
    }
}