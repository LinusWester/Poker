using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Poker.Lib;

namespace Poker
{
    class Game : IPokerGame
    {
        public event OnNewDeal NewDeal;
        public event OnSelectCardsToDiscard SelectCardsToDiscard;
        public event OnRecievedReplacementCards RecievedReplacementCards;
        public event OnShowAllHands ShowAllHands;
        public event OnWinner Winner;
        public event OnDraw Draw;
        public IPlayer[] Players { get=>table.Players.ToArray(); set=>Players=table.Players.ToArray();}//ÄNDRAD
        private Table table;//=new Table();//ÄNDRAD

        public Game(string fileName)
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                string[] data = json.Split(' ');
                string[] names = JsonConvert.DeserializeObject<String[]>(data[0]);
                int[] wins = JsonConvert.DeserializeObject<int[]>(data[1]);
                this.Players = new Player[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    //this.Players[i] = new Player(names[i], wins[i]);   
                }
            }
        }
 
        public Game(string[] playerNames)//ÄNDRAD
        {
            table = new Table();
            if (playerNames.Length == 0)
            {
                throw new Exception("Inga spelarnamn angivna!");
            }
            foreach(var name in playerNames)
            {
                table.AddPlayerToTable(name);
            }
        }

        

        public void RunGame()
        {
            while (true)
            {
                NewDeal();
                table.DealTable();
                foreach (Player player in Players)
                {
                    player.SortPlayerHand();
                    SelectCardsToDiscard(player);
                    foreach (Card card in player.Discard)
                    {
                        player.DiscardCard(card);
                    }
                    table.ReplacementCards(player, player.Discard.Length);// HJÄLP
                    player.SortPlayerHand();
                    RecievedReplacementCards(player);
                    player.Hands.Eval();
                }
                ShowAllHands();
                CompareHands();
                table.CollectPlayersDiscardedCards();
                table.RebuildDeck();
            } 
        }

        public void CompareHands()
        {
            // Varje spelares handtype jämförs
            // bästa handen vinner
            // Om två eller fler spelare har samma hand, jämförs rank
            // Om två spelare har samma rank: oavgjort

            List<Player> BestHand = new List<Player>();
            HandType BestHandType = HandType.HighCard;
            foreach(Player player in Players)
            {
                if ((int)player.Hands.HandType > (int)BestHandType)
                {
                    BestHandType = player.Hands.HandType;
                    BestHand.Clear();
                }
                if ((int)player.Hands.HandType >= (int)BestHandType)
                {
                    BestHand.Add(player);
                }
            }
            if (BestHand.Count == 1)
            {
                BestHand[0].Win();
                Winner(BestHand[0]);
            }
            else if (BestHand.Count > 1)
            {
                if (BestHandType == HandType.Pair)
                {
                    BestHand = BestDuplicate(BestHand);
                }
                else if (BestHandType == HandType.HighCard || BestHandType == HandType.Straight || BestHandType == HandType.Flush || BestHandType == HandType.StraightFlush)
                {
                    BestHand = HighestRankCards(BestHand);
                }
                else if (BestHandType == HandType.TwoPairs)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Rank highestRank = BestHand.Select(player => player.Hands.DuplicateRank[i]).Max();
                        BestHand = BestHand.Where(player => player.Hands.DuplicateRank[i] == highestRank).ToList();
                        if (BestHand.Count == 1) break;
                    }
                    if (BestHand.Count > 1)
                    {
                        BestHand = HighestRankCards(BestHand);
                    }
                }
                else if (BestHandType == HandType.ThreeOfAKind || BestHandType == HandType.FullHouse)
                {
                    BestHand = BestThreeDuplicate(BestHand);
                    if (BestHand.Count > 1)
                    {
                        BestHand = BestDuplicate(BestHand);
                    }
                }
                else if (BestHandType == HandType.FourOfAKind)
                {
                    BestHand = BestFourDuplicate(BestHand);
                }
                else if (BestHandType == HandType.RoyalStraightFlush)
                {
                    Draw(BestHand.ToArray());
                }
                foreach (Player player in Players)
                {
                    if (BestHand.Count == 1)
                    {
                        BestHand[0].Win();
                        Winner(BestHand[0]);
                    }
                    else
                    {
                        Draw(BestHand.ToArray());
                    }
                }
            }
        }

        private List<Player> HighestRankCards(List<Player> players)
        {
            for (int i = 0; i < 5; i++)
            {
                Rank highest = players.Select(player => player.Hands.CardRank[i]).Max();
                players = players.Where(player => player.Hands.CardRank[i] == highest).ToList();
                if (players.Count == 1) break;
            }
            return players;
        }

        private List<Player> BestDuplicate(List<Player> players)
        {
            Rank BestDuplicate = players.Select(player => player.Hands.DuplicateRank.First()).Max();
            players = players.Where(player => player.Hands.DuplicateRank.First() == BestDuplicate).ToList();
            if (players.Count > 1)
            {
                players = HighestRankCards(players);
            }
            return players;
        }

        private List<Player> BestThreeDuplicate(List<Player> players)
        {
            Rank BestThreeDuplicate = players.Select(player => player.Hands.ThreeDuplicateRank.First()).Max();
            players = players.Where(player => player.Hands.ThreeDuplicateRank.First() == BestThreeDuplicate).ToList();
            if (players.Count > 1)
            {
                players = HighestRankCards(players);
            }
            return players;
        }
        private List<Player> BestFourDuplicate(List<Player> players)
        {
            Rank BestFourDuplicate = players.Select(player => player.Hands.FourDuplicateRank.First()).Max();
            players = players.Where(player => player.Hands.FourDuplicateRank.First() == BestFourDuplicate).ToList();
            if (players.Count > 1)
            {
                players = HighestRankCards(players);
            }
            return players;
        }

        public void SaveGameAndExit(string fileName)
        {
            string[] names;
            int[] wins;

            names = new string[Players.Length];
            wins = new int[Players.Length];

            for (int i = 0; i < Players.Length; i++)
            {
                names[i] = Players[i].Name;
                wins[i] = Players[i].Wins;
            }
     
            string json = JsonConvert.SerializeObject(names);
            json += (" " + JsonConvert.SerializeObject(wins));
            File.WriteAllText(fileName, json);
            Environment.Exit(0);
        }

        public void Exit()
        {
            Environment.Exit(0);
        }
    }
}