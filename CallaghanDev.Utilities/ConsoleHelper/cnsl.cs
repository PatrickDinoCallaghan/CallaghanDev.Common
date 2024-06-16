using OpenQA.Selenium.DevTools.V121.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.ConsoleHelper
{
    public static class cnsl
    {
        private const int BAR_LENGTH = 50;
        public static void DisplayProgressBar(int currentIteration, int totalIterations, string Title = "")
        {
            cmslProgressBar.DisplayProgressBar(currentIteration, totalIterations, Title);
        }
        public static void DisplayProgressBar(long currentIteration, long totalIterations, string Title = "")
        {
            cmslProgressBar.DisplayProgressBar(currentIteration, totalIterations, Title);
        }
        public static bool AskYesNoQuestion(string question)
        {
            // Keep asking until a valid input is received
            while (true) 
            {

                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.BackgroundColor = ConsoleColor.Green;
                System.Console.WriteLine(question + " (yes/no):");

                System.Console.ResetColor();
                string? response = System.Console.ReadLine()?.Trim().ToLower();

                if (response == "yes" || response == "y")
                    return true;
                else if (response == "no" || response == "n")

                    return false;
                else
                    System.Console.WriteLine("Invalid input. Please answer with 'yes' or 'no'.");
            }
        }
        public static void Exit()
        {
            System.Console.WriteLine("Press any key to exit."); System.Console.ReadKey();
        }
        public static string AskForFolder(string Question = "Please enter the path to the folder: ")
        {
            string? folderPath = string.Empty;

            while (true)
            {
                cnsl.Write(Question);
                folderPath = ReadLine();

                if (Directory.Exists(folderPath))
                {
                    cnsl.WriteLine("Valid directory detected.");
                    break;
                }
                else
                {
                    cnsl.WriteLine("Directory does not exist. Please try again.");
                }
            }

            return folderPath;
        }
        public static string AskForFile(string Question = "Please enter the path to the file: ")
        {
            string? filePath = string.Empty;

            while (true)
            {
                cnsl.Write(Question);
                filePath = cnsl.ReadLine();

                if (File.Exists(filePath))
                {
                    cnsl.WriteLine("Valid file detected.");
                    break;
                }
                else
                {
                    cnsl.WriteLine("File does not exist. Please try again.");
                }
            }

            return filePath;
        }
        public static int SelectOptionFromList(List<string> options)
        {
            while (true)
            {
                cnsl.WriteLine("Please select an option:");
                for (int i = 0; i < options.Count; i++)
                {
                    cnsl.WriteLine($"{i + 1}. {options[i]}");
                }

                cnsl.Write("Enter the number of your choice: ");
                if (int.TryParse(cnsl.ReadLine(), out int choice) && choice > 0 && choice <= options.Count)
                {
                    return choice - 1;
                }

                cnsl.WriteLine("Invalid input, please try again.");
            }
        }
        #region Process Spinner

        static bool _spinnerActive = false;
        public static void StartSpinner(string title = "")
        {
            _spinnerActive = true;
            var spinnerChars = new char[] { '/', '-', '\\', '|' };
            int counter = 0;

            cnsl.Write(title + " "); // Display the title

            new System.Threading.Thread(() =>
            {
                while (_spinnerActive)
                {
                    cnsl.Write(spinnerChars[counter++ % spinnerChars.Length]);
                    System.Console.SetCursorPosition((System.Console.CursorLeft - 1 > 1) ? System.Console.CursorLeft - 1 :0 , System.Console.CursorTop);
                    System.Threading.Thread.Sleep(100);
                }
            }).Start();
        }

        public static void StopSpinner()
        {
            _spinnerActive = false;
        }
        #endregion 

        #region System.Console Methods

        public static ConsoleColor BackgroundColor
        {
            get { return System.Console.BackgroundColor; }
            set { System.Console.BackgroundColor = value; }
        }

        public static ConsoleColor ForegroundColor
        {
            get { return System.Console.ForegroundColor; }
            set { System.Console.ForegroundColor = value; }
        }

        public static int Read()
        {
            return System.Console.Read();
        }

        public static string? ReadLine()
        {
            return System.Console.ReadLine();
        }

        public static void WriteLine()
        {
            System.Console.WriteLine();
        }

        public static void WriteLine(bool value)
        {
            System.Console.WriteLine(value);
        }

        public static void WriteLine(char value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(char[]? buffer)
        {
            System.Console.WriteLine(buffer);
        }


        public static void WriteLine(char[] buffer, int index, int count)
        {
            System.Console.WriteLine(buffer, index, count);
        }


        public static void WriteLine(decimal value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(double value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(float value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(int value)
        {
            System.Console.WriteLine(value);
        }

        public static void WriteLine(uint value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(long value)
        {
            System.Console.WriteLine(value);
        }

        public static void WriteLine(ulong value)
        {
            System.Console.WriteLine(value);
        }

        public static void WriteLine(object? value)
        {
            System.Console.WriteLine(value);
        }

        public static void WriteLine(string? value)
        {
            System.Console.WriteLine(value);
        }


        public static void WriteLine(string format, object? arg0)
        {
            System.Console.WriteLine(format, arg0);
        }


        public static void WriteLine(string format, object? arg0, object? arg1)
        {
            System.Console.WriteLine(format, arg0, arg1);
        }


        public static void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            System.Console.WriteLine(format, arg0, arg1, arg2);
        }


        public static void WriteLine(string format, params object?[]? arg)
        {
            if (arg == null)                       // avoid ArgumentNullException from String.Format
                System.Console.WriteLine(format, null, null); // faster than System.Console.WriteLine(format, (Object)arg);
            else
                System.Console.WriteLine(format, arg);
        }


        public static void Write(string format, object? arg0)
        {
            System.Console.Write(format, arg0);
        }


        public static void Write(string format, object? arg0, object? arg1)
        {
            System.Console.Write(format, arg0, arg1);
        }


        public static void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            System.Console.Write(format, arg0, arg1, arg2);
        }


        public static void Write(string format, params object?[]? arg)
        {
            if (arg == null)                   // avoid ArgumentNullException from String.Format
                System.Console.Write(format, null, null); // faster than System.Console.Write(format, (Object)arg);
            else
                System.Console.Write(format, arg);
        }


        public static void Write(bool value)
        {
            System.Console.Write(value);
        }


        public static void Write(char value)
        {
            System.Console.Write(value);
        }


        public static void Write(char[]? buffer)
        {
            System.Console.Write(buffer);
        }


        public static void Write(char[] buffer, int index, int count)
        {
            System.Console.Write(buffer, index, count);
        }


        public static void Write(double value)
        {
            System.Console.Write(value);
        }


        public static void Write(decimal value)
        {
            System.Console.Write(value);
        }


        public static void Write(float value)
        {
            System.Console.Write(value);
        }


        public static void Write(int value)
        {
            System.Console.Write(value);
        }

        public static void Write(uint value)
        {
            System.Console.Write(value);
        }


        public static void Write(long value)
        {
            System.Console.Write(value);
        }

        public static void Write(ulong value)
        {
            System.Console.Write(value);
        }

        public static void Write(object? value)
        {
            System.Console.Write(value);
        }

        public static void Write(string? value)
        {
            System.Console.Write(value);
        }
        #endregion

        public static Tuple<Nullable<DateTime>, Nullable<DateTime>> DateTimeRangeSelection(string Title = "DateTime Range")
        {
            Nullable<DateTime> Start = null;
            Nullable<DateTime> End = null;
            CallaghanDev.Utilities.ConsoleHelper.Application.Init();
            var top = CallaghanDev.Utilities.ConsoleHelper.Application.Top;

            var win = new Window(Title)
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            // FrameView 1
            var frameView1 = new FrameView("Start Range")
            {
                X = 0,
                Y = 0,
                Width = 25,
                Height = 7
            };

            var timePicker = new TimeField()
            {
                X = 1, // Relative to GroupBox
                Y = 1,
                Width = 20
            };
            frameView1.Add(timePicker);

            var datePicker = new CallaghanDev.Utilities.ConsoleHelper.DateField(DateTime.Now)
            {
                X = 1,
                Y = 3,
                Width = 20
            };
            frameView1.Add(datePicker);

            var frameView2 = new FrameView("End Range")
            {
                X = Pos.Right(frameView1) + 2,
                Y = 0,
                Width = 25,
                Height = 7
            };
            var timePicker2 = new CallaghanDev.Utilities.ConsoleHelper.TimeField()
            {
                X = 1, // Relative to GroupBox
                Y = 1,
                Width = 20
            };
            frameView2.Add(timePicker2);

            var datePicker2 = new CallaghanDev.Utilities.ConsoleHelper.DateField(DateTime.Now)
            {
                X = 1,
                Y = 3,
                Width = 20
            };
            frameView2.Add(datePicker2);

            win.Add(frameView1, frameView2);

            var okButton = new CallaghanDev.Utilities.ConsoleHelper.Button("OK")
            {
                X = Pos.Left(win),
                Y = 7, // Position below the date picker
            };
            okButton.Clicked += () => {
                Start = new DateTime(
                    datePicker.Date.Year,
                    datePicker.Date.Month,
                    datePicker.Date.Day,
                    timePicker.Time.Hours,
                    timePicker.Time.Minutes,
                    timePicker.Time.Seconds
                );

                End = new DateTime(
                    datePicker2.Date.Year,
                    datePicker2.Date.Month,
                    datePicker2.Date.Day,
                    timePicker2.Time.Hours,
                    timePicker2.Time.Minutes,
                    timePicker2.Time.Seconds
                );
                if (Start >= End)
                {
                    CallaghanDev.Utilities.ConsoleHelper.Application.RequestStop();
                }
                else
                {
                    var ok = MessageBox.Query(50, 7, "Range Selection Error", "The start date should be earlier than the end date. Please adjust your selection and try again.", "Ok");
                }

            };


            win.Add(okButton);

            timePicker2.Time = TimeSpan.Zero;
            timePicker.Time = TimeSpan.Zero;

            CallaghanDev.Utilities.ConsoleHelper.Application.Run();
            CallaghanDev.Utilities.ConsoleHelper.Application.Shutdown();
            Console.Clear();

            return Tuple.Create(Start, End);

        }

    }
}