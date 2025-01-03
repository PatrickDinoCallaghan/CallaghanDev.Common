﻿//
// DateField.cs: text entry for date
//
// Author: Barry Nolte
//
// Licensed under the MIT license
//
using System;
using System.Globalization;
using System.Linq;
using NStack;

namespace CallaghanDev.Utilities.ConsoleHelper {
	/// <summary>
	///   Simple Date editing <see cref="View"/>
	/// </summary>
	
	///   The <see cref="DateField"/> <see cref="View"/> provides date editing functionality with mouse support.
	
	public class DateField : TextField {
		DateTime date;
		bool isShort;
		int longFieldLen = 10;
		int shortFieldLen = 8;
		string sepChar;
		string longFormat;
		string shortFormat;

		int fieldLen => isShort ? shortFieldLen : longFieldLen;
		string format => isShort ? shortFormat : longFormat;

		/// <summary>
		///   DateChanged event, raised when the <see cref="Date"/> property has changed.
		/// </summary>
		
		///   This event is raised when the <see cref="Date"/> property changes.
		
		
		///   The passed event arguments containing the old value, new value, and format string.
		
		public event Action<DateTimeEventArgs<DateTime>> DateChanged;

		/// <summary>
		///    Initializes a new instance of <see cref="DateField"/> using <see cref="LayoutStyle.Absolute"/> layout.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="date">Initial date contents.</param>
		/// <param name="isShort">If true, shows only two digits for the year.</param>
		public DateField (int x, int y, DateTime date, bool isShort = false) : base (x, y, isShort ? 10 : 12, "")
		{
			Initialize (date, isShort);
		}

		/// <summary>
		///  Initializes a new instance of <see cref="DateField"/> using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		public DateField () : this (DateTime.MinValue) { }

		/// <summary>
		///  Initializes a new instance of <see cref="DateField"/> using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <param name="date"></param>
		public DateField (DateTime date) : base ("")
		{
			Width = fieldLen + 2;
			Initialize (date);
		}

		void Initialize (DateTime date, bool isShort = false)
		{
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			sepChar = cultureInfo.DateTimeFormat.DateSeparator;
			longFormat = GetLongFormat (cultureInfo.DateTimeFormat.ShortDatePattern);
			shortFormat = GetShortFormat (longFormat);
			this.isShort = isShort;
			Date = date;
			CursorPosition = 1;
			TextChanged += DateField_Changed;

			// Things this view knows how to do
			AddCommand (Command.DeleteCharRight, () => { DeleteCharRight (); return true; });
			AddCommand (Command.DeleteCharLeft, () => { DeleteCharLeft (); return true; });
			AddCommand (Command.LeftHome, () => MoveHome ());
			AddCommand (Command.Left, () => MoveLeft ());
			AddCommand (Command.RightEnd, () => MoveEnd ());
			AddCommand (Command.Right, () => MoveRight ());

			// Default keybindings for this view
			AddKeyBinding (Key.DeleteChar, Command.DeleteCharRight);
			AddKeyBinding (Key.D | Key.CtrlMask, Command.DeleteCharRight);

			AddKeyBinding (Key.Delete, Command.DeleteCharLeft);
			AddKeyBinding (Key.Backspace, Command.DeleteCharLeft);

			AddKeyBinding (Key.Home, Command.LeftHome);
			AddKeyBinding (Key.A | Key.CtrlMask, Command.LeftHome);

			AddKeyBinding (Key.CursorLeft, Command.Left);
			AddKeyBinding (Key.B | Key.CtrlMask, Command.Left);

			AddKeyBinding (Key.End, Command.RightEnd);
			AddKeyBinding (Key.E | Key.CtrlMask, Command.RightEnd);

			AddKeyBinding (Key.CursorRight, Command.Right);
			AddKeyBinding (Key.F | Key.CtrlMask, Command.Right);
		}

