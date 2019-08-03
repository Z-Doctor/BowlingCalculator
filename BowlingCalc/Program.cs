using System;
using System.Collections.Generic;
using System.Linq;

namespace BowlingCalc {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to the Bowling Calculator.");
            Console.WriteLine("How many are playing?");

            Console.Write("Players: ");
            int playerCount;
            while (!int.TryParse(Console.ReadLine(), out playerCount)) {
                Console.WriteLine("You have entered an invalid Value, try again.");
                Console.WriteLine("How many are playing?");
                Console.Write("Players: ");
            }
            Console.WriteLine($"Players playing: {playerCount}");

            Console.WriteLine("You can specify player names if you'd like. Just input them sperated by a ','. Or just hit enter.");
            Console.Write("Names: ");

            BowlingGame game;
            string inputName = Console.ReadLine();
            if (string.IsNullOrEmpty(inputName))
                game = new BowlingGame(playerCount);
            else {
                game = new BowlingGame(playerCount, inputName.Split(',').Where(name => !string.IsNullOrEmpty(name)).Select(name => name.Trim()).ToArray());
            }

            Console.WriteLine("These are our Players.");
            Console.WriteLine(string.Join(", ", game.Players.Select(data => data.PlayerName)));


            Console.WriteLine("Let's Start");

            for (int round = 1; round <= BowlingGame.Total_Frames; round++) {
                Console.WriteLine($"Round {round}");
                foreach (var player in game.Players) {
                    Console.WriteLine($"Player turn: {player.PlayerName}");
                    var frame = new BowlingFrame(round == BowlingGame.Total_Frames);
                    player.Frames[round - 1] = frame;
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
                }
            }

            Console.WriteLine("The game has ended");
            Console.WriteLine("Here are the scores.");
            foreach (var player in game.Players) {
                Console.WriteLine($"{player.PlayerName} has scored {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
            }

            int highestScore = game.Players.Aggregate((winningPlayer, player) => {
                if (winningPlayer is null)
                    return player;
                if (winningPlayer.Score < player.Score)
                    return player;
                return winningPlayer;
            }).Score;

            var winners = game.Players.Where(data => data.Score == highestScore).Select(data => data).ToArray();

            if (winners.Length > 1) {
                Console.WriteLine($"{string.Join(winners.Length > 2 ? ", " : " ", winners.Select(data => { return data == winners.Last() ? "and " + data.PlayerName : data.PlayerName; }))} " +
                    $"have {(winners.Length > 2 ? "all" : "")} won with {highestScore} {(highestScore > 1 || highestScore == 0 ? "points" : "point")}");
            } else {
                var player = winners.First();
                Console.WriteLine($"{player.PlayerName} has won with a total of {player.Score} {(player.Score > 1 || player.Score == 0 ? "points" : "point")}");
            }
            Console.ReadLine();
        }

    }

    public class BowlingGame {
        public const int Total_Frames = 10;
        public const int Last_Spare_Extra_Balls = 1;
        public const int Last_Strike_Extra_Balls = 2;

        public PlayerData[] Players { get; }

        public BowlingGame(int playerCount, params string[] names) {
            Players = new PlayerData[playerCount];
            for (int i = 0; i < playerCount; i++) {
                if (names.Length > i)
                    Players[i] = new PlayerData(names[i]);
                else
                    Players[i] = new PlayerData();
            }
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

        public BowlingFrame(bool isLastFrame = false) {
            LastFrame = isLastFrame;
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
