using System;
using System.Collections.Generic;
using System.Linq;

namespace BowlingCalc {
    class Program {
        static void Main(string[] args) {
            WelcomeMessage();
            GetPlayerAmount(out int playerCount);
            GetPlayerOptionalNames(out string[] playerNames);
            CreateBowlingGame(out BowlingGame game, in playerCount, in playerNames);
            RunGame(in game);
            EndGame(in game);

            // User press enter to close
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void EndGame(in BowlingGame game) {
            Console.WriteLine("The game has ended");
            Console.WriteLine("Here are the scores.");
            game.ListScores();

            game.GetWinners(out PlayerData[] winners, out int highScore);

            if (winners.Length > 1) {
                Console.WriteLine($"{string.Join(winners.Length > 2 ? ", " : " ", winners.Select(data => { return data == winners.Last() ? "and " + data.PlayerName : data.PlayerName; }))} " +
                    $"have{(winners.Length > 2 ? " all" : "")} won with {highScore} {(highScore > 1 || highScore == 0 ? "points" : "point")}");
            } else {
                var player = winners.First();
                Console.WriteLine($"{player.PlayerName} has won with a total of {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
            }
        }

        private static void RunGame(in BowlingGame game) {
            while (!game.IsOver) {
                Console.WriteLine($"Round {game.Round}");
                for (int i = 0; i < game.PlayerCount; i++) {
                    var player = game.CurrentPlayer;
                    Console.WriteLine($"Player turn: {player.PlayerName}");
                    var frame = new BowlingFrame(game);
                    for (int ball = 1; ball <= BowlingFrame.Balls_Per_Frame; ball++) {
                        Console.Write($"Pins Hit: ");

                        int pinsHit;
                        while (!int.TryParse(Console.ReadLine(), out pinsHit)) {
                            Console.WriteLine("You have entered an invalid Value, try again.");
                            Console.Write($"Pins Hit: ");
                        }
                        pinsHit = Math.Min(pinsHit, BowlingFrame.Total_Pins);

                        if (frame.HitPins(pinsHit) <= 0) {
                            Console.WriteLine($"{player.PlayerName} has hit all the pins for a {frame.FrameType}");
                            if (frame.LastFrame) {
                                switch (frame.FrameType) {
                                    case FrameType.Spare:
                                        BowlingGame.ExtraSpare(player);
                                        break;
                                    case FrameType.Strike:
                                        BowlingGame.ExtraStrike(player);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        } else
                            Console.WriteLine($"{player.PlayerName} has hit {pinsHit} {(pinsHit > 1 || pinsHit == 0 ? "pins" : "pin")}. {frame.PinsLeft} " +
                                $"{(frame.PinsLeft > 1 || frame.PinsLeft == 0 ? "pins" : "pin")} remaining");
                    }
                    Console.WriteLine($"{player.PlayerName}'s turn has ended. They have scored {frame.Score} {(frame.Score > 1 || frame.Score == 0 ? "points" : "point")}" +
                        $" for a total of {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
                    game.NextPlayer();
                }
                game.NextRound();
            }
        }

        private static void CreateBowlingGame(out BowlingGame game, in int playerCount, in string[] playerNames) {
            game = new BowlingGame(playerCount, playerNames);

            Console.WriteLine("These are our Players.");
            game.PrintPlayers();
            Console.WriteLine("Let's Start");
        }

        private static void GetPlayerOptionalNames(out string[] playerNames) {
            Console.WriteLine("You can specify player names if you'd like. Just input them sperated by a ','. Or just hit enter.");
            Console.Write("Names: ");

            string inputName = Console.ReadLine();
            if (string.IsNullOrEmpty(inputName))
                playerNames = null;
            else
                playerNames = inputName.Split(',').Where(name => !string.IsNullOrEmpty(name)).Select(name => name.Trim()).ToArray();
        }

        private static void GetPlayerAmount(out int playerCount) {
            Console.WriteLine("How many are playing?");

            Console.Write("Players: ");
            while (!int.TryParse(Console.ReadLine(), out playerCount)) {
                Console.WriteLine("You have entered an invalid Value, try again.");
                Console.WriteLine("How many are playing?");
                Console.Write("Players: ");
            }
            Console.WriteLine($"Players playing: {playerCount}");
        }

        public static void WelcomeMessage() {
            Console.WriteLine("Welcome to the Bowling Calculator.");
        }

    }

    public class BowlingGame {
        public const int Total_Frames = 10;
        public const int Last_Spare_Extra_Balls = 1;
        public const int Last_Strike_Extra_Balls = 2;

        protected LinkedList<PlayerData> Players { get; }
        //public PlayerData[] Players { get; }

        public int Round { get; private set; } = 1;
        public int PlayerCount { get; }

        public bool IsOver { get => Round > Total_Frames; }

        protected LinkedListNode<PlayerData> CurrentPlayerNode { get; set; }
        public PlayerData CurrentPlayer { get => CurrentPlayerNode.Value; }

        public BowlingGame(int playerCount, params string[] names) {
            PlayerCount = playerCount;
            var playerList = new PlayerData[playerCount];
            for (int i = 0; i < playerCount; i++) {
                if (names?.Length > i)
                    playerList[i] = new PlayerData(names[i]);
                else
                    playerList[i] = new PlayerData();
            }
            Players = new LinkedList<PlayerData>(playerList);

            CurrentPlayerNode = Players.First;
        }

        public PlayerData NextPlayer() {
            CurrentPlayerNode = CurrentPlayerNode.Next;
            if (CurrentPlayerNode is null)
                CurrentPlayerNode = Players.First;
            return CurrentPlayer;
        }

        public static void ExtraSpare(PlayerData player) {
            if (Last_Spare_Extra_Balls <= 0)
                return;

            Console.WriteLine($"Since you have hit a Spare on the last frame, you have {Last_Spare_Extra_Balls} more {(Last_Spare_Extra_Balls > 1 ? "balls" : "ball")}.");

            //var frame = new BowlingFrame(true);
            for (int extra = 0; extra < Last_Spare_Extra_Balls; extra++) {
                Console.Write($"Pins Hit: ");
                int pinsHit;
                while (!int.TryParse(Console.ReadLine(), out pinsHit)) {
                    Console.WriteLine("You have entered an invalid Value, try again.");
                    Console.Write($"Pins Hit: ");
                }
                pinsHit = Math.Min(pinsHit, BowlingFrame.Total_Pins);
                Console.WriteLine($"{player.PlayerName} has hit {pinsHit} {(pinsHit > 1 || pinsHit == 0 ? "pins" : "pin")}.");
                player.ExtraScore += pinsHit;
            }
        }

        public static void ExtraStrike(PlayerData player) {
            if (Last_Strike_Extra_Balls <= 0)
                return;

            Console.WriteLine($"Since you have hit a Strike on the last frame, you have {Last_Strike_Extra_Balls} more {(Last_Strike_Extra_Balls > 1 ? "balls" : "ball")}.");
            int pinsHit;
            Bowl();

            if (pinsHit == BowlingFrame.Total_Pins) {
                Strike();
            } else {
                Console.WriteLine($"{player.PlayerName} has hit {pinsHit} {(pinsHit > 1 || pinsHit == 0 ? "pins" : "pin")}. {BowlingFrame.Total_Pins - pinsHit} " +
                    $"{(BowlingFrame.Total_Pins - pinsHit > 1 ? "pins" : "pin")} remaining.");
            }

            Bowl();
            if (pinsHit == BowlingFrame.Total_Pins)
                Strike();

            void Bowl() {
                Console.Write($"Pins Hit: ");
                while (!int.TryParse(Console.ReadLine(), out pinsHit)) {
                    Console.WriteLine("You have entered an invalid Value, try again.");
                    Console.Write($"Pins Hit: ");
                }
                pinsHit = Math.Min(pinsHit, BowlingFrame.Total_Pins);
                player.ExtraScore += pinsHit;
                return;
            }

            void Strike() {
                Console.WriteLine("Strike!");
            }

            void Score() {
                Console.WriteLine($"{player.PlayerName} has scored {pinsHit} {(pinsHit > 1 || pinsHit == 0 ? "points" : "point")} " +
                    $"for a total of {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
            }
        }

        internal void NextRound() {
            Round++;
        }

        public void PrintPlayers() {
            Console.WriteLine(string.Join(", ", Players.Select(data => data.PlayerName)));
        }

        public void ListScores() {
            foreach (var player in Players) {
                Console.WriteLine($"{player.PlayerName} has scored {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
            }
        }

        public void GetWinners(out PlayerData[] winners, out int highScore) {
            highScore = Players.Aggregate((winningPlayer, player) => {
                if (winningPlayer is null)
                    return player;
                if (winningPlayer.Score < player.Score)
                    return player;
                return winningPlayer;
            }).Score;

            int score = highScore;
            winners = Players.Where(data => data.Score == score).Select(data => data).ToArray();
        }
    }

    public class PlayerData {
        public BowlingFrame[] Frames { get; } = new BowlingFrame[BowlingGame.Total_Frames];
        internal static int UniquePlayerCount { get; private set; } = 1;

        public string PlayerName { get; }

        public int ExtraScore { get; internal set; }

        public PlayerData(string playerName = null) {
            if (string.IsNullOrEmpty(playerName))
                PlayerName = $"Player {UniquePlayerCount}";
            else
                PlayerName = playerName;
            UniquePlayerCount++;
        }

        public int Score {
            get {
                int sum = ExtraScore;
                if ((Frames?.Length ?? 0) > 0)
                    foreach (var frame in Frames) {
                        if (frame is null)
                            break;
                        sum += frame.Score;
                    }
                return sum;
            }
        }

    }

    public class BowlingFrame {
        public const int Total_Pins = 10;
        public const int Balls_Per_Frame = 2;

        public FrameType FrameType { get; private set; } = FrameType.Open;

        public int PinsLeft { get; private set; } = Total_Pins;
        public int Score { get; private set; }
        public int BallsLeft { get; private set; } = Balls_Per_Frame;

        public bool LastFrame { get; }

        public BowlingFrame(in BowlingGame game) {
            LastFrame = game.Round == BowlingGame.Total_Frames;
            game.CurrentPlayer.Frames[game.Round - 1] = this;
        }

        public int HitPins(int count, bool isFoul = false) {
            if (BallsLeft <= 0)
                return PinsLeft;
            PinsLeft = Math.Max(PinsLeft - count, 0);
            // More logic to handle in case consts change
            if (!isFoul) {
                Score = Math.Min(Score + count, Total_Pins); ;
                if (PinsLeft <= 0) {
                    if (BallsLeft == Balls_Per_Frame)
                        FrameType = FrameType.Strike;
                    else
                        FrameType = FrameType.Spare;
                    BallsLeft = 0;
                    return PinsLeft;
                }
            } else
                PinsLeft = Math.Min(PinsLeft + count, Total_Pins);
            BallsLeft--;
            return PinsLeft;
        }

    }

    public enum FrameType {
        Open, Spare, Strike
    }
}