		void DateField_Changed (ustring e)
		{
			try {
				if (!DateTime.TryParseExact (GetDate (Text).ToString (), GetInvarianteFormat (), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
					Text = e;
			} catch (Exception) {
				Text = e;
			}
		}

		string GetInvarianteFormat ()
		{
			return $"MM{sepChar}dd{sepChar}yyyy";
		}

		string GetLongFormat (string lf)
		{
			ustring [] frm = ustring.Make (lf).Split (ustring.Make (sepChar));
			for (int i = 0; i < frm.Length; i++) {
				if (frm [i].Contains ("M") && frm [i].RuneCount < 2)
					lf = lf.Replace ("M", "MM");
				if (frm [i].Contains ("d") && frm [i].RuneCount < 2)
					lf = lf.Replace ("d", "dd");
				if (frm [i].Contains ("y") && frm [i].RuneCount < 4)
					lf = lf.Replace ("yy", "yyyy");
			}
			return $" {lf}";
		}

		string GetShortFormat (string lf)
		{
			return lf.Replace ("yyyy", "yy");
		}

		/// <summary>
		///   Gets or sets the date of the <see cref="DateField"/>.
		/// </summary>
		
		
		public DateTime Date {
			get {
				return date;
			}
			set {
				if (ReadOnly)
					return;

				var oldData = date;
				date = value;
				this.Text = value.ToString (format);
				var args = new DateTimeEventArgs<DateTime> (oldData, value, format);
				if (oldData != value) {
					OnDateChanged (args);
				}
			}
		}

		/// <summary>
		/// Get or set the date format for the widget.
		/// </summary>
		public bool IsShortFormat {
			get => isShort;
			set {
				isShort = value;
				if (isShort)
					Width = 10;
				else
					Width = 12;
				var ro = ReadOnly;
				if (ro)
					ReadOnly = false;
				SetText (Text);
				ReadOnly = ro;
				SetNeedsDisplay ();
			}
		}

		/// <inheritdoc/>
		public override int CursorPosition {
			get => base.CursorPosition;
			set {
				base.CursorPosition = System.Math.Max(System.Math.Min (value, fieldLen), 1);
			}
		}

		bool SetText (Rune key)
		{
			var text = TextModel.ToRunes (Text);
			var newText = text.GetRange (0, CursorPosition);
			newText.Add (key);
			if (CursorPosition < fieldLen)
				newText = newText.Concat (text.GetRange (CursorPosition + 1, text.Count - (CursorPosition + 1))).ToList ();
			return SetText (ustring.Make (newText));
		}

		bool SetText (ustring text)
		{
			if (text.IsEmpty) {
				return false;
			}

			ustring [] vals = text.Split (ustring.Make (sepChar));
			ustring [] frm = ustring.Make (format).Split (ustring.Make (sepChar));
			bool isValidDate = true;
			int idx = GetFormatIndex (frm, "y");
			int year = Int32.Parse (vals [idx].ToString ());
			int month;
			int day;
			idx = GetFormatIndex (frm, "M");
			if (Int32.Parse (vals [idx].ToString ()) < 1) {
				isValidDate = false;
				month = 1;
				vals [idx] = "1";
			} else if (Int32.Parse (vals [idx].ToString ()) > 12) {
				isValidDate = false;
				month = 12;
				vals [idx] = "12";
			} else
				month = Int32.Parse (vals [idx].ToString ());
			idx = GetFormatIndex (frm, "d");
			if (Int32.Parse (vals [idx].ToString ()) < 1) {
				isValidDate = false;
				day = 1;
				vals [idx] = "1";
			} else if (Int32.Parse (vals [idx].ToString ()) > 31) {
				isValidDate = false;
				day = DateTime.DaysInMonth (year, month);
				vals [idx] = day.ToString ();
			} else
				day = Int32.Parse (vals [idx].ToString ());
			string d = GetDate (month, day, year, frm);

			if (!DateTime.TryParseExact (d, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result) ||
				!isValidDate)
				return false;
			Date = result;
			return true;
		}

		string GetDate (int month, int day, int year, ustring [] fm)
		{
			string date = " ";
			for (int i = 0; i < fm.Length; i++) {
				if (fm [i].Contains ("M")) {
					date += $"{month,2:00}";
				} else if (fm [i].Contains ("d")) {
					date += $"{day,2:00}";
				} else {
					if (!isShort && year.ToString ().Length == 2) {
						var y = DateTime.Now.Year.ToString ();
						date += y.Substring (0, 2) + year.ToString ();
					} else if (isShort && year.ToString ().Length == 4) {
						date += $"{year.ToString ().Substring (2, 2)}";
					} else {
						date += $"{year,2:00}";
					}
				}
				if (i < 2)
					date += $"{sepChar}";
			}
			return date;
		}

		ustring GetDate (ustring text)
		{
			ustring [] vals = text.Split (ustring.Make (sepChar));
			ustring [] frm = ustring.Make (format).Split (ustring.Make (sepChar));
			ustring [] date = { null, null, null };

			for (int i = 0; i < frm.Length; i++) {
				if (frm [i].Contains ("M")) {
					date [0] = vals [i].TrimSpace ();
				} else if (frm [i].Contains ("d")) {
					date [1] = vals [i].TrimSpace ();
				} else {
					var year = vals [i].TrimSpace ();
					if (year.RuneCount == 2) {
						var y = DateTime.Now.Year.ToString ();
						date [2] = y.Substring (0, 2) + year.ToString ();
					} else {
						date [2] = vals [i].TrimSpace ();
					}
				}
			}
			return date [0] + ustring.Make (sepChar) + date [1] + ustring.Make (sepChar) + date [2];

		}

		int GetFormatIndex (ustring [] fm, string t)
		{
			int idx = -1;
			for (int i = 0; i < fm.Length; i++) {
				if (fm [i].Contains (t)) {
					idx = i;
					break;
				}
			}
			return idx;
		}

		void IncCursorPosition ()
		{
			if (CursorPosition == fieldLen)
				return;
			if (Text [++CursorPosition] == sepChar.ToCharArray () [0])
				CursorPosition++;
		}

		void DecCursorPosition ()
		{
			if (CursorPosition == 1)
				return;
			if (Text [--CursorPosition] == sepChar.ToCharArray () [0])
				CursorPosition--;
		}

		void AdjCursorPosition ()
		{
			if (Text [CursorPosition] == sepChar.ToCharArray () [0])
				CursorPosition++;
		}

		/// <inheritdoc/>
		public override bool ProcessKey (KeyEvent kb)
		{
			var result = InvokeKeybindings (kb);
			if (result != null)
				return (bool)result;

			// Ignore non-numeric characters.
			if (kb.Key < (Key)((int)'0') || kb.Key > (Key)((int)'9'))
				return false;

			if (ReadOnly)
				return true;

			if (SetText (TextModel.ToRunes (ustring.Make ((uint)kb.Key)).First ()))
				IncCursorPosition ();

			return true;
		}

		bool MoveRight ()
		{
			IncCursorPosition ();
			return true;
		}

		bool MoveEnd ()
		{
			CursorPosition = fieldLen;
			return true;
		}

		bool MoveLeft ()
		{
			DecCursorPosition ();
			return true;
		}

		bool MoveHome ()
		{
			// Home, C-A
			CursorPosition = 1;
			return true;
		}

		/// <inheritdoc/>
		public override void DeleteCharLeft (bool useOldCursorPos = true)
		{
			if (ReadOnly)
				return;

			SetText ('0');
			DecCursorPosition ();
			return;
		}

		/// <inheritdoc/>
		public override void DeleteCharRight ()
		{
			if (ReadOnly)
				return;

			SetText ('0');
			return;
		}

		/// <inheritdoc/>
		public override bool MouseEvent (MouseEvent ev)
		{
			if (!ev.Flags.HasFlag (MouseFlags.Button1Clicked))
				return false;
			if (!HasFocus)
				SetFocus ();

			var point = ev.X;
			if (point > fieldLen)
				point = fieldLen;
			if (point < 1)
				point = 1;
			CursorPosition = point;
			AdjCursorPosition ();
			return true;
		}

		/// <summary>
		/// Event firing method for the <see cref="DateChanged"/> event.
		/// </summary>
		/// <param name="args">Event arguments</param>
		public virtual void OnDateChanged (DateTimeEventArgs<DateTime> args)
		{
			DateChanged?.Invoke (args);
		}
	}

	/// <summary>
	/// Defines the event arguments for <see cref="DateField.DateChanged"/> and <see cref="TimeField.TimeChanged"/> events.
	/// </summary>
	public class DateTimeEventArgs<T> : EventArgs {
		/// <summary>
		/// The old <see cref="DateField"/> or <see cref="TimeField"/> value.
		/// </summary>
		public T OldValue { get; }

		/// <summary>
		/// The new <see cref="DateField"/> or <see cref="TimeField"/> value.
		/// </summary>
		public T NewValue { get; }

		/// <summary>
		/// The <see cref="DateField"/> or <see cref="TimeField"/> format.
		/// </summary>
		public string Format { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="DateTimeEventArgs{T}"/>
		/// </summary>
		/// <param name="oldValue">The old <see cref="DateField"/> or <see cref="TimeField"/> value.</param>
		/// <param name="newValue">The new <see cref="DateField"/> or <see cref="TimeField"/> value.</param>
		/// <param name="format">The <see cref="DateField"/> or <see cref="TimeField"/> format string.</param>
		public DateTimeEventArgs (T oldValue, T newValue, string format)
		{
			OldValue = oldValue;
			NewValue = newValue;
			Format = format;
		}
	}
}