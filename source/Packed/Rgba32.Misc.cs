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

namespace Sungiant.Abacus.Packed
{
	public partial struct Rgba32
	{
		Rgba32(UInt32 packedValue)
		{
			this.packedValue = packedValue;
		}

		public Rgba32(Int32 r, Int32 g, Int32 b)
		{
			if ((((r | g) | b) & -256) != 0)
			{
				r = ClampToByte64((Int64)r);
				g = ClampToByte64((Int64)g);
				b = ClampToByte64((Int64)b);
			}

			g = g << 8;
			b = b << 0x10;

			this.packedValue = (UInt32)(((r | g) | b) | -16777216);
		}

		public Rgba32(Int32 r, Int32 g, Int32 b, Int32 a)
		{
			if (((((r | g) | b) | a) & -256) != 0)
			{
				r = ClampToByte32(r);
				g = ClampToByte32(g);
				b = ClampToByte32(b);
				a = ClampToByte32(a);
			}

			g = g << 8;
			b = b << 0x10;
			a = a << 0x18;

			this.packedValue = (UInt32)(((r | g) | b) | a);
		}

		public Rgba32 (Single r, Single g, Single b)
		{
			var val = new SinglePrecision.Vector4(r, g, b, 1f);
			Pack ( ref val, out this.packedValue);
		}

		public Rgba32 (Single r, Single g, Single b, Single a)
		{
			var val = new SinglePrecision.Vector4(r, g, b, a);
			Pack(ref val, out this.packedValue);
		}

		public Rgba32(SinglePrecision.Vector3 vector)
		{
			var val = new SinglePrecision.Vector4(vector.X, vector.Y, vector.Z, 1f);
			Pack(ref val, out this.packedValue);
		}

		public static Rgba32 GenerateColorFromName(string name)
		{
			System.Random random = new System.Random(name.GetHashCode());
			return new Rgba32(
				(byte)random.Next(byte.MaxValue),
				(byte)random.Next(byte.MaxValue),
				(byte)random.Next(byte.MaxValue));
		}

		

		public static Rgba32 FromNonPremultiplied(SinglePrecision.Vector4 vector)
		{
			Rgba32 color;
			var val = new SinglePrecision.Vector4(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
			Pack(ref val, out color.packedValue);
			return color;
		}

		public static Rgba32 FromNonPremultiplied(int r, int g, int b, int a)
		{
			Rgba32 color;
			r = ClampToByte64((r * a) / 0xffL);
			g = ClampToByte64((g * a) / 0xffL);
			b = ClampToByte64((b * a) / 0xffL);
			a = ClampToByte32(a);
			g = g << 8;
			b = b << 0x10;
			a = a << 0x18;
			color.packedValue = (UInt32)(((r | g) | b) | a);
			return color;
		}

		static Int32 ClampToByte32(Int32 value)
		{
			if (value < 0)
			{
				return 0;
			}

			if (value > 0xff)
			{
				return 0xff;
			}

			return value;
		}

		static Int32 ClampToByte64(Int64 value)
		{
			if (value < 0L)
			{
				return 0;
			}

			if (value > 0xffL)
			{
				return 0xff;
			}

			return (Int32)value;
		}


		public byte R
		{
			get
			{
				return unchecked((byte)this.packedValue);
			}
			set
			{
				this.packedValue = (this.packedValue & 0xffffff00) | value;
			}
		}

		public byte G
		{
			get
			{
				return unchecked((byte)(this.packedValue >> 8));
			}
			set
			{
				this.packedValue = (this.packedValue & 0xffff00ff) | ((UInt32)(value << 8));
			}
		}

		public byte B
		{
			get
			{
				return unchecked((byte)(this.packedValue >> 0x10));
			}
			set
			{
				this.packedValue = (this.packedValue & 0xff00ffff) | ((UInt32)(value << 0x10));
			}
		}

		public byte A
		{
			get
			{
				return unchecked((byte)(this.packedValue >> 0x18));
			}
			set
			{
				this.packedValue = (this.packedValue & 0xffffff) | ((UInt32)(value << 0x18));
			}
		}

		public static Rgba32 Lerp(Rgba32 value1, Rgba32 value2, Single amount)
		{
			Rgba32 color;
			UInt32 packedValue = value1.packedValue;
			UInt32 num2 = value2.packedValue;
			int num7 = (byte)packedValue;
			int num6 = (byte)(packedValue >> 8);
			int num5 = (byte)(packedValue >> 0x10);
			int num4 = (byte)(packedValue >> 0x18);
			int num15 = (byte)num2;
			int num14 = (byte)(num2 >> 8);
			int num13 = (byte)(num2 >> 0x10);
			int num12 = (byte)(num2 >> 0x18);
			int num = (int)PackUtils.PackUnsignedNormalisedValue(65536f, amount);
			int num11 = num7 + (((num15 - num7) * num) >> 0x10);
			int num10 = num6 + (((num14 - num6) * num) >> 0x10);
			int num9 = num5 + (((num13 - num5) * num) >> 0x10);
			int num8 = num4 + (((num12 - num4) * num) >> 0x10);
			color.packedValue = (UInt32)(((num11 | (num10 << 8)) | (num9 << 0x10)) | (num8 << 0x18));
			return color;
		}

		public SinglePrecision.Vector3 ToVector3()
		{
			SinglePrecision.Vector4 colourVec4;
			this.UnpackTo(out colourVec4);

			return new SinglePrecision.Vector3(colourVec4.X, colourVec4.Y, colourVec4.Z);
		}


		public static Rgba32 Desaturate(Rgba32 colour, float desaturation)
		{
			System.Diagnostics.Debug.Assert(desaturation <= 1f && desaturation >= 0f);

			var luminanceWeights = new SinglePrecision.Vector3(0.299f, 0.587f, 0.114f);

			SinglePrecision.Vector4 colourVec4;

			colour.UnpackTo(out colourVec4);

			SinglePrecision.Vector3 colourVec = new SinglePrecision.Vector3(colourVec4.X, colourVec4.Y, colourVec4.Z);


			float luminance;

			SinglePrecision.Vector3.Dot(ref luminanceWeights, ref colourVec, out luminance);

			SinglePrecision.Vector3 lumVec = new SinglePrecision.Vector3(luminance, luminance, luminance);

			SinglePrecision.Vector3.Lerp(ref colourVec, ref lumVec, desaturation, out colourVec);

			return new Rgba32(colourVec.X, colourVec.Y, colourVec.Z, colourVec4.W);
		}

	}
}
