using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

internal class CmslProgressBar
{
    private static readonly Dictionary<string, Tuple<Stopwatch, CmslProgressBar>> ProgressBars = new();
    private static int BarLength = 50;
    private static readonly bool Silent = false; // Assuming you have a mechanism to set this value
    private static int maxLength = 0;
    public bool TitleLineUsed { get; set; } = false;

    public static void DisplayProgressBar(long currentIteration, long totalIterations, string title = "")
    {
        if (Silent || totalIterations <= 0) return;

        if (!ProgressBars.ContainsKey(title))
        {
            var stopwatch = new Stopwatch();
            var progressBar = new CmslProgressBar();
            stopwatch.Start();
            ProgressBars.Add(title, new Tuple<Stopwatch, CmslProgressBar>(stopwatch, progressBar));
        }

        double progress = Math.Min((double)currentIteration / totalIterations, 1);
        int charsToPrint = (int)(progress * BarLength);
        string timeEstimation = GetTimeEstimation(progress, title);

        Console.ForegroundColor = progress < 1 ? ConsoleColor.Red : ConsoleColor.Green;

        PrintProgressBar(progress, title, charsToPrint, timeEstimation);

        if (progress >= 1)
        {
            if (ProgressBars[title].Item1.IsRunning)
            {
                PrintProgressBarFinished("Time Elapsed:" + PrintEffectiveTimeSpan(ProgressBars[title].Item1.Elapsed));
            }
            ProgressBars[title].Item1.Stop();
            ProgressBars.Remove(title);

            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static string PrintEffectiveTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";

        return $"{(int)timeSpan.TotalSeconds}s {timeSpan.Milliseconds}ms";
    }

    private static string GetTimeEstimation(double progress, string title)
    {
        if (progress <= 0) return string.Empty;

        double elapsedSeconds = ProgressBars[title].Item1.Elapsed.TotalSeconds;
        double estimatedTotalTimeSeconds = elapsedSeconds / progress;
        double estimatedRemainingTimeSeconds = estimatedTotalTimeSeconds - elapsedSeconds;
        TimeSpan estimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingTimeSeconds);

        return $" ETA: {FormatTimeSpan(estimatedRemainingTime)}";
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        string eta = string.Empty;

        if (timeSpan.TotalDays >= 1)
            eta += $"{(int)timeSpan.TotalDays}d ";
        if (timeSpan.TotalHours >= 1)
            eta += $"{timeSpan.Hours}h ";
        if (timeSpan.TotalMinutes >= 1)
            eta += $"{timeSpan.Minutes}m ";

        eta += $"{timeSpan.Seconds}s";
        return eta;
    }

    private static void PrintProgressBar(double progress, string title, int charsToPrint, string timeEstimation)
    {
        string printTitle = ProgressBars[title].Item2.TitleLineUsed ? "" : TitleizeFirstLetter(title);

        string progressString = $"{printTitle}\r[" + new string('#', charsToPrint) + new string(' ', BarLength - charsToPrint) + $"] {progress * 100:0.00}% {timeEstimation}";
        int length = progressString.Length;

        if (maxLength < length || Console.WindowWidth < maxLength)
        {
            maxLength = Console.WindowWidth - length - 5;
        }


        Console.Write(progressString + new string(' ', maxLength));
        Console.SetCursorPosition(length, Console.GetCursorPosition().Top);
    }

    private static void PrintProgressBarFinished(string timeTaken)
    {
        Console.Write($" {timeTaken}\r[");
    }

    private static string TitleizeFirstLetter(string input)
    {
        int firstLetterIndex = input.IndexOf(input.FirstOrDefault(char.IsLetter));
        string alphanumeric = Regex.Replace(input.Substring(firstLetterIndex), @"^[^a-zA-Z\d]*", "");

        if (!string.IsNullOrEmpty(alphanumeric))
        {
            alphanumeric = char.ToUpper(alphanumeric[0]) + alphanumeric.Substring(1);
        }

        return alphanumeric;
    }

    public static void SetBarLength (int Length)
    {
        BarLength = Length + 2;
    }
}
