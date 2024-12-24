//
// System.Drawing.Rectangle.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com 
//

using System;

namespace CallaghanDev.Utilities.ConsoleHelper
{
	/// <summary>
	/// Stores a set of four integers that represent the location and size of a rectangle
	/// </summary>
	public struct Rect
	{
		int width;
		int height;

		/// <summary>
		/// Gets or sets the x-coordinate of the upper-left corner of this Rectangle structure.
		/// </summary>
		public int X;
		/// <summary>
		/// Gets or sets the y-coordinate of the upper-left corner of this Rectangle structure.
		/// </summary>
		public int Y;

		/// <summary>
		/// Gets or sets the width of this Rect structure.
		/// </summary>
		public int Width {
			get { return width; }
			set {
				if (value < 0)
					throw new ArgumentException ("Width must be greater or equal to 0.");
				width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of this Rectangle structure.
		/// </summary>
		public int Height {
			get { return height; }
			set {
				if (value < 0)
					throw new ArgumentException ("Height must be greater or equal to 0.");
				height = value;
			}
		}

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		
		///	An uninitialized Rectangle Structure.
		

		public static readonly Rect Empty;

		/// <summary>
		///	FromLTRB Shared Method
		/// </summary>
		///
		
		///	Produces a Rectangle structure from left, top, right
		///	and bottom coordinates.
		

		public static Rect FromLTRB (int left, int top,
						  int right, int bottom)
		{
			return new Rect (left, top, right - left,
					      bottom - top);
		}

		/// <summary>
		///	Inflate Shared Method
		/// </summary>
		///
		
		///	Produces a new Rectangle by inflating an existing 
		///	Rectangle by the specified coordinate values.
		

		public static Rect Inflate (Rect rect, int x, int y)
		{
			Rect r = new Rect (rect.Location, rect.Size);
			r.Inflate (x, y);
			return r;
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		
		///	Inflates the Rectangle by a specified width and height.
		

		public void Inflate (int width, int height)
		{
			Inflate (new Size (width, height));
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		
		///	Inflates the Rectangle by a specified Size.
		

		public void Inflate (Size size)
		{
			X -= size.Width;
			Y -= size.Height;
			Width += size.Width * 2;
			Height += size.Height * 2;
		}

		/// <summary>
		///	Intersect Shared Method
		/// </summary>
		///
		
		///	Produces a new Rectangle by intersecting 2 existing 
		///	Rectangles. Returns null if there is no	intersection.
		

		public static Rect Intersect (Rect a, Rect b)
		{
			// MS.NET returns a non-empty rectangle if the two rectangles
			// touch each other
			if (!a.IntersectsWithInclusive (b))
				return Empty;

			return Rect.FromLTRB (
                 System.Math.Max (a.Left, b.Left),
                 System.Math.Max (a.Top, b.Top),
                 System.Math.Min (a.Right, b.Right),
                 System.Math.Min (a.Bottom, b.Bottom));
		}

		/// <summary>
		///	Intersect Method
		/// </summary>
		///
		
		///	Replaces the Rectangle with the intersection of itself
		///	and another Rectangle.
		

		public void Intersect (Rect rect)
		{
			this = Rect.Intersect (this, rect);
		}

		/// <summary>
		///	Union Shared Method
		/// </summary>
		///
		
		///	Produces a new Rectangle from the union of 2 existing 
		///	Rectangles.
		

		public static Rect Union (Rect a, Rect b)
		{
			return FromLTRB(System.Math.Min (a.Left, b.Left),
                     System.Math.Min (a.Top, b.Top),
                      System.Math.Max (a.Right, b.Right),
                     System.Math.Max (a.Bottom, b.Bottom));
		}

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		
		///	Compares two Rectangle objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two Rectangles.
		

		public static bool operator == (Rect left, Rect right)
		{
			return ((left.Location == right.Location) && 
				(left.Size == right.Size));
		}

		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		
		///	Compares two Rectangle objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two Rectangles.
		

		public static bool operator != (Rect left, Rect right)
		{
			return ((left.Location != right.Location) || 
				(left.Size != right.Size));
		}

		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	Rectangle Constructor
		/// </summary>
		///
		
		///	Creates a Rectangle from Point and Size values.
		

		public Rect (Point location, Size size)
		{
			X = location.X;
			Y = location.Y;
			width = size.Width;
			height = size.Height;
			Width = width;
			Height = height;
		}

		/// <summary>
		///	Rectangle Constructor
		/// </summary>
		///
		
		///	Creates a Rectangle from a specified x,y location and
		///	width and height values.
		

		public Rect (int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			this.width = width;
			this.height = height;
			Width = this.width;
			Height = this.height;
		}



		/// <summary>
		///	Bottom Property
		/// </summary>
		///
		
		///	The Y coordinate of the bottom edge of the Rectangle.
		///	Read only.
				
		public int Bottom {
			get {
				return Y + Height;
			}
		}

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		
		///	Indicates if the width or height are zero. Read only.
				
		public bool IsEmpty {
			get {
				return ((X == 0) && (Y == 0) && (Width == 0) && (Height == 0));
			}
		}

		/// <summary>
		///	Left Property
		/// </summary>
		///
		
		///	The X coordinate of the left edge of the Rectangle.
		///	Read only.
		

		public int Left {
			get {
				return X;
			}
		}

		/// <summary>
		///	Location Property
		/// </summary>
		///
		
		///	The Location of the top-left corner of the Rectangle.
		

		public Point Location {
			get {
				return new Point (X, Y);
			}
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		///	Right Property
		/// </summary>
		///
		
		///	The X coordinate of the right edge of the Rectangle.
		///	Read only.
		

		public int Right {
			get {
				return X + Width;
			}
		}

		/// <summary>
		///	Size Property
		/// </summary>
		///
		
		///	The Size of the Rectangle.
		

		public Size Size {
			get {
				return new Size (Width, Height);
			}
			set {
				Width = value.Width;
				Height = value.Height;
			}
		}

		/// <summary>
		///	Top Property
		/// </summary>
		///
		
		///	The Y coordinate of the top edge of the Rectangle.
		///	Read only.
		

		public int Top {
			get {
				return Y;
			}
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		
		///	Checks if an x,y coordinate lies within this Rectangle.
		

		public bool Contains (int x, int y)
		{
			return ((x >= Left) && (x < Right) && 
				(y >= Top) && (y < Bottom));
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		
		///	Checks if a Point lies within this Rectangle.
		

		public bool Contains (Point pt)
		{
			return Contains (pt.X, pt.Y);
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		
		///	Checks if a Rectangle lies entirely within this 
		///	Rectangle.
		

		public bool Contains (Rect rect)
		{
			return (rect == Intersect (this, rect));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		
		///	Checks equivalence of this Rectangle and another object.
		

		public override bool Equals (object obj)
		{
			if (!(obj is Rect))
				return false;

			return (this == (Rect) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		
		///	Calculates a hashing value.
		

		public override int GetHashCode ()
		{
			return (Height + Width) ^ X + Y;
		}

		/// <summary>
		///	IntersectsWith Method
		/// </summary>
		///
		
		///	Checks if a Rectangle intersects with this one.
		

		public bool IntersectsWith (Rect rect)
		{
			return !((Left >= rect.Right) || (Right <= rect.Left) ||
			    (Top >= rect.Bottom) || (Bottom <= rect.Top));
		}

		bool IntersectsWithInclusive (Rect r)
		{
			return !((Left > r.Right) || (Right < r.Left) ||
			    (Top > r.Bottom) || (Bottom < r.Top));
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		
		///	Moves the Rectangle a specified distance.
		

		public void Offset (int x, int y)
		{
			this.X += x;
			this.Y += y;
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		
		///	Moves the Rectangle a specified distance.
		

		public void Offset (Point pos)
		{
			X += pos.X;
			Y += pos.Y;
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		
		///	Formats the Rectangle as a string in (x,y,w,h) notation.
		

		public override string ToString ()
		{
            return string.Format("{{X={0},Y={1},Width={2},Height={3}}}",
						 X, Y, Width, Height);
		}

	}
}
