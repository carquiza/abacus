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

#if !netwp75

namespace Sungiant.Abacus
{
    // Half Utils
    // ----------
    // 
    // TODO Look at this: http://csharp-half.svn.sourceforge.net/viewvc/csharp-half/
    // 
    // In computing, half precision is a binary floating-point computer 
    // number format that occupies 16 bits in computer memory.
    //
    // This class is based upon the one found in the XNA framework,
    // it's job is to pack Singles to and from the half precision 
    // floating point numbers, which is a common format for data
    // on modern GPUs.
    //
    // In IEEE 754-2008 the 16-bit base 2 format is officially referred 
    // to as binary16. It is intended for storage (of many floating-point 
    // values where higher precision need not be stored), not for 
    // performing arithmetic computations.
    //  
    // sign exponent       fraction
    //  1     5             10
    // ---------------------------------
    // | |         |                   |
    // ---------------------------------
    //
    //
    //
    // Exponent encoding
    // The half-precision binary floating-point exponent is encoded using an 
    // offset-binary representation, with the zero offset being 15; also 
    // known as exponent bias in the IEEE 754 standard.
    //     Emin = 000012 − 011112 = −14
    //         Emax = 111102 − 011112 = 15
    //         Exponent bias = 011112 = 15
    //         Thus, as defined by the offset binary representation, in 
    //         order to get the true exponent the offset of 15 has to be 
    //         subtracted from the stored exponent.
    //         The stored exponents 000002 and 111112 are interpreted specially.
    //
    // The minimum strictly positive (subnormal) value is 2^(−24) ≈ 5.96 × 10^(−8). 
    // The minimum positive normal value is 2^(−14) ≈ 6.10 × 10^(−5). 
    // The maximum representable value is (2 − 2^(−10)) × 215 = 65504.
    //
	internal static class HalfUtils
	{
		const UInt32 BiasDiffo = 0xc8000000; // 3355443200
		
        // Exponent bias
        const int cExpBias = 15;

        // Number of exponent bits
		const int cExpBits = 5;
		
        // Number of fractional bits
        const int cFracBits = 10;

		const int cFracBitsDiff = 13;
		
        const UInt32 cFracMask = 0x3ff; // 1023

		const UInt32 cRoundBit = 0x1000; // 4096
		
        const int cSignBit = 15;
		
        const UInt32 cSignMask = 0x8000; // 32768
		
        const UInt32 eMax = 0x10; // 16
		
        const int eMin = -14;
		
        const UInt32 wMaxNormal = 0x47ffefff; // 1207955455
		
        const UInt32 wMinNormal = 0x38800000; // 947912704

        internal static unsafe UInt16 Pack (Single value)
		{
			UInt32 a = * ( (UInt32*) &value );

			UInt32 b = (UInt32) ( 
                ( a & -2147483648 ) >> 
                0x10 // 16
            ); 

            UInt32 c = a & 0x7fffffff; // 2147483647
			
            if ( c > 0x47ffefff ) // 1207955455
            {
				return (UInt16) ( b | 0x7fff ); // 32767
			}
			
            if ( c < wMinNormal )
            {
				UInt32 d = ( c & 0x7fffff ) | 0x800000;
				
                int e = 0x71 - ( (int) (c >> 0x17) );
				
                c = ( e > 0x1f ) ? 0 : ( d >> e );
				
                return (UInt16) ( 
                    b | 
                    ( 
                        ( ( c + 0xfff ) + 
                        ( ( c >> 13 ) & 1 ) ) >> 
                        13 
                    ) 
                );
			}

			return (UInt16) ( 
                b | 
                ( 
                    ( 
                        ( ( c + -939524096 ) + 0xfff ) + 
                        ( ( c >> 13 ) & 1 ) 
                    ) >> 
                    13 
                ) 
            );
		}

        internal static unsafe Single Unpack (UInt16 value)
		{
			UInt32 result;

            UInt32 sign = value & cSignMask;

            int t1 = (int) ( sign << (int) eMax );

            UInt32 fraction = (UInt32) ( value & cFracMask );

            var t5 = value & -33792;

            if ( t5 == 0 ) 
            {
                if ( fraction != 0 ) 
                {
					UInt32 b = 0xfffffff2;
					
                    while ( ( fraction & 0x400 ) == 0 ) 
                    {
						b--;
						fraction = fraction << 1;
					}
					
                    fraction &= 0xfffffbff;
					
                    var t11 = b + 0x7f;
                    var t2 = t11 <<  0x17; 
                    var t3 = ( (UInt32) t1 ) | t2 ;
                    var t4 = fraction << cFracBitsDiff;

                    result = t3 | t4;
				}
                else 
                {
                    result = (UInt32) ( sign << (int)eMax );
				}
			}
            else 
            {
                var t12 = value >> 10;
                var t13 = t12 & 0x1f;
                var t14 = t13 - cExpBias;
                var t15 = t14 + 0x7f;
                var t16 = t15 << 0x17;
                var t17 = t1 | t16;
                var t19 = fraction << cFracBitsDiff; 


                result = ( (UInt32) t17 ) | t19;
			}

			return *( ( (Single*) &result ) );
		}
	}


}
#endif