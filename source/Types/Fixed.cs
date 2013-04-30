﻿// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ Abacus - Fast, efficient, cross precision, maths library               │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Brought to you by:                                                     │ \\
// │          _________                    .__               __             │ \\
// │         /   _____/__ __  ____    ____ |__|____    _____/  |_           │ \\
// │         \_____  \|  |  \/    \  / ___\|  \__  \  /    \   __\          │ \\
// │         /        \  |  /   |  \/ /_/  >  |/ __ \|   |  \  |            │ \\
// │        /_______  /____/|___|  /\___  /|__(____  /___|  /__|            │ \\
// │                \/           \//_____/         \/     \/                │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2013 A.J.Pook (http://sungiant.github.com)                 │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Permission is hereby granted, free of charge, to any person obtaining  │ \\
// │ a copy of this software and associated documentation files (the        │ \\
// │ "Software"), to deal in the Software without restriction, including    │ \\
// │ without limitation the rights to use, copy, modify, merge, publish,    │ \\
// │ distribute, sublicense, and/or sellcopies of the Software, and to      │ \\
// │ permit persons to whom the Software is furnished to do so, subject to  │ \\
// │ the following conditions:                                              │ \\
// │                                                                        │ \\
// │ The above copyright notice and this permission notice shall be         │ \\
// │ included in all copies or substantial portions of the Software.        │ \\
// │                                                                        │ \\
// │ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        │ \\
// │ EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     │ \\
// │ MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. │ \\
// │ IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   │ \\
// │ CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   │ \\
// │ TORT OR OTHERWISE, ARISING FROM,OUT OF OR IN CONNECTION WITH THE       │ \\
// │ SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 │ \\
// └────────────────────────────────────────────────────────────────────────┘ \\

using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Sungiant.Abacus
{
	///
	/// Fixed32 is a binary fixed point number in the Q39.24 number format.
	///
	/// This type can be useful:
	/// - as a performance enhancement when working with embedded 
	///   systems that do not have hardware based floating point support.
	/// - when a constant resolution is required
	/// 
	/// Q is a fixed point number format where the number of fractional 
	/// bits (and optionally the number of integer bits) is specified. For 
	/// example, a Q15 number has 15 fractional bits; a Q1.14 number has 
	/// 1 integer bit and 14 fractional bits.
	/// 
	/// Q format numbers are fixed point numbers; that is, they are stored 
	/// and operated upon as regular binary numbers (i.e. signed integers), 
	/// thus allowing standard integer hardware/ALU to perform rational 
	/// number calculations.
	///
	/// For a given Qm.n format, using an m+n+1 bit signed integer 
	/// container with n fractional bits:
	/// - its range is [-2^m, 2^m - 2^-n]
	/// - its resolution is 2^-n
	///
	/// Unlike floating point numbers, the resolution of Q numbers will 
	/// remain constant over the entire range.
	///
	/// Q numbers are a ratio of two integers: the numerator is kept in storage, 
	/// the denominator is equal to 2^n.
	/// 
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Fixed32
		: IFormattable
		, IComparable<Fixed32>
		, IComparable
		, IConvertible
		, IEquatable<Fixed32>
	{
		// s is the number of sign bits
		public const Int32 s = 1;

		// m is the number of bits set aside to designate the two's complement integer
		// portion of the number exclusive of the sign bit.
		public const Int32 m = 32 - n - s;

		// n is the number of bits used to designate the fractional portion of the
		// number, i.e. the number of bit's to the right of the binary point.
		// (If n = 0, the Q numbers are integers — the degenerate case)
		public const Int32 n = 12;

		// This is the raw value that is stored and operated upon.
		// Size: Signed 64-bit integer
		// Range: –9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
		Int32 numerator;

		// This value is inferred as this is a Qm.n number.
		Int32 denominator { get { return TwoToThePowerOf(n); } }

		double value { get { return (double)numerator / (double)denominator; } }



		// perhaps this shouldn't be public
		public Int32 RawValue { get { return numerator; } }
		//public Int32 RawHigh { get { return numerator >> n; } }
		//public Int32 RawLow { get { return numerator - (RawHigh << n); } }
		
		static Fixed32()
		{
			// i think this is wrong.
			Int32 l = One.RawValue;
			while (l != 0)
			{
				l /= 10;
				Digits += 1;
				DMul *= 10;
			}
		}

		public Fixed32(Int32 value)
		{
			numerator = value << n;
		}

		public Fixed32 (Int64 value)
		{
			numerator = (Int32)value << n;
		}

		public Fixed32 (Double value)
		{
			numerator = (Int32)System.Math.Round (value * (1 << n));
		}

		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Fixed32 result)
		{
			Double d;
			Boolean ok = Double.TryParse(s, style, provider, out d);
			if( ok )
			{
				result = new Fixed32(d);
			}
			else
			{
				result = 0;
			}

			return ok;
		}

		public static bool TryParse(string s, out Fixed32 result)
		{
			return TryParse(s, NumberStyles.Any, null, out result);
		}

		public static Fixed32 Parse(string s)
		{
			return Parse(s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
		}

		public static Fixed32 Parse (string s, IFormatProvider provider)
		{
			return Parse(s, (NumberStyles.Float | NumberStyles.AllowThousands), provider);
		}
		
		public static Fixed32 Parse (string s, NumberStyles style)
		{
			return Parse(s, style, null);
		}
		
		public static Fixed32 Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			Double d = Double.Parse(s, style, provider);
			return new Fixed32(d);
		}


		public static Fixed32 CreateFromRaw (Int32 rawValue)
		{
			Fixed32 f;
			f.numerator = rawValue;
			return f;
		}

		public Int32 ToInt32 ()
		{
			// todo: explain
			return (Int32)(numerator >> n);
		}
		
		public Double ToDouble ()
		{
			return numerator * d;
		}

		public Single ToSingle ()
		{
			return (Single) this.ToDouble();
		}

		public override Boolean Equals(object obj)
		{
			if (obj is Fixed32)
			{
				return ((Fixed32)obj).numerator == numerator;
			}

			return false;
		}

		public override Int32 GetHashCode()
		{
			//return (Int32)(numerator & 0xffffffff) ^ (Int32)(numerator >> 32); (for 64bit)

			return numerator;
		}

		public override String ToString()
		{
			return ToDouble().ToString();
		}
	}
}