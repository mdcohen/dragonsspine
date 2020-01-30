#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;

namespace DragonsSpine.Utilities
{
    /// <summary>
    /// This class defines the dice that are used for random rolls. 
    /// </summary>
    public class Dice
    {
        /// <summary>
        ///  Holds the number of dice.
        /// </summary>
        private readonly int m_number;

        /// <summary>
        /// Holds the side of the dice.
        /// </summary>
        private readonly int m_sides;

        public int Number
        {
            get { return m_number; }
        }

        public int Sides
        {
            get { return m_sides; }
        }

        public int MaxRoll
        {
            get { return Number * Sides; }
        }

        /// <summary>
        /// Constructs new dice.
        /// </summary>
        /// <param name="number">How many dice.</param>
        /// <param name="sides">How many sides are on the dice.</param>
        public Dice(int number, int sides)
        {
            m_number = number;
            m_sides = sides;
        }

        #region Random Number Generator
        public class RandMT
        {
            // Period parameters
            private const ulong N = 624;
            private const ulong M = 397;
            private const ulong MATRIX_A = 0x9908B0DFUL;		// constant vector a 
            private const ulong UPPER_MASK = 0x80000000UL;		// most significant w-r bits
            private const ulong LOWER_MASK = 0X7FFFFFFFUL;		// least significant r bits
            private const uint DEFAULT_SEED = 4357;

            private static ulong[] mt = new ulong[N + 1];	// the array for the state vector
            private static ulong mti = N + 1;			// mti==N+1 means mt[N] is not initialized

            public RandMT()
            {
                ulong[] init = new ulong[4];
                init[0] = 0x123;
                init[1] = 0x234;
                init[2] = 0x345;
                init[3] = 0x456;
                ulong length = 4;
                init_by_array(init, length);
            }

            // initializes mt[N] with a seed
            void init_genrand(ulong s)
            {
                mt[0] = s & 0xffffffffUL;
                for (mti = 1; mti < N; mti++)
                {
                    mt[mti] = (1812433253UL * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                    /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                    /* In the previous versions, MSBs of the seed affect   */
                    /* only MSBs of the array mt[].                        */
                    /* 2002/01/09 modified by Makoto Matsumoto             */
                    mt[mti] &= 0xffffffffUL;
                    /* for >32 bit machines */
                }
            }


            // initialize by an array with array-length
            // init_key is the array for initializing keys
            // key_length is its length
            public void init_by_array(ulong[] init_key, ulong key_length)
            {
                ulong i, j, k;
                init_genrand(19650218UL);
                i = 1; j = 0;
                k = (N > key_length ? N : key_length);
                for (; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525UL))
                    + init_key[j] + j;		// non linear 
                    mt[i] &= 0xffffffffUL;	// for WORDSIZE > 32 machines
                    i++; j++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                    if (j >= key_length) j = 0;
                }
                for (k = N - 1; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941UL))
                    - i;					// non linear
                    mt[i] &= 0xffffffffUL;	// for WORDSIZE > 32 machines
                    i++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                }
                mt[0] = 0x80000000UL;		// MSB is 1; assuring non-zero initial array
            }

            // generates a random number on [0,0x7fffffff]-interval
            public long genrand_int31()
            {
                return (long)(genrand_int32() >> 1);
            }
            // generates a random number on [0,1]-real-interval
            public double genrand_real1()
            {
                return (double)genrand_int32() * (1.0 / 4294967295.0); // divided by 2^32-1 
            }
            // generates a random number on [0,1)-real-interval
            public double genrand_real2()
            {
                return (double)genrand_int32() * (1.0 / 4294967296.0); // divided by 2^32
            }
            // generates a random number on (0,1)-real-interval
            public double genrand_real3()
            {
                return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0); // divided by 2^32
            }
            // generates a random number on [0,1) with 53-bit resolution
            public double genrand_res53()
            {
                ulong a = genrand_int32() >> 5;
                ulong b = genrand_int32() >> 6;
                return (double)(a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
            }
            // These real versions are due to Isaku Wada, 2002/01/09 added 

            // generates a random number on [0,0xffffffff]-interval
            public ulong genrand_int32()
            {
                ulong y = 0;
                ulong[] mag01 = new ulong[2];
                mag01[0] = 0x0UL;
                mag01[1] = MATRIX_A;
                /* mag01[x] = x * MATRIX_A  for x=0,1 */

                if (mti >= N)
                {
                    // generate N words at one time
                    ulong kk;

                    if (mti == N + 1)   /* if init_genrand() has not been called, */
                        init_genrand(5489UL); /* a default initial seed is used */

                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        //mt[kk] = mt[kk+(M-N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
                        mt[kk] = mt[kk - 227] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                    mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];

                    mti = 0;
                }

                y = mt[mti++];

                /* Tempering */
                y ^= (y >> 11);
                y ^= (y << 7) & 0x9d2c5680UL;
                y ^= (y << 15) & 0xefc60000UL;
                y ^= (y >> 18);

                return y;
            }
            //==========================================================================================
            /// <summary>
            /// Generates a random positive signed integer in the interval [0,0x7fffffff]. The value
            /// is created by simply dropping the high-order sign bit of the raw unsigned value.
            /// </summary>
            /// <returns>A random positive signed integer in the interval [0,0x7fffffff].</returns>
            public long Next()
            {
                return genrand_int31();
            }

            //==========================================================================================
            /// <summary>
            /// Generates a random positive integer in the interval [0,maxValue). The value will be
            /// GE zero and LT maxValue.
            /// </summary>
            /// <param name="maxValue">The maximum value ceiling.</param>
            /// <returns>A random positive integer in the interval [0,maxValue).</returns>
            public int Next(int maxValue)
            {
                return Next(0, maxValue);
            }

            //==========================================================================================
            /// <summary>
            /// Generates a random positive integer in the interval [minValue,maxValue). The value will
            /// be GE minValue and LT maxValue.
            /// </summary>
            /// <param name="minValue">The minimum value floor.</param>
            /// <param name="maxValue">The maximum value ceiling.</param>
            /// <returns>A random positive integer in the interval [minValue,maxValue).</returns>
            /// <exception cref="ArgumentException">Thrown if the range is invalid.</exception>
            public int Next(int minValue, int maxValue)
            {
                return RandomRange(minValue, maxValue);
            }
            public int RandomRange(int lo, int hi)
            {
                return (Math.Abs((int)genrand_int32() % (hi - lo + 1)) + lo);
            }

        }
        #endregion

        public int Roll()
        {
            //RandMT generator = new RandMT();
            System.Random generator = new Random((int)(DateTime.Now.Millisecond * 4352));
            int roll = 0;

            for (int a = 0; a < m_number; a++)
            {
                roll += generator.Next(1, m_sides + 1);
            }

            return roll;
        }

        public override string ToString()
        {
            return m_number.ToString() + "d" + m_sides.ToString();
        }
    }
}
