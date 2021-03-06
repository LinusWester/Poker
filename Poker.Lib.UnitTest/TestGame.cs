using NUnit.Framework;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Poker.Lib.UnitTest
{
    public class GameTest
    {
        private Game game;

        [SetUp]
        public void Setup()
        {
            game = new Game(new string[2] { "Test1", "Test2" });
        }

        [Test]
        public void PokerGameCreatesPlayersIfZero()
        {
            Table table = new Table();
            Game Game = new Game(new string[0] { });
            table.AddPlayerToTable("");

            Assert.AreEqual(2, game.Players.Length);
            Assert.AreEqual("player1", Game.Players[0].Name);
            Assert.AreEqual("player2", Game.Players[1].Name);
        }

        [Test]
        public void PokerGameCreatesPlayerIfOne()
        {
            Table table = new Table();
            Game Game = new Game(new string[1] { "Test1" });
            table.AddPlayerToTable("Test1");

            Assert.AreEqual(2, game.Players.Length);
            Assert.AreEqual("Test1", Game.Players[0].Name);
            Assert.AreEqual("player", Game.Players[1].Name);
        }

        [Test]
        public void GameCanLoadFile()
        {
            IPokerGame game;
            Table table = new Table();
            string fileName = "savedGame.txt";
            string[] names = new string[2] { "Test1", "Test2" };
            int[] wins = new int[2] { 7, 3 };

            string json = JsonConvert.SerializeObject(names);
            json += (" " + JsonConvert.SerializeObject(wins));
            File.WriteAllText(fileName, json);

            string Json = File.ReadAllText(fileName);
            string[] data = Json.Split(' ');
            string[] Names = JsonConvert.DeserializeObject<String[]>(data[0]);
            int[] Wins = JsonConvert.DeserializeObject<int[]>(data[1]);

            for (int i = 0; i < names.Length; i++)
            {
                table.AddPlayerToTable(names[i], wins[i]);
            }


            game = GameFactory.LoadGame(fileName);

            Assert.AreEqual(game.Players[0].Name, "Test1");
            Assert.AreEqual(game.Players[1].Name, "Test2");
            Assert.AreEqual(game.Players[0].Wins, 7);
            Assert.AreEqual(game.Players[1].Wins, 3);
        }

        [Test, Sequential]
        public void GameEventsWorkandRunGame([Values(0, 1, 2, 3, 4)] int i)
        {
            Game game = new Game(new string[2] { "Test1", "Test2" });
            int count = 0;
            bool WorkingEvents = false;

            game.NewDeal += IfNewDealWorks;
            game.SelectCardsToDiscard += IfSelectCardsToDiscardWorks;
            game.RecievedReplacementCards += IfRecievedReplacementCardsWorks;
            game.ShowAllHands += IfShowAllHandsWorks;
            game.Winner += IfWinnerWorks;
            game.Draw += IfDrawWorks;

            void IfNewDealWorks()
            {
                WorkingEvents = true;
                count++;
            }
            void IfSelectCardsToDiscardWorks(IPlayer player)
            {
                player.Discard = new ICard[] { player.Hand[i] };
                player.Discard = new ICard[] { player.Hand[i] };
                WorkingEvents = true;
                count++;
                game.GameRunning();
            }

            void IfShowAllHandsWorks()
            {
                WorkingEvents = true;
                count++;
                game.GameRunning();
            }
            void IfWinnerWorks(IPlayer player)
            {
                WorkingEvents = true;
                count++;
                game.GameRunning();
            }
            void IfRecievedReplacementCardsWorks(IPlayer player) { }
            void IfDrawWorks(IPlayer[] player) { }


            game.RunGame();
            Assert.IsTrue(WorkingEvents);
        }
        [Test,Sequential]
        public void GameCanDealTable([Values(0, 1, 2, 3, 4)] int i)
        {
            game.Table.DealTable();
            Assert.That(game.Table.Players, Has.Exactly(2).Items);
            Assert.That(game.Table.Players[0].Hand, Has.Exactly(5).Items);
            Assert.That(game.Table.Players[1].Hand, Has.Exactly(5).Items);
            Assert.IsInstanceOf<Card>(game.Table.Players[0].Hand[i]);
        }
        [Test]
        public void GameCanDiscardCards()
        {   
            Card testCard= new Card(Suite.Hearts, Rank.Ace);
            Card[] hand1 = TestsHand.ToCards("♣4♥J♠Q♥K♥A");
            foreach (Card cards in hand1)
            {
                game.Table.Players[0].ReceiveCards(cards);
            }

            Assert.That(game.Table.Players[0].Hand, Has.Exactly(5).Items);
            game.Table.DiscardCard(game.Table.Players[0],testCard );

            CollectionAssert.DoesNotContain(game.Table.Players[0].Hand, testCard);
            Assert.That(game.Table.Players[0].Hand, Has.Exactly(4).Items);
        }

        [Test]
        public void GameCanReplaceCards([Values(0, 1, 2, 3, 4)] int i)
        {
            game.Table.ReplacementCards(game.Table.Players[0], 5);
            Assert.That(game.Table.Players[0].Hand, Has.Exactly(5).Items);
            Assert.IsInstanceOf<Card>(game.Table.Players[0].Hand[i]);
        }



        [Test]
        public void GameSaveAndExitAfterSave()
        {
            string fileName = "savedGame.txt";
            IPokerGame Game;
            Game = GameFactory.NewGame(new string[2] { "Test1", "Test2" });
            game = (Game)Game;
            Assert.IsTrue(game.GameIsRunning);
            //act
            game.SaveGameAndExit(fileName);
            string Json = File.ReadAllText(fileName);
            string[] data = Json.Split(' ');
            string[] Names = JsonConvert.DeserializeObject<String[]>(data[0]);
            int[] Wins = JsonConvert.DeserializeObject<int[]>(data[1]);
            //assert
            Assert.IsFalse(game.GameIsRunning);
            Assert.AreEqual(Names[0], "Test1");
            Assert.AreEqual(Names[1], "Test2");
            Assert.AreEqual(Wins[0], 0);
            Assert.AreEqual(Wins[1], 0);
        }

        [Test]
        public void FindBestCard()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<IPlayer> players = new List<IPlayer> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣4♥J♠Q♥K♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣7♥8♠9♥Q");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            for (int i = 0; i < 5; i++)
            {
                Assert.GreaterOrEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
            }
            Assert.Greater(player1.Wins, player2.Wins);
        }

        [Test]
        public void FindBestPair()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣3♥9♠10♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣7♥8♠Q♥Q");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            game.BestDuplicate(players);
            for (int i = 0; i < 1; i++)
            {
                Assert.Greater(player1.Hands.DuplicateRank[i], player2.Hands.DuplicateRank[i]);
            }
            Assert.Greater(player1.Wins, player2.Wins);
        }

        [Test]
        public void FindBestThreeOfAKind()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣7♥9♣A♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣7♣Q♠Q♥Q");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            game.BestThreeDuplicate(players);
            for (int i = 0; i < 1; i++)
            {
                Assert.Greater(player1.Hands.ThreeDuplicateRank[i], player2.Hands.ThreeDuplicateRank[i]);
            }
        }

        [Test]
        public void IfTiedHighCard()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣4♥J♠Q♥K♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣7♥8♠K♠A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            game.HighestRankCards(players);
            for (int i = 0; i < 5; i++)
            {
                Assert.GreaterOrEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
            }
            Assert.Greater(player1.Wins, player2.Wins);
        }

        [Test]
        public void IfTiedPair()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣3♥9♠10♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥2♣7♥8♣A♦A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }

            game.BestDuplicate(players);
            for (int i = 0; i < 1; i++)
            {
                Assert.GreaterOrEqual(player1.Hands.DuplicateRank[i], player2.Hands.DuplicateRank[i]);
                Assert.GreaterOrEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
            }
            game.CompareHands(Players);
            Assert.Greater(player1.Wins, player2.Wins);
        }

        [Test]
        public void IfTiedTwoPairs()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣3♥9♠9♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣9♦9♣A♦A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }

            game.BestDuplicate(players);
            for (int i = 0; i < 1; i++)
            {
                Assert.GreaterOrEqual(player2.Hands.DuplicateRank[i], player1.Hands.DuplicateRank[i]);
                Assert.GreaterOrEqual(player2.Hands.CardRank[i], player1.Hands.CardRank[i]);
            }
            game.CompareHands(Players);
            Assert.Greater(player2.Wins, player1.Wins);
        }

        [Test]
        public void IfOnlyHighPairTiedTwoPairs()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣3♥10♠10♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥4♣9♦9♣A♦A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }

            foreach (Player player in players)
            {
                player.Hands.Eval();
            }

            game.BestDuplicate(players);
            for (int i = 0; i < 1; i++)
            {
                Assert.GreaterOrEqual(player1.Hands.DuplicateRank[i], player2.Hands.DuplicateRank[i]);
            }
            game.CompareHands(Players);
            Assert.Greater(player1.Wins, player2.Wins);
        }

        [Test]
        public void IfMoreThanOneStraight()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣9♥10♦J♠Q♥K");
            Card[] hand2 = TestsHand.ToCards("♥3♣4♦5♣6♠7");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.Greater(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
                }
                Assert.Greater(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfTiedStraight()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣9♥10♦J♠Q♥K");
            Card[] hand2 = TestsHand.ToCards("♥9♣10♠J♣Q♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
                }
                Assert.AreEqual(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfMoreThanOneFlush()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♥4♥10♥J♥Q♥K");
            Card[] hand2 = TestsHand.ToCards("♠2♠3♠J♠Q♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.GreaterOrEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
                }
                Assert.Greater(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfTiedFlush()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♥4♥10♥J♥Q♥K");
            Card[] hand2 = TestsHand.ToCards("♠4♠10♠J♠Q♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(player1.Hands.CardRank[i], player2.Hands.CardRank[i]);
                }
                Assert.AreEqual(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfMoreThanOneFullHouse()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣10♥10♦A♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥J♣J♦K♣K♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                game.CompareHands(Players);
                for (int i = 0; i < 1; i++)
                {
                    Assert.Greater(player1.Hands.ThreeDuplicateRank[i], player2.Hands.ThreeDuplicateRank[i]);
                }
                Assert.Greater(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfMoreThanOneFourOfAKind()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♣10♣A♦A♠A♥A");
            Card[] hand2 = TestsHand.ToCards("♥J♥K♦K♣K♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                game.CompareHands(Players);
                for (int i = 0; i < 1; i++)
                {
                    Assert.Greater(player1.Hands.FourDuplicateRank[i], player2.Hands.FourDuplicateRank[i]);
                }
                Assert.Greater(player1.Wins, player2.Wins);
            }
        }

        [Test]
        public void IfMoreThanOneStraightFlush()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♥7♥8♥9♥10♥J");
            Card[] hand2 = TestsHand.ToCards("♠9♠10♠J♠Q♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.Greater(player2.Hands.CardRank[i], player1.Hands.CardRank[i]);
                }
                Assert.Greater(player2.Wins, player1.Wins);
            }
        }

        [Test]
        public void IfTiedStraightFlush()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♥9♥10♥J♥Q♥K");
            Card[] hand2 = TestsHand.ToCards("♠9♠10♠J♠Q♠K");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(player2.Hands.CardRank[i], player1.Hands.CardRank[i]);
                }
                Assert.AreEqual(player2.Wins, player1.Wins);
            }
        }

        [Test]
        public void IfTiedRoyalStraightFlush()
        {
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            IPlayer[] Players = { player1, player2 };
            List<Player> players = new List<Player> { player1, player2 };
            Card[] hand1 = TestsHand.ToCards("♥10♥J♥Q♥K♥A");
            Card[] hand2 = TestsHand.ToCards("♠10♠J♠Q♠K♠A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }
            game.CompareHands(Players);
            if (player1.Hands.HandType.Equals(player2.Hands.HandType))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(player2.Hands.CardRank[i], player1.Hands.CardRank[i]);
                }
                Assert.AreEqual(player2.Wins, player1.Wins);
            }
        }

        [Test]
        public void GameKnowsRanksOfHandTypesAndWinner()
        {
            Game Game = new Game(new string[5] { "Test1", "Test2", "Test3", "Test4", "Test5" });
            Player player1 = new Player("Test1", 0);
            Player player2 = new Player("Test2", 0);
            Player player3 = new Player("Test3", 0);
            Player player4 = new Player("Test4", 0);
            Player player5 = new Player("Test5", 0);
            IPlayer[] Players = { player1, player2, player3, player4, player5 };
            List<Player> players = new List<Player> { player1, player2, player3, player4, player5 };
            Card[] hand1 = TestsHand.ToCards("♥10♥J♥Q♥K♥A");
            Card[] hand2 = TestsHand.ToCards("♠9♠10♠J♠Q♠K");
            Card[] hand3 = TestsHand.ToCards("♠2♣5♦5♦5♠5");
            Card[] hand4 = TestsHand.ToCards("♠3♦3♦6♠6♥6");
            Card[] hand5 = TestsHand.ToCards("♥2♠4♠7♠8♠A");
            foreach (Card cards in hand1)
            {
                player1.ReceiveCards(cards);
            }
            foreach (Card cards in hand2)
            {
                player2.ReceiveCards(cards);
            }
            foreach (Card cards in hand3)
            {
                player3.ReceiveCards(cards);
            }
            foreach (Card cards in hand4)
            {
                player4.ReceiveCards(cards);
            }
            foreach (Card cards in hand5)
            {
                player5.ReceiveCards(cards);
            }
            foreach (Player player in players)
            {
                player.Hands.Eval();
            }

            Game.CompareHands(Players);
            Assert.Greater(player1.Hands.HandType, player2.Hands.HandType);
            Assert.Greater(player2.Hands.HandType, player3.Hands.HandType);
            Assert.Greater(player3.Hands.HandType, player4.Hands.HandType);
            Assert.Greater(player4.Hands.HandType, player5.Hands.HandType);

            Assert.Greater(player1.Wins, player2.Wins);
            Assert.Greater(player1.Wins, player3.Wins);
            Assert.Greater(player1.Wins, player4.Wins);
            Assert.Greater(player1.Wins, player5.Wins);

            Assert.AreEqual(player2.Wins, player3.Wins);
            Assert.AreEqual(player3.Wins, player4.Wins);
            Assert.AreEqual(player4.Wins, player5.Wins);
        }

        [Test]
        public void CanExitGame()
        {
            IPokerGame Game;
            Game = GameFactory.NewGame(new string[2] { "Test1", "Test2" });
            game = (Game)Game;
            Assert.IsTrue(game.GameIsRunning);
            //act
            game.Exit();
            //assert
            Assert.IsFalse(game.GameIsRunning);
        }
    }
}