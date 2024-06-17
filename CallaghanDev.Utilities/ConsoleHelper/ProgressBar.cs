using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

internal class cmslProgressBar
{
    private static Dictionary<string, Tuple<Stopwatch, cmslProgressBar>> _ProgressBars = new Dictionary<string, Tuple<Stopwatch, cmslProgressBar>>();
    private const int BAR_LENGTH = 50;
    private static bool Silent = false; // Assuming you have a mechanism to set this value

    public bool TitleLineUsed { get; set; } = false;

    public static void DisplayProgressBar(long currentIteration, long totalIterations, string title = "")
    {
        if (Silent) return;

        if (!_ProgressBars.ContainsKey(title))
        {
            var stopwatch = new Stopwatch();
            var progressBar = new cmslProgressBar();
            stopwatch.Start();
            _ProgressBars.Add(title, new Tuple<Stopwatch, cmslProgressBar>(stopwatch, progressBar));
        }

        if (totalIterations <= 0) return;

        double progress = (double)currentIteration / totalIterations;
        int charsToPrint = (int)(progress * BAR_LENGTH);
        progress = Math.Min(progress, 1);

        string timeEstimation = GetTimeEstimation(progress, title);

        Console.ForegroundColor = progress < 1 ? ConsoleColor.Red : ConsoleColor.Green;

        if (progress < 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            UpdateTitleLineIfNeeded(title);
            Console.ForegroundColor = ConsoleColor.Green;
        }

        PrintProgressBar(progress, title, charsToPrint, timeEstimation);

        if (progress == 1)
        {
            _ProgressBars[title].Item1.Stop();
            _ProgressBars.Remove(title);
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static string GetTimeEstimation(double progress, string title)
    {
        if (progress <= 0) return string.Empty;

        double elapsedSeconds = _ProgressBars[title].Item1.Elapsed.TotalSeconds;
        double estimatedTotalTimeSeconds = elapsedSeconds / progress;
        double estimatedRemainingTimeSeconds = estimatedTotalTimeSeconds - elapsedSeconds;
        TimeSpan estimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingTimeSeconds);

        string eta = " ETA: ";

        if (estimatedRemainingTime.TotalDays >= 1)
        {
            eta += $"{(int)estimatedRemainingTime.TotalDays}d ";
        }
        if (estimatedRemainingTime.TotalHours >= 1)
        {
            eta += $"{estimatedRemainingTime.Hours}h ";
        }
        if (estimatedRemainingTime.TotalMinutes >= 1)
        {
            eta += $"{estimatedRemainingTime.Minutes}m ";
        }
        eta += $"{estimatedRemainingTime.Seconds}s";

        return eta;
    }

    private static void UpdateTitleLineIfNeeded(string title)
    {
        if (_ProgressBars[title].Item2.TitleLineUsed)
        {
            Console.CursorTop = Console.CursorTop - 1;
            Console.CursorLeft = 0;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(TitleizeFirstLetter(title));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        if (!_ProgressBars[title].Item2.TitleLineUsed)
        {
            Console.WriteLine(TitleizeFirstLetter(title));
            _ProgressBars[title].Item2.TitleLineUsed = true;
        }
    }

    private static void PrintProgressBar(double progress, string title, int charsToPrint, string timeEstimation)
    {
        string printTitle = _ProgressBars[title].Item2.TitleLineUsed ? "" : TitleizeFirstLetter(title);

        Console.Write($"{printTitle}\r[");
        Console.Write(new string('#', charsToPrint));
        Console.Write(new string(' ', BAR_LENGTH - charsToPrint));
        Console.Write($"] {progress * 100:0.00}% {timeEstimation}");
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
}
