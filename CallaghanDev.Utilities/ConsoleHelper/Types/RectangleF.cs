﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Copied from https://github.com/dotnet/corefx/tree/master/src/System.Drawing.Primitives/src/System/Drawing

using System;
using System.ComponentModel;

namespace CallaghanDev.Utilities.ConsoleHelper {
	/// <summary>
	/// Stores the location and size of a rectangular region.
	/// </summary>
	public struct RectangleF : IEquatable<RectangleF> {
		/// <summary>
		/// Initializes a new instance of the <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> class.
		/// </summary>
		public static readonly RectangleF Empty;

		private float x; // Do not rename (binary serialization)
		private float y; // Do not rename (binary serialization)
		private float width; // Do not rename (binary serialization)
		private float height; // Do not rename (binary serialization)

		/// <summary>
		/// Initializes a new instance of the <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> class with the specified location
		/// and size.
		/// </summary>
		public RectangleF (float x, float y, float width, float height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> class with the specified location
		/// and size.
		/// </summary>
		public RectangleF (PointF location, SizeF size)
		{
			x = location.X;
			y = location.Y;
			width = size.Width;
			height = size.Height;
		}

		/// <summary>
		/// Creates a new <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> with the specified location and size.
		/// </summary>
		public static RectangleF FromLTRB (float left, float top, float right, float bottom) =>
		    new RectangleF (left, top, right - left, bottom - top);

		/// <summary>
		/// Gets or sets the coordinates of the upper-left corner of the rectangular region represented by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		[Browsable (false)]
		public PointF Location {
			get => new PointF (X, Y);
			set {
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// Gets or sets the size of this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		[Browsable (false)]
		public SizeF Size {
			get => new SizeF (Width, Height);
			set {
				Width = value.Width;
				Height = value.Height;
			}
		}

		/// <summary>
		/// Gets or sets the x-coordinate of the upper-left corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public float X {
			get => x;
			set => x = value;
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the upper-left corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public float Y {
			get => y;
			set => y = value;
		}

		/// <summary>
		/// Gets or sets the width of the rectangular region defined by this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public float Width {
			get => width;
			set => width = value;
		}

		/// <summary>
		/// Gets or sets the height of the rectangular region defined by this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public float Height {
			get => height;
			set => height = value;
		}

		/// <summary>
		/// Gets the x-coordinate of the upper-left corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> .
		/// </summary>
		[Browsable (false)]
		public float Left => X;

		/// <summary>
		/// Gets the y-coordinate of the upper-left corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		[Browsable (false)]
		public float Top => Y;

		/// <summary>
		/// Gets the x-coordinate of the lower-right corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		[Browsable (false)]
		public float Right => X + Width;

		/// <summary>
		/// Gets the y-coordinate of the lower-right corner of the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		[Browsable (false)]
		public float Bottom => Y + Height;

		/// <summary>
		/// Tests whether this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> has a <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF.Width'/> or a <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF.Height'/> of 0.
		/// </summary>
		[Browsable (false)]
		public bool IsEmpty => (Width <= 0) || (Height <= 0);

		/// <summary>
		/// Tests whether <paramref name="obj"/> is a <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> with the same location and
		/// size of this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public override bool Equals (object obj) => obj is RectangleF && Equals ((RectangleF)obj);

		/// <summary>
		/// Returns true if two <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> objects have equal location and size.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals (RectangleF other) => this == other;

		/// <summary>
		/// Tests whether two <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> objects have equal location and size.
		/// </summary>
		public static bool operator == (RectangleF left, RectangleF right) =>
		    left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;

		/// <summary>
		/// Tests whether two <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> objects differ in location or size.
		/// </summary>
		public static bool operator != (RectangleF left, RectangleF right) => !(left == right);

		/// <summary>
		/// Determines if the specified point is contained within the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> .
		/// </summary>
		public bool Contains (float x, float y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

		/// <summary>
		/// Determines if the specified point is contained within the rectangular region defined by this
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> .
		/// </summary>
		public bool Contains (PointF pt) => Contains (pt.X, pt.Y);

		/// <summary>
		/// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within
		/// the rectangular region represented by this <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> .
		/// </summary>
		public bool Contains (RectangleF rect) =>
		    (X <= rect.X) && (rect.X + rect.Width <= X + Width) && (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);

		/// <summary>
		/// Gets the hash code for this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public override int GetHashCode ()
		{
			return (Height.GetHashCode () + Width.GetHashCode ()) ^ X.GetHashCode () + Y.GetHashCode ();
		}

		/// <summary>
		/// Inflates this <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> by the specified amount.
		/// </summary>
		public void Inflate (float x, float y)
		{
			X -= x;
			Y -= y;
			Width += 2 * x;
			Height += 2 * y;
		}

		/// <summary>
		/// Inflates this <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> by the specified amount.
		/// </summary>
		public void Inflate (SizeF size) => Inflate (size.Width, size.Height);

		/// <summary>
		/// Creates a <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> that is inflated by the specified amount.
		/// </summary>
		public static RectangleF Inflate (RectangleF rect, float x, float y)
		{
			RectangleF r = rect;
			r.Inflate (x, y);
			return r;
		}

		/// <summary>
		/// Creates a Rectangle that represents the intersection between this Rectangle and rect.
		/// </summary>
		public void Intersect (RectangleF rect)
		{
			RectangleF result = Intersect (rect, this);

			X = result.X;
			Y = result.Y;
			Width = result.Width;
			Height = result.Height;
		}

		/// <summary>
		/// Creates a rectangle that represents the intersection between a and b. If there is no intersection, an
		/// empty rectangle is returned.
		/// </summary>
		public static RectangleF Intersect (RectangleF a, RectangleF b)
		{
			float x1 = System.Math.Max (a.X, b.X);
			float x2 = System.Math.Min (a.X + a.Width, b.X + b.Width);
			float y1 = System.Math.Max (a.Y, b.Y);
			float y2 = System.Math.Min (a.Y + a.Height, b.Y + b.Height);

			if (x2 >= x1 && y2 >= y1) {
				return new RectangleF (x1, y1, x2 - x1, y2 - y1);
			}

			return Empty;
		}

		/// <summary>
		/// Determines if this rectangle intersects with rect.
		/// </summary>
		public bool IntersectsWith (RectangleF rect) =>
		    (rect.X < X + Width) && (X < rect.X + rect.Width) && (rect.Y < Y + Height) && (Y < rect.Y + rect.Height);

		/// <summary>
		/// Creates a rectangle that represents the union between a and b.
		/// </summary>
		public static RectangleF Union (RectangleF a, RectangleF b)
		{
			float x1 = System.Math.Min (a.X, b.X);
			float x2 = System.Math.Max (a.X + a.Width, b.X + b.Width);
			float y1 = System.Math.Min (a.Y, b.Y);
			float y2 = System.Math.Max (a.Y + a.Height, b.Y + b.Height);

			return new RectangleF (x1, y1, x2 - x1, y2 - y1);
		}

		/// <summary>
		/// Adjusts the location of this rectangle by the specified amount.
		/// </summary>
		public void Offset (PointF pos) => Offset (pos.X, pos.Y);

		/// <summary>
		/// Adjusts the location of this rectangle by the specified amount.
		/// </summary>
		public void Offset (float x, float y)
		{
			X += x;
			Y += y;
		}

		/// <summary>
		/// Converts the specified <see cref='CallaghanDev.Utilities.ConsoleHelper.Rect'/> to a
		/// <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/>.
		/// </summary>
		public static implicit operator RectangleF (Rect r) => new RectangleF (r.X, r.Y, r.Width, r.Height);

		/// <summary>
		/// Converts the <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF.Location'/> and <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF.Size'/>
		/// of this <see cref='CallaghanDev.Utilities.ConsoleHelper.RectangleF'/> to a human-readable string.
		/// </summary>
		public override string ToString () =>
		    "{X=" + X.ToString () + ",Y=" + Y.ToString () +
		    ",Width=" + Width.ToString () + ",Height=" + Height.ToString () + "}";
	}
}
