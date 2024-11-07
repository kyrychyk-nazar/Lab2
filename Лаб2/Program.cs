using System;
using System.Collections.Generic;

namespace Лаб1
{
    public abstract class BaseGame
    {
        public string OpponentName { get; set; }
        public Guid GameId { get; private set; }
        public bool IsWin { get; set; }

        protected BaseGame(string opponentName, bool isWin)
        {
            OpponentName = opponentName;
            GameId = Guid.NewGuid();
            IsWin = isWin;
        }

        public abstract int CalculateRating();
    }

    public class StandardGame : BaseGame
    {
        public int Rating { get; private set; }

        public StandardGame(string opponentName, int rating, bool isWin) : base(opponentName, isWin)
        {
            if (rating <= 0)
                throw new ArgumentException("Рейтинг повинен бути вище 0");

            Rating = rating;
        }

        public override int CalculateRating()
        {
            return Rating;
        }
    }

    public class TrainingGame : BaseGame
    {
        public TrainingGame(string opponentName) : base(opponentName, false) {}

        public override int CalculateRating()
        {
            return 0;
        }
    }

    public class GameFactory
    {
        public static BaseGame CreateStandardGame(string opponentName, int rating, bool isWin)
        {
            return new StandardGame(opponentName, rating, isWin);
        }

        public static BaseGame CreateTrainingGame(string opponentName)
        {
            return new TrainingGame(opponentName);
        }
    }
    
    public abstract class GameAccount
    {
        public string Username { get; set; }
        public int CurrentRating { get; set; }
        public int GameCounts { get; private set; }

        protected List<BaseGame> gamesHistory = new List<BaseGame>();

        protected GameAccount(string username, int initialRating)
        {
            if (initialRating < 1)
                throw new ArgumentException("Рейтинг має бути хоча б 1.");

            Username = username;
            CurrentRating = initialRating;
            GameCounts = 0;
        }

        public void WinGame(BaseGame game)
        {
            int rating = game.CalculateRating();
            CurrentRating += rating;
            GameCounts++;
            gamesHistory.Add(game);
            Console.WriteLine($"Win against {game.OpponentName} with {rating}. Current rating: {CurrentRating}");
        }

        public abstract void LoseGame(BaseGame game);

        public void PlayTrainingGame(string opponentName)
        {
            var game = (TrainingGame)GameFactory.CreateTrainingGame(opponentName);
            gamesHistory.Add(game);
            Console.WriteLine($"Training game against {opponentName}. No rating change.");
        }

        protected void PrintLoseMessage(string opponentName, int rating)
        {
            Console.WriteLine($"Lose against {opponentName} with {rating}. Current rating: {CurrentRating}");
        }

        public void GetStats()
        {
            Console.WriteLine($"\nСтатистика ігор гравця {Username}:");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("| Противник     | Результат  | Рейтинг | Game ID      |");
            Console.WriteLine("-------------------------------------------------");

            foreach (var game in gamesHistory)
            {
                string result = game.IsWin ? "Win" : "Lose";
                Console.WriteLine($"| {game.OpponentName,-12} | {result,-7} | {game.CalculateRating(),-6} | {game.GameId} |");
            }
            Console.WriteLine("-------------------------------------------------");
        }
    }

    public class PayToWin : GameAccount
    {
        public PayToWin(string username, int initialRating) : base(username, initialRating * 2) {}

        public override void LoseGame(BaseGame game)
        {
            int rating = game.CalculateRating() / 2;
            if (CurrentRating - rating < 1)
            {
                Console.WriteLine("Рейтинг не може бути менше одного, ваш рейтинг залишається поточним.");
                rating = 0;
            }
            else
            {
                CurrentRating -= rating;
            }

            gamesHistory.Add(game);
            PrintLoseMessage(game.OpponentName, rating);
        }
    }

    public class StandartAcc : GameAccount
    {
        public StandartAcc(string username, int initialRating) : base(username, initialRating) {}

        public override void LoseGame(BaseGame game)
        {
            int rating = game.CalculateRating();
            if (CurrentRating - rating < 1)
            {
                Console.WriteLine("Рейтинг не може бути менше одного, ваш рейтинг залишається поточним.");
                rating = 0;
            }
            else
            {
                CurrentRating -= rating;
            }

            gamesHistory.Add(game);
            PrintLoseMessage(game.OpponentName, rating);
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            GameAccount player1 = new StandartAcc("Player1", 100);
            GameAccount player2 = new PayToWin("Player2", 100);
            GameAccount player3 = new StandartAcc("Player3", 100);

            var trainingGame = GameFactory.CreateTrainingGame("Player2");
            player1.PlayTrainingGame(trainingGame.OpponentName);

            var loseGame1 = GameFactory.CreateStandardGame("Player3", 25, false);
            player1.LoseGame(loseGame1);

            var loseGame2 = GameFactory.CreateStandardGame("Player1", 25, false);
            player2.LoseGame(loseGame2);

            var winGame = GameFactory.CreateStandardGame("Player3", 25, true);
            player2.WinGame(winGame);

            var loseGame3 = GameFactory.CreateStandardGame("Player1", 25, false);
            player3.LoseGame(loseGame3);

            var loseGame4 = GameFactory.CreateStandardGame("Player2", 25, false);
            player3.LoseGame(loseGame4);

            player1.GetStats();
            player2.GetStats();
            player3.GetStats();
        }
    }
}