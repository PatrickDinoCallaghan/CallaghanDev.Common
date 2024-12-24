//
// System.Drawing.Size.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using System;

namespace CallaghanDev.Utilities.ConsoleHelper {
	/// <summary>
	/// Stores an ordered pair of integers, which specify a Height and Width.
	/// </summary>
	public struct Size
	{
		int width, height;

		/// <summary>
		/// Gets a Size structure that has a Height and Width value of 0.
		/// </summary>
		public static readonly Size Empty;

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		
		///	Addition of two Size structures.
		

		public static Size operator + (Size sz1, Size sz2)
		{
			return new Size (sz1.Width + sz2.Width, 
					 sz1.Height + sz2.Height);
		}

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		
		///	Compares two Size objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		

		public static bool operator == (Size sz1, Size sz2)
		{
			return ((sz1.Width == sz2.Width) && 
				(sz1.Height == sz2.Height));
		}

		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		
		///	Compares two Size objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		

		public static bool operator != (Size sz1, Size sz2)
		{
			return ((sz1.Width != sz2.Width) || 
				(sz1.Height != sz2.Height));
		}

		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		
		///	Subtracts two Size structures.
		

		public static Size operator - (Size sz1, Size sz2)
		{
			return new Size (sz1.Width - sz2.Width, 
					 sz1.Height - sz2.Height);
		}

		/// <summary>
		///	Size to Point Conversion
		/// </summary>
		///
		
		///	Returns a Point based on the dimensions of a given 
		///	Size. Requires explicit cast.
		

		public static explicit operator Point (Size size)
		{
			return new Point (size.Width, size.Height);
		}

		/// <summary>
		///	Size Constructor
		/// </summary>
		///
		
		///	Creates a Size from a Point value.
		

		public Size (Point pt)
		{
			width = pt.X;
			height = pt.Y;
		}

		/// <summary>
		///	Size Constructor
		/// </summary>
		///
		
		///	Creates a Size from specified dimensions.
		

		public Size (int width, int height)
		{
			if (width < 0 || height < 0)
				throw new ArgumentException ("Either Width and Height must be greater or equal to 0.");

			this.width = width;
			this.height = height;
		}

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		
		///	Indicates if both Width and Height are zero.
		

		public bool IsEmpty {
			get {
				return ((width == 0) && (height == 0));
			}
		}

		/// <summary>
		///	Width Property
		/// </summary>
		///
		
		///	The Width coordinate of the Size.
		

		public int Width {
			get {
				return width;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("Width must be greater or equal to 0.");
				width = value;
			}
		}

		/// <summary>
		///	Height Property
		/// </summary>
		///
		
		///	The Height coordinate of the Size.
		

		public int Height {
			get {
				return height;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("Height must be greater or equal to 0.");
				height = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		
		///	Checks equivalence of this Size and another object.
		

		public override bool Equals (object obj)
		{
			if (!(obj is Size))
				return false;

			return (this == (Size) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		
		///	Calculates a hashing value.
		

		public override int GetHashCode ()
		{
			return width^height;
		}

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        
        ///	Formats the Size as a string in coordinate notation.
        

        public override string ToString() => string.Format("{{Width={0}, Height={1}}}", width, height);

        /// <summary>
        /// Adds the width and height of one Size structure to the width and height of another Size structure.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="sz1">The first Size structure to add.</param>
        /// <param name="sz2">The second Size structure to add.</param>
        public static Size Add (Size sz1, Size sz2)
		{
			return new Size (sz1.Width + sz2.Width, 
					 sz1.Height + sz2.Height);

		}

		/// <summary>
		/// Subtracts the width and height of one Size structure to the width and height of another Size structure.
		/// </summary>
		/// <returns>The subtract.</returns>
		/// <param name="sz1">The first Size structure to subtract.</param>
		/// <param name="sz2">The second Size structure to subtract.</param>
		public static Size Subtract (Size sz1, Size sz2)
		{
			return new Size (sz1.Width - sz2.Width, 
					 sz1.Height - sz2.Height);
		}

	}
}
