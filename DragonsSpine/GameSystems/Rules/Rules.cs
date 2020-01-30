using System;
using System.Collections.Generic;
using DragonsSpine.Autonomy.EntityBuilding;
using DragonsSpine.GameWorld;

namespace DragonsSpine
{    

    public static class Rules
    {
        /// <summary>
        /// Number of karma to turn evil (lawfuls and neutrals only).
        /// </summary>
        public const int NUM_KARMA_TURN_EVIL = 4;

        /// <summary>
        /// Holds the dice rolling object.
        /// </summary>
        public static RandMT Dice = new RandMT();

        #region Random Number Generator (New)
        public sealed class RandMT
        {
            #region Constants -------------------------------------------------------
            // Period parameters.
            private const int N = 624;
            private const int M = 397;
            private const uint MATRIX_A = 0x9908b0dfU;   // constant vector a
            private const uint UPPER_MASK = 0x80000000U; // most significant w-r bits
            private const uint LOWER_MASK = 0x7fffffffU; // least significant r bits
            private const int MAX_RAND_INT = 0x7fffffff;
            #endregion Constants
            #region Instance Variables ----------------------------------------------
            // mag01[x] = x * MATRIX_A  for x=0,1
            private uint[] mag01 = { 0x0U, MATRIX_A };
            // the array for the state vector
            private uint[] mt = new uint[N];
            // mti==N+1 means mt[N] is not initialized
            private int mti = N + 1;
            #endregion Instance Variables
            #region Constructors ----------------------------------------------------
            /// <summary>
            /// Creates a random number generator using the time of day in milliseconds as
            /// the seed.
            /// </summary>
            public RandMT()
            {
                init_genrand((uint)DateTime.Now.Millisecond);
            }
            /// <summary>
            /// Creates a random number generator initialized with the given seed.
            /// </summary>
            /// <param name="seed">The seed.</param>
            public RandMT(int seed)
            {
                init_genrand((uint)seed);
            }
            /// <summary>
            /// Creates a random number generator initialized with the given array.
            /// </summary>
            /// <param name="init">The array for initializing keys.</param>
            public RandMT(int[] init)
            {
                uint[] initArray = new uint[init.Length];
                for (int i = 0; i < init.Length; ++i)
                    initArray[i] = (uint)init[i];
                init_by_array(initArray, (uint)initArray.Length);
            }
            #endregion Constructors
            #region Properties ------------------------------------------------------
            /// <summary>
            /// Gets the maximum random integer value. All random integers generated
            /// by instances of this class are less than or equal to this value. This
            /// value is <c>0x7fffffff</c> (<c>2,147,483,647</c>).
            /// </summary>
            public static int MaxRandomInt
            {
                get
                {
                    return 0x7fffffff;
                }
            }
            #endregion Properties
            #region Member Functions ------------------------------------------------
            /// <summary>
            /// Returns a random integer greater than or equal to zero and
            /// less than or equal to <c>MaxRandomInt</c>.
            /// </summary>
            /// <returns>The next random integer.</returns>
            public int Next()
            {
                return genrand_int31();
            }
            /// <summary>
            /// Returns a positive random integer between 0 and 1 less than the specified maximum.
            /// </summary>
            /// <param name="maxValue">The maximum value. Must be greater than zero.</param>
            /// <returns>A positive random integer less than or equal to <c>maxValue</c>.</returns>
            public int Next(int maxValue)
            {
                return Next(0, maxValue);
            }
            /// <summary>
            /// Returns a random integer within the specified range.
            /// </summary>
            /// <param name="minValue">The lower bound.</param>
            /// <param name="maxValue">The upper bound.</param>
            /// <returns>A random integer greater than or equal to <c>minValue</c>, and less than
            /// or equal to <c>maxValue</c>.</returns>
            public int Next(int minValue, int maxValue)
            {
                //return new Random(Guid.NewGuid().GetHashCode()).Next(minValue, maxValue + 1);
                if (minValue == maxValue)
                    return minValue;
                if (minValue > maxValue)
                {
                    int tmp = maxValue;
                    maxValue = minValue;
                    minValue = tmp;
                }
                int test = (int)(Math.Floor((maxValue - minValue) * genrand_real2() + minValue));
                return test;
            }
            /// <summary>
            /// Returns a random number between 0.0 and 1.0.
            /// </summary>
            /// <returns>A single-precision floating point number greater than or equal to 0.0,
            /// and less than 1.0.</returns>
            public float NextFloat()
            {
                return (float)genrand_real2();
            }
            /// <summary>
            /// Returns a random number greater than or equal to zero, and either strictly
            /// less than one, or less than or equal to one, depending on the value of the
            /// given boolean parameter.
            /// </summary>
            /// <param name="includeOne">
            /// If <c>true</c>, the random number returned will be
            /// less than or equal to one; otherwise, the random number returned will
            /// be strictly less than one.
            /// </param>
            /// <returns>
            /// If <c>includeOne</c> is <c>true</c>, this method returns a
            /// single-precision random number greater than or equal to zero, and less
            /// than or equal to one. If <c>includeOne</c> is <c>false</c>, this method
            /// returns a single-precision random number greater than or equal to zero and
            /// strictly less than one.
            /// </returns>
            public float NextFloat(bool includeOne)
            {
                if (includeOne)
                {
                    return (float)genrand_real1();
                }
                return (float)genrand_real2();
            }
            /// <summary>
            /// Returns a random number greater than 0.0 and less than 1.0.
            /// </summary>
            /// <returns>A random number greater than 0.0 and less than 1.0.</returns>
            public float NextFloatPositive()
            {
                return (float)genrand_real3();
            }
            /// <summary>
            /// Returns a random number between 0.0 and 1.0.
            /// </summary>
            /// <returns>A double-precision floating point number greater than or equal to 0.0,
            /// and less than 1.0.</returns>
            public double NextDouble()
            {
                return genrand_real2();
            }
            /// <summary>
            /// Returns a random number greater than or equal to zero, and either strictly
            /// less than one, or less than or equal to one, depending on the value of the
            /// given boolean parameter.
            /// </summary>
            /// <param name="includeOne">
            /// If <c>true</c>, the random number returned will be
            /// less than or equal to one; otherwise, the random number returned will
            /// be strictly less than one.
            /// </param>
            /// <returns>
            /// If <c>includeOne</c> is <c>true</c>, this method returns a
            /// single-precision random number greater than or equal to zero, and less
            /// than or equal to one. If <c>includeOne</c> is <c>false</c>, this method
            /// returns a single-precision random number greater than or equal to zero and
            /// strictly less than one.
            /// </returns>
            public double NextDouble(bool includeOne)
            {
                if (includeOne)
                {
                    return genrand_real1();
                }
                return genrand_real2();
            }
            /// <summary>
            /// Returns a random number greater than 0.0 and less than 1.0.
            /// </summary>
            /// <returns>A random number greater than 0.0 and less than 1.0.</returns>
            public double NextDoublePositive()
            {
                return genrand_real3();
            }
            /// <summary>
            /// Generates a random number on <c>[0,1)</c> with 53-bit resolution.
            /// </summary>
            /// <returns>A random number on <c>[0,1)</c> with 53-bit resolution</returns>
            public double Next53BitRes()
            {
                return genrand_res53();
            }
            /// <summary>
            /// Reinitializes the random number generator using the time of day in
            /// milliseconds as the seed.
            /// </summary>
            public void Initialize()
            {
                init_genrand((uint)DateTime.Now.Millisecond);
            }
            /// <summary>
            /// Reinitializes the random number generator with the given seed.
            /// </summary>
            /// <param name="seed">The seed.</param>
            public void Initialize(int seed)
            {
                init_genrand((uint)seed);
            }
            /// <summary>
            /// Reinitializes the random number generator with the given array.
            /// </summary>
            /// <param name="init">The array for initializing keys.</param>
            public void Initialize(int[] init)
            {
                uint[] initArray = new uint[init.Length];
                for (int i = 0; i < init.Length; ++i)
                    initArray[i] = (uint)init[i];
                init_by_array(initArray, (uint)initArray.Length);
            }
            #region Methods ported from C -------------------------------------------
            // initializes mt[N] with a seed
            private void init_genrand(uint s)
            {
                mt[0] = s & 0xffffffffU;
                for (mti = 1; mti < N; mti++)
                {
                    mt[mti] =
                      (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                    // See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier.
                    // In the previous versions, MSBs of the seed affect
                    // only MSBs of the array mt[].
                    // 2002/01/09 modified by Makoto Matsumoto
                    mt[mti] &= 0xffffffffU;
                    // for >32 bit machines
                }
            }
            // initialize by an array with array-length
            // init_key is the array for initializing keys
            // key_length is its length
            private void init_by_array(uint[] init_key, uint key_length)
            {
                int i, j, k;
                init_genrand(19650218U);
                i = 1; j = 0;
                k = (int)(N > key_length ? N : key_length);
                for (; k > 0; k--)
                {
                    mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j); /* non linear */
                    mt[i] &= 0xffffffffU; // for WORDSIZE > 32 machines
                    i++; j++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                    if (j >= key_length) j = 0;
                }
                for (k = N - 1; k > 0; k--)
                {
                    mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941U)) - i); /* non linear */
                    mt[i] &= 0xffffffffU; // for WORDSIZE > 32 machines
                    i++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                }
                mt[0] = 0x80000000U; // MSB is 1; assuring non-zero initial array
            }
            // generates a random number on [0,0xffffffff]-interval
            uint genrand_int32()
            {
                uint y = 0;
                if (mti >= N)
                { /* generate N words at one time */
                    int kk;
                    if (mti == N + 1)   /* if init_genrand() has not been called, */
                        init_genrand(5489U); /* a default initial seed is used */
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                    }
                    y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                    mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                    mti = 0;
                }
                try
                {
                    y = mt[mti++];
                }
                catch (IndexOutOfRangeException)
                {
                    try
                    {
                        y = mt[mti - 1];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        y = mt[mt.Length - 1];
                    }
                    //Utils.LogException(iorex);
                    //Utils.Log("IndexOutOfRangeException in Rules.Dice.genrand_int32() y = " + y.ToString() + " mti = " + mti + " mt.Length = " + mt.Length.ToString() + " N = " + N.ToString(), Utils.LogType.Unknown);
                }
                finally
                {
                    // Tempering
                    y ^= (y >> 11);
                    y ^= (y << 7) & 0x9d2c5680U;
                    y ^= (y << 15) & 0xefc60000U;
                    y ^= (y >> 18);
                }
                return y;
            }
            // generates a random number on [0,0x7fffffff]-interval
            private int genrand_int31()
            {
                return (int)(genrand_int32() >> 1);
            }
            // generates a random number on [0,1]-real-interval
            double genrand_real1()
            {
                return genrand_int32() * (1.0 / 4294967295.0);
                // divided by 2^32-1
            }
            // generates a random number on [0,1)-real-interval
            double genrand_real2()
            {
                return genrand_int32() * (1.0 / 4294967296.0);
                // divided by 2^32
            }
            // generates a random number on (0,1)-real-interval
            double genrand_real3()
            {
                return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
                // divided by 2^32
            }
            // generates a random number on [0,1) with 53-bit resolution
            double genrand_res53()
            {
                uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
                return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
            }
            // These real versions are due to Isaku Wada, 2002/01/09 added
            #endregion Methods ported from C
            #endregion Member Functions
        }
        #endregion

        #region Detect Thief
        /// <summary>
        /// This is called for all professions, yet is only significant if the detector is a thief or knight.
        /// </summary>
        /// <param name="thief">Possible thief to detect.</param>
        /// <param name="detector">The Character object attempting to detect a thief.</param>
        /// <returns>True if detected.</returns>
        public static bool DetectThief(Character thief, Character detector)
        {
            // Safety net. This method is called from several locations.
            if (thief.BaseProfession != Character.ClassType.Thief) return true;

            // Override for immortals.
            if (detector.IsImmortal && thief.BaseProfession == Character.ClassType.Thief) return true;

            // Override with the improved disguise spell.
            if (thief.EffectsList.ContainsKey(Effect.EffectTypes.Obfuscation))
                return false;

            // Thief is an NPC, without Improved Disguise, and is attacking/getting ready to attack the detector.
            if (thief.BaseProfession == Character.ClassType.Thief && thief is NPC && (thief as NPC).MostHated != null && (thief as NPC).MostHated == detector) return true;

            // Thief is an NPC, without Improved Disguise, and is attacking/getting ready to attack the pet of the detector.
            if (thief.BaseProfession == Character.ClassType.Thief && thief is NPC && (thief as NPC).MostHated != null && (thief as NPC).MostHated.PetOwner != null && (thief as NPC).MostHated.PetOwner == detector) return true;

            // Other thieves and knights are the only ones who can currently detect thieves.
            if (detector.BaseProfession != Character.ClassType.Thief && detector.BaseProfession != Character.ClassType.Knight)
                return false;

            // Thief detecting a non-thief.
            if (detector.BaseProfession == Character.ClassType.Thief && thief.BaseProfession != Character.ClassType.Thief)
                return true;

            // This should probably be removed now that NPC thieves are prevelant. 11/23/2015 Eb
            //if ((thief is NPC) && (detector is PC))
            //    return true;

            // Knights compare their level versus the thief's magic level.
            if(detector.BaseProfession == Character.ClassType.Knight && detector.Level > Skills.GetSkillLevel(thief.magic))
                return true;

            // Thieves compare magic levels.
            if (detector.BaseProfession == Character.ClassType.Thief && Skills.GetSkillLevel(detector.magic) >= Skills.GetSkillLevel(thief.magic))
                return true;

            // All other character classes, and situations, see a fighter of the same alignment.
            return false;
        }
        #endregion

        #region Detect Alignment
        public static bool DetectAlignment(Character target, Character detector)
        {
            /*
             * Since alignment is the primary determining factor if a target is an enemy,
             * it only makes sense to return true here if the target is indeed an enemy.
             */

            try
            {
                if (!detector.IsPC) // detector is not a player
                {
                    // First and foremost, if the two Character object's entities are natural enemies there will be blood spilled.
                    // Perhaps in the future when polymorphing is added to the logic this could be overridden/ignored.
                    if (EntityLists.TupledEntityListContains(EntityLists.NATURAL_ENEMIES, target.entity, detector.entity))
                        return true;

                    // Coexistent ZPlane. Non pets don't attack each other.
                    if (target is NPC && detector.PetOwner == null && detector.Map.ZPlanes[detector.Z].isCoexistent)
                        return false;

                    if (target.IsImmortal) { return false; } // nothing goes after immortals

                    if (target is Merchant || detector is Merchant) { return false; }
                    //{
                    //    if ((target as Merchant).trainerType != Merchant.TrainerType.None) { return false; } // never detect alignment of trainers
                    //    if ((target as Merchant).merchantType != Merchant.MerchantType.None) { return false; } // never detect alignment of merchants
                    //    if ((target as Merchant).interactiveType != Merchant.InteractiveType.None) { return false; } // never go after quest givers?
                    //}

                    if ((target is NPC) && (target as NPC).PetOwner == target) return false; // never attack pet owner?

                    if (detector is NPC npc)
                    {
                        if (npc.PetOwner != null)
                        {
                            // Pets automatically detect alignment when their pet owner is the target.
                            if (target is NPC && target.TargetID == (detector as NPC).PetOwner.UniqueID) return true;
                        }

                        // lawful NOT detecting a non lawful with no karma and not flagged
                        if (detector.Alignment == Globals.eAlignment.Lawful && target.Alignment != Globals.eAlignment.Lawful && target is PC pc && pc.currentKarma <= 0 && !detector.FlaggedUniqueIDs.Contains(target.UniqueID))
                            return false;
                    }

                    if (!target.IsPC && !target.IsImage && target.QuestList.Count > 0) { return false; } // should this be? -Eb

                    // target is a player, detector has the player flagged, check if detector is a pet of the player (just in case the player's ID somehow became flagged)
                    if (target.IsPC && detector.FlaggedUniqueIDs.Contains(target.UniqueID) && detector.PetOwner != null && detector.PetOwner != target) { return true; }

                    // improved disguise works against all except chaotic evil?
                    if (target.EffectsList.ContainsKey(Effect.EffectTypes.Obfuscation) && detector.Alignment != Globals.eAlignment.ChaoticEvil) { return false; }

                    // Special rules for lawful and neutral elves to work together. Except for Drow, because they're just plain evil.
                    if (detector.species == Globals.eSpecies.Elvish && target.species == Globals.eSpecies.Elvish && (detector.Alignment == Globals.eAlignment.Lawful || detector.Alignment == Globals.eAlignment.Neutral))
                    {
                        if (target.Alignment == Globals.eAlignment.Evil || target.Alignment == Globals.eAlignment.Chaotic || target.Alignment == Globals.eAlignment.ChaoticEvil)
                            return true;
                    }

                    // All Fey entities ignore alignment. (not enemies)
                    if (EntityLists.FEY.Contains(target.entity) && EntityLists.FEY.Contains(detector.entity))
                        return false;

                    switch (detector.Alignment)
                    {
                        case Globals.eAlignment.ChaoticEvil:
                            if (target.Alignment == Globals.eAlignment.ChaoticEvil) return false; // chaotic evil doesn't attack fellow chaotic evil
                            return true;
                        case Globals.eAlignment.Amoral: // amorals don't care about alignment
                            return false;
                        case Globals.eAlignment.Chaotic:
                            if (target.Alignment == Globals.eAlignment.Chaotic) { return false; } // chaotics do not attack chaotic alignment
                            else if (target.Alignment == Globals.eAlignment.Amoral) { return false; } // chaotics do not attack amoral alignment
                            if (detector.PetOwner != null && detector.PetOwner == target) { return false; } // chaotic pets will never attack owner
                            return true;
                        case Globals.eAlignment.Evil: // evils attack everything except amoral
                            if (target.Alignment == Globals.eAlignment.Amoral) { return false; }
                            if (target.IsPC)
                            {
                                // Evil NPC pets won't attack owner (perhaps they shouldn't attack anyone if a pet?)
                                if (detector.PetOwner != null && detector.PetOwner == target) return false;
                            }
                            // evil npc should attack evil NPC... 
                            if (target.Alignment == Globals.eAlignment.Evil) { return false; }
                            return true;
                        case Globals.eAlignment.Lawful: // lawfuls will usually attack any non lawful and non amoral
                            if (target.Alignment != Globals.eAlignment.Lawful)
                            {
                                if (target.Alignment == Globals.eAlignment.Amoral) { return false; } // lawfuls will not attack amoral alignment

                                if (target.BaseProfession == Character.ClassType.Thief) // a thief can disguise their alignment
                                {
                                    if (!DetectThief(target, detector)) return false;
                                }

                                // lawful NPCs won't attack a non lawful PC unless they have karma
                                if (target is PC nonlawfulPC && nonlawfulPC.currentKarma <= 0) return false;

                                return true; // otherwise return true, lawfuls will attack any non lawful
                            }
                            else
                            {
                                if(target is PC lawfulPC && lawfulPC.currentKarma > 0)
                                {
                                    // sheriffs and knights attack lawful players with karma
                                    if ((detector as NPC).aiType == NPC.AIType.Sheriff || detector.BaseProfession == Character.ClassType.Knight) return true;
                                }
                            }
                            return false;
                        case Globals.eAlignment.Neutral:
                            //TODO this is currently limited as there are not many, if any, neutral NPCs in the game world 9/23/2013 -Eb
                            // neutrals are not attacked on sight in the Underworld
                            if (target.LandID != World.LAND_UW)
                            {
                                if (target.Alignment == Globals.eAlignment.Chaotic || target.Alignment == Globals.eAlignment.Evil || target.Alignment == Globals.eAlignment.ChaoticEvil)
                                    return true;
                            }
                            // temporarily added while testing pet AI
                            //if ((detector is NPC) && (detector as NPC).previousMostHated == target)
                            //    return true;
                            return false;
                        default:
                            return false;
                    }
                }
                else
                {
                    if (target is NPC && target.TargetID == detector.UniqueID) return true;

                    // Currently the thief class is the only option for disguising alignment.
                    if (target.BaseProfession == Character.ClassType.Thief)
                    {
                        if (target.Alignment == Globals.eAlignment.Neutral)
                            return DetectThief(target, detector);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.Log("Rules.DetectAlignment(" + target.Alignment + ", " + detector.Alignment + ") " + e.Message + "  Stack: " + e.StackTrace, Utils.LogType.ExceptionDetail);
                return false;
            }
        }
        #endregion

        #region Detect Hidden

        /// <summary>
        /// Returns true if detector detects invis.
        /// </summary>
        /// <param name="invis"></param>
        /// <param name="detector"></param>
        /// <returns></returns>
        public static bool DetectInvisible(Character invis, Character detector)
        {
            // Higher impLevel && immortal is set to on.
            if ((detector is PC) && (invis is PC) && (detector as PC).ImpLevel >= (invis as PC).ImpLevel && detector.IsImmortal)
                return true;

            // Always detect yourself.
            if (invis == detector)
                return true;

            // Do not see the dead.
            if (invis.IsDead)
                return false;

            // Do not detect someone who is not invisible.
            if (!invis.IsInvisible)
                return true;

            // Take impLevel into account. Higher level or equal level can see equal and lower level. Lower level cannot see invisible higher level impLevel.
            if (invis is PC invisPC && detector is PC detectorPC && invisPC.ImpLevel <= detectorPC.ImpLevel)
                return true;

            return false;
        }

        public static bool DetectHidden(Character hider, Character detector) // return true if a hider is det
        {
            if (hider == null || detector == null) return false;

            // Detect Undead effect always detects undead, even if they're hidden.
            if (hider.IsUndead && detector.HasEffect(Effect.EffectTypes.Detect_Undead))
                return true;

            // Cognoscere effect. Can see even Umbral Form.
            if (detector.HasEffect(Effect.EffectTypes.Cognoscere) || EntityLists.COGNIZANT.Contains(detector.entity)) return true;

            // always see a hider in the same cell
            if (hider.CurrentCell == detector.CurrentCell && !hider.HasEffect(Effect.EffectTypes.Umbral_Form))
            {
                return true;
            }
            
            // The hider will always see itself.
            if (hider == detector)
                return true;

            // If the hider does not have a Hide_In_Shadows effect it is not hidden.
            if (!hider.IsHidden)
                return true;

            // If the immortal flag is set the hider (player only) is always hidden.
            if (detector.IsImmortal)
                return true;

            if (detector.IsMeditating) return false; // meditating means the hidden creature will be able to avoid detection

            // AI Enforcer will see any hidden NPC, it's their job.
            if (!detector.IsPC && !hider.IsPC && (detector as NPC).aiType == NPC.AIType.Enforcer) return true;

            int distance = Cell.GetCellDistance(detector.X, detector.Y, hider.X, hider.Y);

            // unless the NPC is a thief it has a natural ability to hide and is thus hidden at a distance greater than 1
            if (hider.EffectsList[Effect.EffectTypes.Hide_in_Shadows].Duration < 0 && distance > 1 && hider.BaseProfession != Character.ClassType.Thief)
            {
                return false;
            }

            int hiderSkillLevel = Skills.GetSkillLevel(hider.magic);
            int difference = hiderSkillLevel - detector.Level;

            // intelligence affects a hider and detector's ability
            difference += (int)((GetFullAbilityStat(hider, Globals.eAbilityStat.Intelligence) - GetFullAbilityStat(detector, Globals.eAbilityStat.Intelligence)) / 2);

            // small bonus for those with magic skill > 10
            if (hiderSkillLevel > 10) difference += hiderSkillLevel - 10;

            int modifier = 0;

            if (hider.HasEffect(Effect.EffectTypes.Obfuscation, out Effect obfuscationEffect)) modifier += obfuscationEffect.Power;
            if (hider.HasEffect(Effect.EffectTypes.Umbral_Form, out Effect umbralFormEffect)) modifier += umbralFormEffect.Power;

            if (distance <= 0) // always see characters with natural ability to hide when on same cell
            {
                // give a thief with magical hide 10+ a chance to remain hidden when on the same cell
                if (hider.BaseProfession == Character.ClassType.Thief && hiderSkillLevel >= 10)
                {
                    //TODO: Add a talent for better hiding ability.
                    // Added Umbral Form -- still need a talent for this.
                    if (RollD(1, 100) + (hiderSkillLevel - 10) + difference + modifier >= 50)
                    {
                        return false;
                    }
                }
                return true;
            }
            else return !(55 + (distance * 10) + GetFullAbilityStat(hider, Globals.eAbilityStat.Dexterity) + difference + modifier > Rules.RollD(1, 100));
        }
        #endregion

        #region Aging Effects
        public static void DoAgingEffect(PC ch)
        {
            if (ch.Age == World.AgeCycles[0]) // young
            { ch.WriteToDisplay("You are now young."); }
            else if (ch.Age == World.AgeCycles[1]) // middle-aged
            {
                ch.WriteToDisplay("You are now middle-aged.");
                if (ch.Wisdom < 18) { ch.Wisdom++; }
            }
            else if (ch.Age == World.AgeCycles[2]) // old
            {
                ch.WriteToDisplay("You are now old.");
                if (ch.Constitution > 9) { ch.Constitution--; }
                if (ch.Strength > 9) { ch.Strength--; }
                if (ch.Dexterity > 9) { ch.Dexterity--; }
                if (ch.Wisdom < 18) { ch.Wisdom++; }
                if (ch.Charisma < 18) { ch.Charisma++; }
            }
            else if (ch.Age == World.AgeCycles[3]) // very old
            {
                ch.WriteToDisplay("You are now very old.");
                if (ch.Constitution > 9) { ch.Constitution--; }
                if (ch.Strength > 9) { ch.Strength--; }
                if (ch.Dexterity > 9) { ch.Dexterity--; }
                if (ch.Wisdom < 21) { ch.Wisdom++; }
                if (ch.Charisma < 21) { ch.Charisma++; }
            }
            else if (ch.Age == World.AgeCycles[4]) // ancient
            {
                ch.WriteToDisplay("You are now ancient.");
                if (ch.Constitution > 5) { ch.Constitution = 9; }
                if (ch.Strength > 5) { ch.Strength = 9; }
                if (ch.Dexterity > 5) { ch.Dexterity--; }
                if (ch.Wisdom < 22) { ch.Wisdom++; }
                if (ch.Charisma < 22) { ch.Charisma++; }
            }

            if (ch.Age >= World.AgeCycles[4] && ch.ImpLevel == Globals.eImpLevel.USER)
            {
                int grim;
                grim = Rules.RollD(1, 100);
                if (ch.IsLucky) grim += Rules.RollD(1, 4);
                if (grim < 5) // things look grim, send player to Underworld to regain youth
                {
                    ch.WriteToDisplay("You have died of natural causes.");
                    Rules.EnterUnderworld(ch);
                }
            }
        }
        #endregion

        public static bool BreakHideSpell(Character ch)
        {
            if (ch.HasEffect(Effect.EffectTypes.Umbral_Form))
                return false;

            if (GetEncumbrance(ch) > Globals.eEncumbranceLevel.Heavily)
                return true;

            if (ch.RightHand != null)
                if (ch.RightHand.size == Globals.eItemSize.Belt_Large_Slot_Only || ch.RightHand.size == Globals.eItemSize.No_Container)
                    return true;

            if (ch.LeftHand != null)
                if (ch.LeftHand.size == Globals.eItemSize.Belt_Large_Slot_Only || ch.LeftHand.size == Globals.eItemSize.No_Container)
                    return true;

            if (ch.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield))
                return true;

            return false;
        }

        public static int GetHitsGain(Character ch, int loopTimes)
        {
            int hitsGain = 0;

            int minimum = 1;

            if (ch.IsLucky) { minimum = World.HitDice[(int)ch.BaseProfession] / 2; }

            int statMod = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Constitution);

            if (statMod < -3) statMod = -3;

            for (int a = 0; a < loopTimes; a++)
            {
                hitsGain += Dice.Next(minimum, World.HitDice[(int)ch.BaseProfession] + 1 + statMod);
            }

            if (hitsGain <= 0) { hitsGain = 1; } // confirm at least 1 hit is gained

            return hitsGain;
        }

        public static int GetManaGain(Character ch, int loopTimes)
        {
            if (World.ManaDice[(int)ch.BaseProfession] == 0) { return 0; }

            int manaGain = 0;

            int minimum = 1;

            if (ch.IsLucky) { minimum = World.ManaDice[(int)ch.BaseProfession] / 2; }

            int statMod = 0;

            if (ch.IsIntelligenceCaster)
                statMod = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Intelligence);

            if (ch.IsWisdomCaster)
                statMod = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Wisdom);

            if (statMod < -3) statMod = -3;
            
            for (int a = 0; a < loopTimes; a++)
            {
                manaGain += Dice.Next(minimum, World.ManaDice[(int)ch.BaseProfession] + 1 + statMod);
            }

            if (manaGain <= 0) { manaGain = 1; } // confirm at least 1 mana is gained
            return manaGain;
        }

        public static int GetStaminaGain(Character ch, int loopTimes)
        {
            int staminaGain = 0;
            int minimum = 1;
            if (ch.IsLucky) { minimum = (int)World.StaminaDice[(int)ch.BaseProfession] / 2; }
            int statMod = Rules.GetGenericStatModifier(ch, Globals.eAbilityStat.Constitution);
            if(statMod < -3) statMod = -3;
            for (int a = 0; a < loopTimes; a++)
            {
                staminaGain += Dice.Next(minimum, World.StaminaDice[(int)ch.BaseProfession] + 1 + statMod);
            }
            if (staminaGain <= 0) { staminaGain = 1; } // confirm at least 1 stamina is gained
            return staminaGain;
        }

        public static int GetMaximumHits(Character ch)
        {
            // Caps set to Advanced Game on 2/3/2017. Eb
            //Land land = ch.Facet.GetLandByID(Land.ID_ADVANCEDGAME);
            return World.HitDice[(int)ch.BaseProfession] * ch.Level + 64;

            //return ch.Land.HitDice[(int)ch.BaseProfession] * ch.Level + ch.Land.StatCapOperand;
        }

        public static int GetMaximumMana(Character ch)
        {
            // Ravagers and knights.
            if (ch.IsHybrid) return 3;

            // Caps set to Advanced Game on 2/3/2017. Eb
            //Land land = ch.Facet.GetLandByID(Land.ID_ADVANCEDGAME);
            return World.ManaDice[(int)ch.BaseProfession] * ch.Level;

            //return ch.Land.ManaDice[(int)ch.BaseProfession] * ch.Level;
        }

        public static int GetMaximumStamina(Character ch)
        {
            // Caps set to Advanced Game on 2/3/2017. Eb
            //Land land = ch.Facet.GetLandByID(Land.ID_ADVANCEDGAME);
            return World.StaminaDice[(int)ch.BaseProfession] * ch.Level;

            //return ch.Land.StaminaDice[(int)ch.BaseProfession] * ch.Level;
        }

        public static Globals.eEncumbranceLevel GetEncumbrance(Character ch)
        {
            int maxEncumb = Rules.Formula_MaxEncumbrance(ch);
            
            if (ch.encumbrance >= maxEncumb * 1.5)
                return Globals.eEncumbranceLevel.Severely;
            else if(ch.encumbrance >= maxEncumb)
                return Globals.eEncumbranceLevel.Heavily;
            else if(ch.encumbrance >= maxEncumb / 1.5)
                return Globals.eEncumbranceLevel.Moderately;
            else return Globals.eEncumbranceLevel.Lightly;
        }

        public static int Formula_MaxEncumbrance(Character ch)
        {
            return Convert.ToInt32((GetFullAbilityStat(ch, Globals.eAbilityStat.Strength) * 9) + (GetFullAbilityStat(ch, Globals.eAbilityStat.Dexterity) * 2) +
                (GetFullAbilityStat(ch, Globals.eAbilityStat.Constitution) * 2));
        }

        public static long Formula_TrainingCostForLevel(int skillLevel)
        {
            return Convert.ToInt64((skillLevel + 1) * Math.Pow(2, skillLevel) * 25);
        }

        public static long Formula_DoctoredHPCost(int hitsDoctored, Character.ClassType profession)
        {
            int doctoredHPLimit = World.DoctoredHPLimits[(int)profession]; // get doctored hp limit
            int cost = doctoredHPLimit + 191; // get initial cost of first doctored hp

            int costAdd = 3;

            for (int a = 0, counter = 1; a < hitsDoctored + 1; a++)
            {
                if (a > 40) // will not start calculating increase to costAdd until hitpoint 41
                {
                    if (counter == 5) // cost increase every 10 hit points
                    {
                        counter = 0; // reset to 0, will increase after this if statement

                        if (costAdd < 28) // at cost 28 start increasing by 100 every 10 hits
                            costAdd++;
                        else costAdd += 100;
                    }

                    counter++; // increase every time through the loop
                }

                cost += costAdd;
            }

            return cost;
        }

        public static long Formula_DoctoredHPCost(Character player)
        {
            // this function will not be called unless player passes all requirements for HP doctoring

            int doctoredHPLimit = World.DoctoredHPLimits[(int)player.BaseProfession]; // get doctored hp limit
            int cost = doctoredHPLimit + 191; // get initial cost of first doctored hp

            int costAdd = 3;

            for (int a = 0, counter = 1; a < player.HitsDoctored + 1; a++)
            {
                if (a > 40) // will not start calculating increase to costAdd until hitpoint 41
                {
                    if (counter == 5) // cost increase every 5 hit points
                    {
                        counter = 0; // reset to 0, will increase after this if statement

                        if (costAdd < 28) // at cost 28 start increasing by 100 every 10 hits
                            costAdd++;
                        else costAdd += 100;
                    }

                    counter++; // increase every time through the loop
                }

                cost += costAdd;
            }

            return cost;
        }

        public static long Formula_SkillsLossAtDeath(int skillLevel)
        {
            return Convert.ToInt64(Skills.GetSkillForLevel(skillLevel) / 8);
        }

        /// <summary>
        /// Roll and check vs. full stat. Returns true if check is successful.
        /// </summary>
        /// <param name="ch">The character whose stat will be checked.</param>
        /// <param name="stat">The stat.</param>
        /// <returns>True if pass, false otherwise.</returns>
        public static bool FullStatCheck(Character ch, Globals.eAbilityStat stat)
        {
            if (ch.IsImmortal) return true;

            return RollD(1, 20) < GetFullAbilityStat(ch, stat);
        }

        /// <summary>
        /// Full stat check is base stat + temporary stat vs a d20. Negative modifier helps pass the check. 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="stat"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static bool FullStatCheck(Character ch, Globals.eAbilityStat stat, int modifier)
        {
            if (ch.IsImmortal) return true;

            return RollD(1, 20) + modifier < GetFullAbilityStat(ch, stat);
        }

        public static double Formula_AbilityModifier(Character ch, Globals.eAbilityStat stat)
        {
            return Convert.ToInt16(GetFullAbilityStat(ch, stat) - 10 / 2);
        }

        public static int GetFullAbilityStat(Character ch, Globals.eAbilityStat stat)
        {
            switch (stat)
            {
                case Globals.eAbilityStat.Strength:
                    return ch.Strength + ch.TempStrength;
                case Globals.eAbilityStat.Dexterity:
                    return ch.Dexterity + ch.TempDexterity;
                case Globals.eAbilityStat.Intelligence:
                    return ch.Intelligence + ch.TempIntelligence;
                case Globals.eAbilityStat.Wisdom:
                    return ch.Wisdom + ch.TempWisdom;
                case Globals.eAbilityStat.Constitution:
                    return ch.Constitution + ch.TempConstitution;
                case Globals.eAbilityStat.Charisma:
                    return ch.Charisma + ch.TempCharisma;
                default:
                    return 0;
            }
        }

        public static int GetGenericStatModifier(Character ch, Globals.eAbilityStat stat)
        {
            //return Formula_AbilityModifier(ch, stat);
            return GetFullAbilityStat(ch, stat) - 15;
        }

        public static int RollD(int number, int sides)
        {
            int roll = 0;

            for (int a = 0; a < number; a++)
                roll += Dice.Next(1, sides + 1);

            return roll;
        }

        public static int GetExpLevel(long exp)
        {
            if (exp > Globals.EXP_LEVEL_20) return GetExpLevelPostLevel20(exp);

            long low = Globals.EXP_LEVEL_3;
            long high = Globals.EXP_LEVEL_3 * 2;

            for (int a = 3; a <= Globals.MAX_EXP_LEVEL; a++)
            {
                if (exp >= low && exp < high)
                    return a;

                low = high;

                high = high * 2;
            }
            return 3;
        }

        public static long GetExperienceRequiredForLevel(int level)
        {
            long experience = Globals.EXP_LEVEL_3;

            for (int a = 3, b = 0, c = 1; a <= Globals.MAX_EXP_LEVEL; a++)
            {
                if (a == level)
                    return experience;

                if (a <= 20)
                {
                    experience = experience * 2;
                }
                else
                {
                    experience = experience + (Globals.EXP_LEVEL_20 * c);
                    b++;

                    if (b == 2)
                    {
                        c++;
                        b = 0;
                    }
                }
            }

            return experience;
        }

        public static int GetExpLevelPostLevel20(long exp)
        {
            long experienceCurve = Globals.EXP_LEVEL_20;
            long low = experienceCurve;
            long high = experienceCurve * 2;

            for (int a = 20, b = 0, c = 1; a <= Globals.MAX_EXP_LEVEL; a++)
            {
                if (b == 2)
                {
                    c++;
                    b = 0;
                }
                experienceCurve = Globals.EXP_LEVEL_20 * c;
                b++;

                if (exp >= low && exp < high)
                    return a;

                low = high;

                high = high + experienceCurve;
            }
            return 20;
        }

        public static void GiveAEKillExp(Character ch, Character target)
        {
            try
            {
                if (ch == null || target == null) { return; }
                if (ch.IsDead) { return; }
                if (!target.special.Contains("figurine") && (target is NPC) && (target as NPC).IsSummoned) { return; } // no experience for summoned creatures?
                if (ch.MapID != target.MapID) { return; } // no experience gain across maps
                // random amount is used for variety only
                var randomExpGain = Rules.Dice.Next((int)-(target.Experience * .05), (int)(target.Experience * .05)); // random xp adjustment

                var expGain = Convert.ToInt64(target.Experience + randomExpGain); // total xp gain value

                expGain = (long)(expGain * .432); // AE kill experience is lessened

                // Accelerated experience gain option.
                if (DragonsSpineMain.Instance.Settings.AcceleratedExperienceGain)
                    expGain = Convert.ToInt64(expGain * DragonsSpineMain.Instance.Settings.AcceleratedExperienceGainMultiplier);

                // more experience gain
                if(ch.HasEffect(Effect.EffectTypes.Bazymon__s_Bounty, out Effect bazyEffect))
                {
                    if (bazyEffect.Power <= 2)
                        expGain += expGain / 2;
                    else expGain = expGain * bazyEffect.Power;
                }

                // less experience gain
                if(ch.HasEffect(Effect.EffectTypes.The_Withering, out Effect theWithering))
                {
                    if (theWithering.Power <= 2)
                        expGain -= expGain / 2;
                    else expGain = expGain / theWithering.Power;
                }

                if((ch.IsPC || (ch.PetOwner != null && ch.PetOwner.IsPC)) && !target.IsPC)
                {
                    if (ch.Group != null)
                        ch.Group.GiveGroupExperience(ch, expGain, ch.Name + " has slain a foe.");
                    else
                    {
                        // If this is a pet that killed something give the pet owner experience.
                        if (ch.PetOwner != null)
                        {
                            if (ch.special.Contains("figurine")) // If this is a pet that is spawned from a figurine, give it the experience.
                            {
                                ch.Experience += expGain;
                                return;
                            }

                            ch.PetOwner.Experience += expGain;
                        }
                        else
                        {
                            ch.Experience += expGain;
                        }

                        ch.Kills++;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void GiveKillExp(Character ch, Character target)
        {
            try
            {
                if (ch == null || target == null) return;

                if (ch.IsDead) return; // characters that are dead do not earn experience

                //if (ch.MapID != target.MapID) return; // creature died on another map (maybe a damage over time, or delayed effect...)

                if (target.PetOwner != null && target.PetOwner == ch) return;

                //if (target.special.Contains("figurine")) return; // no experience for figurines

                int randomExpGain = Rules.Dice.Next((int)-(target.Experience * .05), (int)(target.Experience * .05)); // random xp adjustment

                long expGain = Convert.ToInt64(target.Experience + randomExpGain); // total xp gain value

                // Fighter's bonus experience gain.
                if (ch.BaseProfession == Character.ClassType.Fighter && EntityLists.IsHumanOrHumanoid(ch))
                    expGain = expGain + (int)(expGain * (ch.Level * 2) / 100);

                // Accelerated experience gain option.
                if (DragonsSpineMain.Instance.Settings.AcceleratedExperienceGain)
                    expGain = Convert.ToInt64(expGain * DragonsSpineMain.Instance.Settings.AcceleratedExperienceGainMultiplier);

                // Bazymon's Bounty -- more experience gain
                if (ch.HasEffect(Effect.EffectTypes.Bazymon__s_Bounty, out Effect bazyEffect))
                {
                    if (bazyEffect.Power <= 2)
                        expGain += expGain / 2;
                    else expGain = expGain * bazyEffect.Power;
                }

                // The Withering -- less experience gain
                if (ch.HasEffect(Effect.EffectTypes.The_Withering, out Effect theWithering))
                {
                    if (theWithering.Power <= 2)
                        expGain -= expGain / 2;
                    else expGain = expGain / theWithering.Power;
                }

                // Experience for killing NPCs by PCs and pets owned by PCs.
                if ((ch.IsPC || (ch.PetOwner != null && ch.PetOwner.IsPC)) && !target.IsPC)
                {
                    if (ch.IsPC && ch.Group != null)
                    {
                        ch.Group.GiveGroupExperience(ch, expGain, ch.Name + " has slain a foe.");

                        //TODO: handle quest flags in player groups
                    }
                    else
                    {
                        // If this is a pet that killed something give the pet owner experience.
                        if (ch.PetOwner != null)
                        {
                            if(ch.special.Contains("figurine")) // If this is a pet that is spawned from a figurine, give it the experience.
                            {
                                ch.Experience += expGain / 3 * 2;
                                ch.PetOwner.Experience += expGain / 3;
                                return;
                            }
                            
                            ch.PetOwner.Experience += expGain;
                        }
                        else
                        {
                            ch.Experience += expGain;
                        }

                        ch.Kills++;

                        #region Quest Flags
                        if (target is NPC && (target as NPC).QuestFlags.Count > 0)
                        {
                            foreach (string flag in (target as NPC).QuestFlags)
                            {
                                if (!ch.QuestFlags.Contains(flag))
                                {
                                    string[] s = flag.Split(ProtocolYuusha.VSPLIT.ToCharArray());
                                    // verify that the player has the quest in order to get the flag
                                    // if the questID parse results in 0 then the quest does not need to be started to get the flag
                                    int questID = Convert.ToInt32(s[0]);
                                    if (questID <= 0 || ch.GetQuest(questID) != null)
                                    {
                                        if (ch.GetQuest(Convert.ToInt32(s[0])) != null)
                                        {
                                            if (!ch.QuestFlags.Contains(flag))
                                            {
                                                ch.QuestFlags.Add(flag);
                                                ch.WriteToDisplay("You have received a quest flag!");
                                            }
                                        }
                                    }
                                }
                            }
                        } 
                        #endregion

                        Utils.Log(ch.GetLogString() + " earned " + expGain + " for killing " + target.GetLogString() + ".", Utils.LogType.ExperienceMeleeKill);
                    }
                }
                else if (!target.IsPC && !ch.IsPC) // NPCs killing NPCs (does not include pet kills)
                {
                    ch.Experience += expGain;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void MakeSpecialCorpse(NPC target)
        {
            if (target == null) return;

            if (target.CurrentCell != null)
            {
                // do not drop worn items for spectral or summoned creatures
                if (!target.IsSpectral && !target.IsSummoned)
                {
                    foreach (Item item in new List<Item>(target.wearing))
                    {
                        if (item != null)
                            target.CurrentCell.Add(item);
                    }
                }

                foreach (Item item in new List<Item>(target.pouchList))
                {
                    if (item != null)
                        target.CurrentCell.Add(item);
                }

                foreach (Item item in new List<Item>(target.sackList))
                {
                    if (item != null)
                        target.CurrentCell.Add(item);
                }

                foreach (Item item in new List<Item>(target.beltList))
                {
                    if (item != null)
                        target.CurrentCell.Add(item);
                }

                if (target.RightHand != null)
                    target.CurrentCell.Add(target.RightHand);

                if (target.LeftHand != null)
                    target.CurrentCell.Add(target.LeftHand);
            }

            // decrement the number of this NPC in the list
            if (World.GetFacetByID(target.FacetID).Spawns.ContainsKey(target.SpawnZoneID))
                World.GetFacetByID(target.FacetID).Spawns[target.SpawnZoneID].NumberInZone--;

            target.RemoveFromWorld();
        }

        public static void UnderworldDeadRest(Character chr)
        {
            if ((chr as PC).currentKarma > 0)
                chr.CurrentCell = Cell.GetCell(chr.FacetID, chr.LandID, chr.MapID, chr.Map.KarmaResX, chr.Map.KarmaResY, chr.Map.KarmaResZ);
            else
                chr.CurrentCell = Cell.GetCell(chr.FacetID, chr.LandID, chr.MapID, chr.Map.ResX, chr.Map.ResY, chr.Map.ResZ);
            
            chr.IsDead = false;
            chr.Hits = chr.HitsMax;
            chr.updateAll = true;
            chr.Stamina = chr.StaminaMax;
            chr.IsHidden = false;
            chr.IsInvisible = false;
            chr.Stunned = 0;
            chr.Mana = 0;
        }

        /// <summary>
        /// This is called when a dead PC uses the rest command (the only command available when dead).
        /// </summary>
        /// <param name="pc">The dead Player Character.</param>
        public static void DeadRest(PC pc) // called when a dead player uses the rest command
        {
            try
            {
                //remove the player's corpse from the cell 
                //TODO: Instead of removing the corpse, dump players items into corpse - strip player
                //TODO: use Corpse.Ghost for identification
                foreach (Item item in pc.CurrentCell.Items)
                {
                    if (item.special == pc.Name)
                    {
                        pc.CurrentCell.Remove(item);
                        break;
                    }
                }

                pc.WriteToDisplay("You've gained the attention of a passing Ghod!");

                pc.SendSound(Sound.GetCommonSound(Sound.CommonSound.DeathRevive));

                #region Death Penalties
                if (pc.Level >= 5)
                {
                    #region Skill loss (top two highest skill levels).
                    //int numSkills = 1;

                    //if (pc.Level >= 10)
                    //    numSkills = 2;

                    //Globals.eSkillType[] skillsToLose = Skills.GetXHighestSkills(pc, numSkills);

                    //foreach (Globals.eSkillType skillType in skillsToLose)
                    //{
                    //    if (Skills.GetSkillLevel(pc, skillType) >= 5)
                    //        Skills.SkillLoss(pc, skillType, Formula_SkillsLossAtDeath(Skills.GetSkillLevel(pc.GetSkillExperience(skillType))));
                    //}
                    #endregion

                    // 15% chance to drop strength if above 3
                    if (pc.Strength > 3 && RollD(1, 100) < 15)
                    {
                        pc.Strength--;
                        pc.WriteToDisplay("You have lost 1 strength point.");
                    }

                    // 5% chance to drop dexterity if above 3
                    if (pc.Dexterity > 3 && RollD(1, 100) < 5)
                    {
                        pc.Dexterity--;
                        pc.WriteToDisplay("You have lost 1 dexterity point.");
                    }

                    // 5% chance to drop intelligence if above 3
                    if (pc.Intelligence > 3 && RollD(1, 100) < 5)
                    {
                        pc.Intelligence--;
                        pc.WriteToDisplay("You have lost 1 intelligence point.");
                    }

                    // 5% chance to drop wisdom if above 3
                    if (pc.Wisdom > 3 && RollD(1, 100) < 5)
                    {
                        pc.Wisdom--;
                        pc.WriteToDisplay("You have lost 1 wisdom point.");
                    }

                    // 25% chance to drop constitution if greater than 3
                    #region Constitution loss.
                    if (pc.Constitution > 3 && RollD(1, 100) < 25)
                    {
                        // 1/20 chance to drop 2 points of constitution if con will stay at 10 or above
                        if (RollD(1, 20) == 1 && pc.Constitution >= 12)
                        {
                            pc.Constitution -= 2;
                            pc.WriteToDisplay("You have lost 2 constitution points.");
                        }
                        else
                        {
                            pc.Constitution--;
                            pc.WriteToDisplay("You have lost 1 constitution point.");

                        }
                    }
                    #endregion

                    // 10% chance to drop charisma if above 3
                    if (pc.Charisma > 3 && RollD(1, 100) < 5)
                    {
                        pc.Charisma--;
                        pc.WriteToDisplay("You have lost 1 charisma point.");
                    }
                }
                #endregion

                if (pc.BaseProfession == Character.ClassType.Thief)
                {
                    if (World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ThiefResX != -1)
                    {
                        pc.CurrentCell = Cell.GetCell(pc.FacetID, pc.LandID, pc.MapID,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ThiefResX,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ThiefResY,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ThiefResZ);
                    }
                    else
                    {
                        pc.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                    }
                }
                else if (pc.Alignment == Globals.eAlignment.Evil || pc.Alignment == Globals.eAlignment.ChaoticEvil)
                {
                    if (World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).EvilResX != -1)
                    {
                        pc.CurrentCell = Cell.GetCell(pc.FacetID, pc.LandID, pc.MapID,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).EvilResX,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).EvilResY,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).EvilResZ);
                    }
                    else
                    {
                        pc.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                    }
                }
                // character has karma or alignment is not lawful
                else if (pc.currentKarma > 0 || pc.Alignment == Globals.eAlignment.Chaotic)
                {
                    // character is evil
                    if (World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).KarmaResX != -1)
                    {
                        pc.CurrentCell = Cell.GetCell(pc.FacetID, pc.LandID, pc.MapID,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.CurrentCell.MapID).KarmaResX,
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).KarmaResY,
                             World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).KarmaResZ);
                    }
                    else
                    {
                        pc.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                        //Rules.EnterUnderworld(pc);
                        //return;
                    }
                }
                else // character has no karma and is lawful or neutral
                {
                    if (World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ResX != -1)
                    {
                        pc.CurrentCell = Cell.GetCell(pc.FacetID, pc.LandID, pc.MapID, 
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ResX, 
                            World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ResY, 
                             World.GetFacetByID(pc.FacetID).GetLandByID(pc.LandID).GetMapByID(pc.MapID).ResZ);
                    }
                    else
                    {
                        pc.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                    }
                }

                pc.IsDead = false;
                pc.Hits = 1;
                pc.updateAll = true;
                pc.Stamina = 1;
                pc.IsHidden = false;
                pc.IsInvisible = false;
                pc.Stunned = 0;

                if(pc.ManaFull > 0)
                    pc.Mana = 1;

                // lessened experience gain for 1 minute
                Effect.CreateCharacterEffect(Effect.EffectTypes.The_Withering, 2, pc, Utils.TimeSpanToRounds(new TimeSpan(0, 1, 0)), null);
                // lessened skill gain for 1 minute
                Effect.CreateCharacterEffect(Effect.EffectTypes.Drudgery, 2, pc, Utils.TimeSpanToRounds(new TimeSpan(0, 1, 0)), null);
            }
            catch (Exception e)
            {
                pc.WriteToDisplay("Error during rest function. Please report this.");
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// Place all worn rings into a Corpse object.
        /// </summary>
        /// <param name="target">The Character being turned into a Corpse.</param>
        /// <param name="corpse">The Corpse being created.</param>
        public static void CorpseRings(Character target, Corpse corpse)
        {
            // TODO: check tanning result for ring items and do not add them to corpse if they are tanned?
            if (target.RightRing1 != null) { corpse.Contents.Add(target.RightRing1); target.RightRing1 = null; }
            if (target.RightRing2 != null) { corpse.Contents.Add(target.RightRing2); target.RightRing2 = null; }
            if (target.RightRing3 != null) { corpse.Contents.Add(target.RightRing3); target.RightRing3 = null; }
            if (target.RightRing4 != null) { corpse.Contents.Add(target.RightRing4); target.RightRing4 = null; }
            if (target.LeftRing1 != null) { corpse.Contents.Add(target.LeftRing1); target.LeftRing1 = null; }
            if (target.LeftRing2 != null) { corpse.Contents.Add(target.LeftRing2); target.LeftRing2 = null; }
            if (target.LeftRing3 != null) { corpse.Contents.Add(target.LeftRing3); target.LeftRing3 = null; }
            if (target.LeftRing4 != null) { corpse.Contents.Add(target.LeftRing4); target.LeftRing4 = null; }
        }

        /// <summary>
        /// Place all sack Items into a Corpse object.
        /// </summary>
        /// <param name="target">The Character being turned into a Corpse.</param>
        /// <param name="corpse">The Corpse being created.</param>
        public static void CorpseSack(Character target, Corpse corpse)
        {
            if(target.sackList.Count > 0)
            {
                foreach(Item item in new List<Item>(target.sackList))
                {
                    if (item != null)
                    {
                        // if armor is in the tanningResult it doesn't get added to the corpse
                        
                        if(!(target is NPC))
                        {
                            // target is not an npc
                            corpse.Contents.Add(item);
                        }
                        else if((target is NPC))
                        {
                            if((target as NPC).tanningResult == null || (target as NPC).tanningResult.Count <= 0) // mob does not have any tanning items
                            {
                                // add the item to the corpse
                                corpse.Contents.Add(item);
                            }
                            else if ((target as NPC).tanningResult.Count >= 1) // mob has tanning items
                            {
                                if ((target as NPC).tanningResult.ContainsKey(item.itemID)) // item is in the tanning list
                                {
                                    // dont add the item to the corpse
                                }
                                else
                                {
                                    corpse.Contents.Add(item); // item is not in the tanning list
                                }
                            }
                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Place all sack Items into a Corpse object.
        /// </summary>
        /// <param name="target">The Character being turned into a Corpse.</param>
        /// <param name="corpse">The Corpse being created.</param>
        public static void CorpsePouch(Character target, Corpse corpse)
        {
            if (target.pouchList.Count > 0)
            {
                foreach (Item item in new List<Item>(target.pouchList))
                {
                    if (item != null)
                    {
                        // if armor is in the tanningResult it doesn't get added to the corpse

                        if (!(target is NPC))
                        {
                            // target is not an npc
                            corpse.Contents.Add(item);
                        }
                        else if ((target is NPC))
                        {
                            if ((target as NPC).tanningResult == null || (target as NPC).tanningResult.Count <= 0) // mob does not have any tanning items
                            {
                                // add the item to the corpse
                                corpse.Contents.Add(item);
                            }
                            else if ((target as NPC).tanningResult.Count >= 1) // mob has tanning items
                            {
                                if ((target as NPC).tanningResult.ContainsKey(item.itemID)) // item is in the tanning list
                                {
                                    // dont add the item to the corpse
                                }
                                else
                                {
                                    corpse.Contents.Add(item); // item is not in the tanning list
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Place all belt Items into a Corpse object.
        /// </summary>
        /// <param name="target">The Character being turned into a Corpse.</param>
        /// <param name="corpse">The Corpse being created.</param>
        public static void CorpseBelt(Character target, Corpse corpse)
        {
            if(target.beltList.Count > 0)
            {
                foreach(Item item in new List<Item>(target.beltList))
                {
                    if (item != null)
                    {
                        // if armor is in the tanningResult it doesn't get added to the corpse

                        if (!(target is NPC))
                        {
                            // target is not an npc
                            corpse.Contents.Add(item);
                        }
                        else if ((target is NPC))
                        {
                            if ((target as NPC).tanningResult == null || (target as NPC).tanningResult.Count <= 0) // mob does not have any tanning items
                            {
                                // add the item to the corpse
                                corpse.Contents.Add(item);
                            }
                            else if ((target as NPC).tanningResult.Count >= 1) // mob has tanning items
                            {
                                if ((target as NPC).tanningResult.ContainsKey(item.itemID)) // item is in the tanning list
                                {
                                    // dont add the item to the corpse
                                }
                                else
                                {
                                    corpse.Contents.Add(item); // item is not in the tanning list
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Place all worn Items into a Corpse object.
        /// </summary>
        /// <param name="target">The Character being turned into a Corpse.</param>
        /// <param name="corpse">The Corpse being created.</param>
        public static void CorpseWearing(Character target, Corpse corpse)
        {
            if (target.wearing.Count > 0)
            {
                foreach (Item item in new List<Item>(target.wearing))
                {
                    if (item != null)
                    {
                        if (!(target is NPC))
                        {
                            corpse.Contents.Add(item);
                        }
                        else if ((target as NPC).tanningResult == null || (target as NPC).tanningResult.Count == 0)
                        {
                            corpse.Contents.Add(item);
                        }
                        else if ((target as NPC).tanningResult != null &&
                            (target as NPC).tanningResult.Count > 0 &&
                            !(target as NPC).tanningResult.ContainsKey(item.itemID))
                        {
                            corpse.Contents.Add(item);
                        }
                    }
                }
            }
        }

        public static void SpawnFigurine(Item figurine, Cell targetCell, Character chr)
        {
            // Check character's hands for the figurine and unequip it.
            if (chr.RightHand != null && chr.RightHand == figurine)
                chr.UnequipRightHand(figurine);
            else if (chr.LeftHand != null && chr.LeftHand == figurine)
                chr.UnequipLeftHand(figurine);

            string message = "";

            NPC figSpirit = NPC.LoadNPC(Item.ID_SUMMONEDMOB, targetCell.FacetID, targetCell.LandID, targetCell.MapID, targetCell.X, targetCell.Y, targetCell.Z, -1);

            figSpirit.animal = true;
            figSpirit.IsSummoned = true;

            Item armor = null;

            EntityBuilder builder = new EntityBuilder();

            #region Set attributes.
            figSpirit.Level = Skills.GetSkillLevel(figurine.figExp) + 1;
            figSpirit.Experience = figurine.figExp;

            builder.SetOnTheFlyVariables(figSpirit);

            figSpirit.IsSummoned = true;
            figSpirit.IsUndead = false;

            int oneMinute = Utils.TimeSpanToRounds(new TimeSpan(0, 1, 0));
            // 5 minutes per level of the figurine
            figSpirit.RoundsRemaining = (oneMinute * figSpirit.Level);
            figSpirit.Age = 0;

            figSpirit.Alignment = chr.Alignment;
            figSpirit.special = figurine.special;
            figSpirit.BaseProfession = Character.ClassType.Fighter;
            figSpirit.classFullName = "Fighter";
            figSpirit.IsMobile = true;
            figSpirit.animal = true;
            figSpirit.unarmed = Skills.GetSkillToNext(figSpirit.Level);
            figSpirit.canCommand = true;
            figSpirit.FollowID = chr.UniqueID;
            chr.Pets.Add(figSpirit);
            figSpirit.PetOwner = chr;
            #endregion

            if (figurine.special.ToLower().IndexOf("snake") > -1)
            {
                #region Ebon Snake Staff
                armor = Item.CopyItemFromDictionary(8113); // scales
                figSpirit.Name = "snake";
                figSpirit.idleSound = "0017";
                figSpirit.deathSound = "0041";
                figSpirit.attackSound = "0029";
                figSpirit.Speed = 3;
                figSpirit.Strength = 3 + figSpirit.Level;
                figSpirit.Dexterity = 12 + figSpirit.Level;
                figSpirit.Constitution = 4 + figSpirit.Level;
                figSpirit.Wisdom = 2 + figSpirit.Level;
                figSpirit.Charisma = 2 + figSpirit.Level;
                figSpirit.Intelligence = 2 + figSpirit.Level;
                message = "the hiss of a snake.";
                figSpirit.shortDesc = "snake";
                figSpirit.longDesc = "a snake";
                figSpirit.species = Globals.eSpecies.Reptile;
                figSpirit.entity = EntityLists.Entity.Snake;
                figSpirit.visualKey = "snake";
                #endregion
            }
            else if (figurine.special.ToLower().IndexOf("tiger") > -1)
            {
                #region Tiger Figurine
                armor = Item.CopyItemFromDictionary(7204); // fur
                figSpirit.Name = "tiger";
                figSpirit.idleSound = "0015";
                figSpirit.deathSound = "0039";
                figSpirit.attackSound = "0027";
                figSpirit.Speed = 2;
                figSpirit.Strength = 7 + (figSpirit.Level / 3);
                figSpirit.Dexterity = 8 + (figSpirit.Level / 3);
                figSpirit.Constitution = 6 + (figSpirit.Level / 3);
                figSpirit.Wisdom = 3 + (figSpirit.Level / 3);
                figSpirit.Charisma = 3 + (figSpirit.Level / 4);
                figSpirit.Intelligence = 3 + (figSpirit.Level / 3);
                figSpirit.shortDesc = "tiger";
                figSpirit.longDesc = "a tiger";
                message = "a tiger roar!";
                figSpirit.species = Globals.eSpecies.WildAnimal;
                figSpirit.entity = EntityLists.Entity.Tiger;
                figSpirit.visualKey = "tiger";
                #endregion
            }
            else if (figurine.special.ToLower().IndexOf("griffin") > -1)
            {
                #region Griffin Figurine
                armor = Item.CopyItemFromDictionary(6200); // feathers
                figSpirit.Name = "griffin";
                figSpirit.idleSound = "0009";
                figSpirit.deathSound = "0033";
                figSpirit.attackSound = "0021";
                figSpirit.Speed = 2;
                figSpirit.Strength = 10 + figSpirit.Level;
                figSpirit.Dexterity = 8 + figSpirit.Level;
                figSpirit.Constitution = 9 + figSpirit.Level;
                figSpirit.Wisdom = 3 + (figSpirit.Level / 3);
                figSpirit.Charisma = 3 + (figSpirit.Level / 4);
                figSpirit.Intelligence = 3 + (figSpirit.Level / 3);
                figSpirit.shortDesc = "griffin";
                figSpirit.longDesc = "a griffin";
                message = "a griffin screech!";
                figSpirit.species = Globals.eSpecies.Avian;
                figSpirit.entity = EntityLists.Entity.Griffin;
                figSpirit.visualKey = "griffin";
                #endregion
            }
            else if (figurine.special.ToLower().IndexOf("firedragon") > -1)
            {
                #region Fire Dragon Figurine
                armor = Item.CopyItemFromDictionary(8107); // Young dragon
                figSpirit.Name = "dragon";
                figSpirit.idleSound = "0019";
                figSpirit.deathSound = "0043";
                figSpirit.attackSound = "0031";
                figSpirit.Speed = 4;
                figSpirit.Strength = 12 + (figSpirit.Level / 3);
                figSpirit.Dexterity = 10 + (figSpirit.Level / 3);
                figSpirit.Constitution = 11 + (figSpirit.Level / 3);
                figSpirit.Wisdom = 6 + (figSpirit.Level / 3);
                figSpirit.Charisma = 5 + (figSpirit.Level / 4);
                figSpirit.Intelligence = 7 + (figSpirit.Level / 3);
                figSpirit.shortDesc = "dragon";
                figSpirit.longDesc = "a dragon";
                message = "a dragon roar!";
                figSpirit.castMode = NPC.CastMode.Unlimited;
                figSpirit.species = Globals.eSpecies.FireDragon;
                figSpirit.entity = EntityLists.Entity.Red_Dragon;
                figSpirit.magic = Skills.GetSkillToNext(figSpirit.Level);
                figSpirit.visualKey = "red_dragon";
                #endregion
            }
            else if (figurine.special.ToLower().IndexOf("drake") > -1)
            {
                #region Lightning Drake
                armor = Item.CopyItemFromDictionary(8109); // drake
                figSpirit.Name = "drake";
                figSpirit.idleSound = "0019";
                figSpirit.deathSound = "0043";
                figSpirit.attackSound = "0031";
                figSpirit.shortDesc = "lightning drake";
                figSpirit.longDesc = "a lightning drake";
                figSpirit.castMode = NPC.CastMode.Unlimited;
                figSpirit.species = Globals.eSpecies.LightningDrake;
                figSpirit.entity = EntityLists.Entity.Drake;
                figSpirit.Speed = 4;
                figSpirit.Strength = 14 + (figSpirit.Level / 3);
                figSpirit.Dexterity = 11 + (figSpirit.Level / 3);
                figSpirit.Constitution = 12 + (figSpirit.Level / 3);
                figSpirit.Wisdom = 6 + (figSpirit.Level / 3);
                figSpirit.Charisma = 5 + (figSpirit.Level / 4);
                figSpirit.Intelligence = 7 + (figSpirit.Level / 3);
                message = "a drake roar!";
                figSpirit.castMode = NPC.CastMode.Unlimited;
                figSpirit.magic = Skills.GetSkillToNext(figSpirit.Level); 
                #endregion
            }

            figSpirit.Hits = figSpirit.HitsFull;
            figSpirit.Mana = figSpirit.ManaFull;
            figSpirit.Stamina = figSpirit.StaminaFull;

            // Check figurine's hands for the figurine and unequip it.
            if (figSpirit.RightHand != null)
                figSpirit.UnequipRightHand(figSpirit.RightHand);
            if (figSpirit.LeftHand != null)
                figSpirit.UnequipLeftHand(figSpirit.LeftHand);

            foreach (Item item in new List<Item>(figSpirit.wearing))
                figSpirit.RemoveWornItem(item);

            if (armor != null)
            {
                figSpirit.WearItem(armor);

                figSpirit.tanningResult = new Dictionary<int, Autonomy.ItemBuilding.LootManager.LootRarityLevel>
                {
                    { armor.itemID, Autonomy.ItemBuilding.LootManager.LootRarityLevel.Never }
                };
            }

            figSpirit.visualKey = "blue_drake";
            figSpirit.CurrentCell = targetCell;
            figSpirit.CurrentCell.SendShout(message);
            figSpirit.CurrentCell.EmitSound(figSpirit.attackSound);

            figSpirit.AddToWorld();

            Utils.Log(figSpirit.GetLogString() + " [Final Hits: " + figSpirit.HitsFull + "] [Final Mana: " + figSpirit.ManaFull + "] Rounds: " + figSpirit.RoundsRemaining + " Owner: " + figSpirit.PetOwner != null ? figSpirit.PetOwner.GetLogString() : "None", Utils.LogType.ItemFigurineUse);
        }

        public static void DespawnFigurine(NPC target)
        {
            Item fig = null;

            try
            {
                if (target.special.Contains("snake"))
                    fig = Item.CopyItemFromDictionary(Item.ID_EBONSNAKESTAFF);
                else if (target.special.Contains("tiger"))
                    fig = Item.CopyItemFromDictionary(Item.ID_TIGERFIG);
                else if (target.special.Contains("drake"))
                    fig = Item.CopyItemFromDictionary(Item.ID_DRAKEFIG);
                else if (target.special.Contains("firedragon"))
                    fig = Item.CopyItemFromDictionary(Item.ID_DRAGONFIG);
                else if (target.special.Contains("griffin"))
                    fig = Item.CopyItemFromDictionary(Item.ID_GRIFFINFIG);

                if (fig != null)
                {
                    fig.coinValue = (int)(target.Experience / 2);
                    fig.figExp = target.Experience;
                    target.CurrentCell.Add(fig);
                }
                else
                {
                    target.SendToAllInSight("There was error despawning the figurine. The error has been logged.");
                    Utils.Log("Error while despawning figurine with special tag: " + target.special + ".", Utils.LogType.Unknown);
                }

                if (target.PetOwner != null)
                {
                    if (target.PetOwner.Pets.Contains(target))
                        target.PetOwner.Pets.Remove(target);

                    target.PetOwner = null;
                }

                target.EmitSound(target.deathSound);
                target.RemoveFromWorld();
            }
            catch (Exception)
            {
                Utils.Log("Error while despawning figurine with special tag: " + target.special + ".", Utils.LogType.Unknown);
                target.RemoveFromWorld();
            }
        }

        public static void UnsummonCreature(NPC target)
        {
            if (target.special.Contains("figurine"))
            {
                DespawnFigurine(target);
                return;
            }

            if (!target.IsSummoned) return;

            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                target.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();

            // death or unsummoning sound
            if (target.special.ToLower().Contains("summonnaturesally") || target.special.ToLower().Contains("summonthief"))
                target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.LongLowWhoosh));
            else target.EmitSound(target.deathSound);

            // Summoned creatures don't "have" pouches or sacks, or lockers. Just inventory and right/left hand.
            List<Item> nonEtherealInventory = new List<Item>();
            foreach(Item item in target.wearing)
            {
                if(!item.special.Contains(Item.EXTRAPLANAR))
                {
                    nonEtherealInventory.Add(item);
                    if (target.CurrentCell != null)
                        target.CurrentCell.Add(item);
                }
            }

            foreach (Item item in nonEtherealInventory)
                target.RemoveWornItem(item);

            // reset named demons
            if (EntityLists.NAMED_DEMONS.Contains(target.entity))
            {
                Map hell = target.Land.GetMapByID(World.MAP_HELL);
                List<Tuple<int, int, int>> cellsList = new List<Tuple<int, int, int>>(hell.cells.Keys);

                Cell destinationCell = null;

                do
                {
                    destinationCell = hell.cells[cellsList[Rules.Dice.Next(cellsList.Count)]];
                }
                while (!Spells.ChaosPortalSpell.CellMeetsTeleportRequirements(destinationCell));

                target.Hits = target.HitsFull;
                target.Mana = target.ManaFull;
                target.Stamina = target.StaminaFull;
                target.aiType = NPC.AIType.Unknown;
                target.IsSummoned = false;
                target.Name = (string)target.TemporaryStorage; // name was temporarily stored here
                target.Alignment = Globals.eAlignment.Chaotic; // all demons return to chaotic alignment
                target.canCommand = false;
                return;
            }            

            if (World.GetFacetByID(target.FacetID).Spawns.ContainsKey(target.SpawnZoneID))
                World.GetFacetByID(target.FacetID).Spawns[target.SpawnZoneID].NumberInZone--;

            if (target.PetOwner != null)
            {
                if(target.PetOwner.Pets.Contains(target))
                    target.PetOwner.Pets.Remove(target);
                
                target.PetOwner = null;
            }

            target.RemoveFromWorld();
        }

        /// <summary>
        /// Karma is currently used only for PC (Players) objects. 12/9/2015 Eb
        /// </summary>
        /// <param name="target"></param>
        /// <param name="killer"></param>
        public static void DoKarma(Character target, Character killer)
        {
            if (target == null || killer == null || target.IsImage || !(killer is PC) || killer == target) return; // no karma for these situations

            // Avoidance of karma if the player is lawful and the attacking "killer" is lawful.
            if (target.Map.ZPlanes[target.Z].isCoexistent)
            {
                if (killer.Alignment != Globals.eAlignment.Evil)
                    return;

                // Killer is a lawful (player) and was not flagged as hostile.
                //if (killer.Alignment == Globals.eAlignment.Lawful && !target.PlayersFlagged.Contains(killer.UniqueID))
                //{
                //    return;
                //}
            }

            // killer is not null, killer is a player, and killer did not kill itself
            if (killer != target)
            {
                PC pc = (PC)killer;

                // only regular users can get karma
                if (pc.ImpLevel == Globals.eImpLevel.USER)
                {
                    // target is not a player
                    if (!target.IsPC)
                    {
                        // only give karma if the target is lawful
                        if (target.Alignment == Globals.eAlignment.Lawful)
                        {
                            pc.currentKarma++; // this may be lowered by turning in tiger figurines to a confessor ghost
                            pc.lifetimeKarma++; // this never decreases

                            killer.WriteToDisplay("You have received karma for the slaying of " + target.Name + ".");

                            // first ever karma
                            //if(pc.lifetimeKarma == 1)
                            //{
                            //    killer.WriteToDisplay("Accruing karma can be good however it is usually bad. If you're evil then the more karma you have the better off you are among similarly-minded beings.");
                            //    killer.WriteToDisplay("If you're not evil, karma isn't all that beneficial and it means you will attract unwanted attention.");
                            //}

                            Utils.Log(killer.GetLogString() + " has received karma for killing " + target.GetLogString() + ".", Utils.LogType.Karma);
                            
                            // killer will change alignment if they are lawful
                            if (killer.Alignment == Globals.eAlignment.Lawful)
                            {
                                killer.Alignment = Globals.eAlignment.Neutral;
                                killer.WriteToDisplay("Your alignment has changed to " + killer.Alignment.ToString() +".");
                            }
                            else if (killer.Alignment == Globals.eAlignment.Neutral)
                            {
                                // at 4 karma a neutral player will become evil
                                if ((killer as PC).currentKarma >= NUM_KARMA_TURN_EVIL)
                                {
                                    killer.Alignment = Globals.eAlignment.Evil;
                                    killer.WriteToDisplay("Your alignment has changed to " + killer.Alignment.ToString() + ".");
                                }
                            }
                            else if (killer.Alignment == Globals.eAlignment.Evil)
                            {
                                if (killer.BaseProfession == Character.ClassType.Ravager || killer.BaseProfession == Character.ClassType.Sorcerer)
                                    killer.WriteToDisplay("You feel that Nergal is pleased by your actions.");

                                //TODO: give Ravagers mana for karma kills?
                                //TODO: benefits for evils for killing lawfuls
                                //TODO: add deities
                            }
                        }
                    }
                    else
                    {
                        // if one of the cells is not pvp enabled
                        if (!killer.CurrentCell.IsPVPEnabled || !target.CurrentCell.IsPVPEnabled)
                        {
                            // if target is not flagged by the killer
                            if (!killer.FlaggedUniqueIDs.Contains(target.UniqueID))
                            {
                                if (target.Alignment == Globals.eAlignment.Lawful ||
                                    (target.Alignment == Globals.eAlignment.Neutral && target.BaseProfession == Character.ClassType.Thief))
                                {
                                    (killer as PC).currentKarma++;
                                    (killer as PC).lifetimeKarma++;
                                    killer.WriteToDisplay("You have received 1 karma for the slaying of " + target.Name + ".");
                                    Utils.Log(killer.GetLogString() + " has received karma for killing " + target.GetLogString() + ".", Utils.LogType.Karma);
                                }
                                (killer as PC).currentMarks++; // add a mark
                                (killer as PC).PlayersKilled.Add(target.UniqueID); // add to players killed list
                                killer.WriteToDisplay("Your account has received a mark for the slaying of " + target.Name + " without provocation."); // inform player
                                Utils.Log(killer.GetLogString() + " has received a mark for an unprovoked killing " + target.GetLogString() + ".", Utils.LogType.Mark);
                                target.WriteToDisplay(killer.Name + " has received a mark for killing you unprovoked.");
                                if ((killer as PC).currentMarks >= Character.MAX_MARKS)
                                {
                                    killer.WriteToDisplay("You have exceeded the maximum amount of marks allowed. You are being disconnected from the server and will need to contact a GameMaster from the website at www.dragonsspine.com.");
                                    Utils.Log(killer.GetLogString() + " has exceeded the maximum amount of marks allowed and is being disconnected and the account suspended. Target was " + target.GetLogString() + ".", Utils.LogType.Mark);
                                    killer.RemoveFromWorld();
                                    killer.RemoveFromServer();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DoDeath(Character target, Character killer)
        {
            try
            {
                if (target == null)
                {
                    Utils.Log("Rules.DoDeath() : Target was null.", Utils.LogType.Unknown);
                    return;
                }

                if (target.IsDead) return;

                if (target.IsImage)
                {
                    Effect imageEffect = target.EffectsList[Effect.EffectTypes.Image];
                    target.SendToAllInSight("The image of " + target.EffectsList[Effect.EffectTypes.Image].Caster.Name + " dissipates.");
                    target.RemoveFromWorld();
                    return;
                }

                target.SendToAllInSight(target.GetNameForActionResult() + GameSystems.Text.TextManager.IS_SLAIN_TEXT);

                target.WriteToDisplay(GameSystems.Text.TextManager.YOU_HAVE_BEEN_SLAIN_TEXT);

                // TODO: wizard eye and peeking targets

                target.IsDead = true;

                // verify hits, stamina and mana are at 0
                if (target.IsPC)
                {
                    target.Hits = 0;
                    target.Stamina = 0;
                    target.Mana = 0;
                }

                if (target.CurrentCell == null && killer != null)
                    target.CurrentCell = killer.CurrentCell; // this fixes null cell issues when the target vanishes before leaving a corpse

                // Check if targets and/or killer are in a boxing ring and take care of karma.
                if (target.CurrentCell != null && killer != null && killer.CurrentCell != null)
                {
                    if (!target.CurrentCell.IsBoxingRing && !killer.CurrentCell.IsBoxingRing)
                    {
                        if (target.IsPC || killer.IsPC || (killer.PetOwner != null && killer.PetOwner.IsPC))
                        {
                            if (killer.PetOwner != null)
                                DoKarma(target, killer.PetOwner);
                            else DoKarma(target, killer);
                        }
                    }
                }

                if (!target.IsPC && target.QuestFlags != null && killer != null && killer.IsPC)//mlt fix for quest flag on critter death
                {
                    foreach (string tflag in target.QuestFlags)
                    {
                        if (tflag != null && tflag != " " && !killer.QuestFlags.Contains(tflag))
                        {
                            killer.WriteToDisplay("You have earned a quest flag!");
                            killer.QuestFlags.Add(tflag);
                        }
                    }
                }

                // if the target was flagged by the player, remove the flag
                if (killer != null && killer.FlaggedUniqueIDs != null)
                {
                    lock (killer.FlaggedUniqueIDs)
                    {
                        if (killer.FlaggedUniqueIDs.Remove(target.UniqueID) && target != null && target.FlaggedUniqueIDs != null)
                        {
                            lock(target.FlaggedUniqueIDs)
                                target.FlaggedUniqueIDs.Remove(killer.UniqueID);
                        }
                    }
                }

                #region Emit Death Sound
                if (!target.IsPC)
                {
                    if (target.Group != null)
                    {
                        target.Group.Remove((NPC)target);
                    }

                    #region Emit Death Sound
                    if (target.deathSound != "")
                    {
                        target.EmitSound(target.deathSound);
                    }
                    else
                    {
                        if (target.race != "")
                        {
                            if (target.gender == Globals.eGender.Female)
                            {
                                target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleGrunt));
                            }
                            else
                            {
                                target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleGrunt));
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Emit Death Sound
                    switch (target.gender)
                    {
                        case Globals.eGender.Female:
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DeathFemalePlayer));
                            break;
                        case Globals.eGender.Male:
                            target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DeathMalePlayer));
                            break;
                    }
                    #endregion

                    if (target.IsPeeking)
                    {
                        target.EffectsList[Effect.EffectTypes.Peek].StopCharacterEffect();
                        return;
                    }

                    if (target.IsWizardEye)
                    {
                        target.EffectsList[Effect.EffectTypes.Wizard_Eye].StopCharacterEffect();
                        return;
                    }
                }
                #endregion

                // break follow mode
                if (target.FollowID != 0)
                    target.BreakFollowMode();

                // checks to be made to pets before clearing the array
                if (target.Pets.Count > 0)
                {
                    List<NPC> summonedPets = new List<NPC>();

                    foreach (Character pet in new List<NPC>(target.Pets))
                    {
                        if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                            pet.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();
                        if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                            pet.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();
                        if(pet is NPC && (pet as NPC).IsSummoned)
                            summonedPets.Add(pet as NPC);
                    }

                    foreach (NPC summonedNPC in new List<NPC>(summonedPets))
                    {
                        if (summonedNPC.special.Contains("figurine"))
                            DespawnFigurine(summonedNPC);
                        else UnsummonCreature(summonedNPC);
                    }
                }

                // remove from pet owners pet list
                if (target.PetOwner != null)
                {
                    // stop the charmed animal effect -- this is redundant, as when a target is killed the effects list is cleared
                    if (target.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                        target.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();
                    else if (target.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                        target.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();

                    if(target.PetOwner != null && target.PetOwner.Pets.Contains(target as NPC))
                        target.PetOwner.Pets.Remove(target as NPC);

                    target.PetOwner = null;
                }

                // clear pets
                target.Pets.Clear();

                #region Players in the Underworld
                if (target.IsPC && target.InUnderworld)
                {
                    Rules.UnderworldDeadRest(target);
                    return;
                } 
                #endregion

                #region Figurines - Despawn Only
                if (target.special.Contains("figurine"))
                {
                    Rules.DespawnFigurine(target as NPC);
                    Utils.Log("(Figurine) " + target.GetLogString(), Utils.LogType.DeathCreature);
                    return;
                }
                #endregion

                #region Undead - Make Special Corpse
                if (target.IsUndead)
                {
                    Rules.MakeSpecialCorpse((NPC)target);
                    Utils.Log("(Undead) " + target.GetLogString(), Utils.LogType.DeathCreature);
                    return;
                }
                #endregion

                //#region Demons - Unsummon (reset) and Make Special Corpse
                //if (target.species == Globals.eSpecies.Demon)
                //{
                //    Rules.UnsummonCreature((NPC)target);
                //    Rules.MakeSpecialCorpse((NPC)target);
                //    Utils.Log("(Demon) " + target.GetLogString(), Utils.LogType.DeathCreature);
                //    return;
                //}
                //#endregion

                #region Summoned - Unsummon Only
                if ((target is NPC) && (target as NPC).IsSummoned)
                {
                    Rules.UnsummonCreature((NPC)target);
                    Utils.Log("(Summoned) " + target.GetLogString(), Utils.LogType.DeathCreature);
                    return;
                }
                #endregion

                if (!target.IsPC) // if statement to disable player dropping items on death
                {
                    if (killer != null && target != killer)
                    {
                        try
                        {
                            #region AttuneType.Slain
                            if (target.RightHand != null && target.RightHand.attuneType == Globals.eAttuneType.Slain)
                                target.RightHand.AttuneItem(killer);
                            if (target.LeftHand != null && target.LeftHand.attuneType == Globals.eAttuneType.Slain)
                                target.LeftHand.AttuneItem(killer);

                            foreach (var ring in target.GetRings())
                                if (ring != null && ring.attuneType == Globals.eAttuneType.Slain)
                                    ring.AttuneItem(killer);

                            foreach (Item wItem in target.wearing)
                            {
                                if (wItem.attuneType == Globals.eAttuneType.Slain)
                                {
                                    if ((target is NPC) && (target as NPC).tanningResult != null && (target as NPC).tanningResult.ContainsKey(wItem.itemID))
                                        wItem.AttuneItemSilently(killer);
                                    else wItem.AttuneItem(killer);
                                }
                            }

                            foreach (Item pItem in target.pouchList)
                            {
                                if (pItem.attuneType == Globals.eAttuneType.Slain)
                                {
                                    if ((target is NPC) && (target as NPC).tanningResult != null && (target as NPC).tanningResult.ContainsKey(pItem.itemID))
                                        pItem.AttuneItemSilently(killer);
                                    else pItem.AttuneItem(killer);
                                }
                            }

                            foreach (Item sItem in target.sackList)
                            {
                                if (sItem.attuneType == Globals.eAttuneType.Slain)
                                {
                                    if ((target is NPC) && (target as NPC).tanningResult != null && (target as NPC).tanningResult.ContainsKey(sItem.itemID))
                                        sItem.AttuneItemSilently(killer);
                                    else sItem.AttuneItem(killer);
                                }
                            }

                            foreach (Item bItem in target.beltList)
                            {
                                if (bItem.attuneType == Globals.eAttuneType.Slain)
                                {
                                    if ((target is NPC) && (target as NPC).tanningResult != null && (target as NPC).tanningResult.ContainsKey(bItem.itemID))
                                        bItem.AttuneItemSilently(killer);
                                    else bItem.AttuneItem(killer);
                                }
                            }
                            #endregion
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                            Utils.Log("Error in Rules.DoDeath when attuning an item upon target slain. Target: " + target.GetLogString() + " Killer: " + killer.GetLogString(), Utils.LogType.Debug);
                        }
                    }

                    #region Drop RightHand
                    try
                    {
                        if (target.RightHand != null)
                        {
                            if (target.RightHand.skillType == Globals.eSkillType.Bow && target.RightHand.name.IndexOf("crossbow") == -1)
                                    target.RightHand.IsNocked = false;
                            if (target.CurrentCell != null)
                                target.CurrentCell.Add(target.RightHand);
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.Log(target.GetLogString() + "Error on drop RightHand of DoDeath()", Utils.LogType.SystemFailure);
                        Utils.LogException(e);
                    }
                    #endregion

                    #region Drop LeftHand
                    if (target.LeftHand != null)
                    {
                        if (target.LeftHand.skillType == Globals.eSkillType.Bow && target.LeftHand.name.IndexOf("crossbow") == -1)
                                target.LeftHand.IsNocked = false;
                        if(target.CurrentCell != null)
                            target.CurrentCell.Add(target.LeftHand);
                       
                    } 
                    #endregion

                    if (target is NPC)
                    {
                        #region Log lair creature and non-lair creature deaths.
                        if ((target as NPC).lairCritter)
                        {
                            if (killer != null)
                                Utils.Log(target.GetLogString() + " was killed by " + killer.GetLogString(), Utils.LogType.DeathLair); // log this non player death if the menu item is checked
                            else
                                Utils.Log(target.GetLogString(), Utils.LogType.DeathLair);
                        }
                        else if (target is Adventurer)
                        {
                            if (killer != null)
                                Utils.Log(target.GetLogString() + " was killed by " + killer.GetLogString(), Utils.LogType.DeathAdventurer); // log this non player death if the menu item is checked
                            else
                                Utils.Log(target.GetLogString(), Utils.LogType.DeathAdventurer);
                        }
                        else if (EntityLists.UNIQUE.Contains(target.entity))
                        {
                            if (killer != null)
                                Utils.Log(target.GetLogString() + " was killed by " + killer.GetLogString(), Utils.LogType.DeathUniqueEntity); // log this non player death if the menu item is checked
                            else
                                Utils.Log(target.GetLogString(), Utils.LogType.DeathUniqueEntity);
                        }
                        else
                        {
                            if (killer != null)
                                Utils.Log(target.GetLogString() + " was killed by " + killer.GetLogString(), Utils.LogType.DeathCreature); // log this non player death if the menu item is checked
                            else
                                Utils.Log(target.GetLogString(), Utils.LogType.DeathCreature);
                        } 
                        #endregion

                        AI.DoDeath(target as NPC);
                    }

                    if (target is Adventurer)
                    {
                        Adventurer.AdventurerCount--;

                        if (Adventurer.AdventurerCountPerMap.ContainsKey(target.Map.Name))
                            Adventurer.AdventurerCountPerMap[target.Map.Name]--;

                        Utils.Log(target.GetLogString() + " was killed by " + killer != null ? killer.GetLogString() : "NULL" + ".", Utils.LogType.Adventurer);
                    }
                }
                else
                {
                    target.Deaths++;
                    target.preppedSpell = null;
                    target.IsInvisible = true;
                    target.Stunned = 0;
                    target.Hits = 0;
                    target.Poisoned = 0; // 12/21/2016 Implementation of GameEffects will not require this in the future? -Eb

                    foreach (Effect chEffects in target.EffectsList.Values)
                        chEffects.StopCharacterEffect();

                    if (killer != null)
                        Utils.Log(target.GetLogString() + " was killed by " + killer.GetLogString() + " # of Attackers: " + target.NumAttackers, Utils.LogType.DeathPlayer);
                    else
                        Utils.Log(target.GetLogString(), Utils.LogType.DeathPlayer);
                }

                Corpse corpse = Corpse.MakeCorpse(target);

                // Player killed by an eater. Dump their corpse, destroy it, then send them straight to the Underworld... (for now, muhahaha)
                if (target is PC && killer != null && killer is NPC && EntityLists.EATERS.Contains((killer as NPC).entity))
                {
                    Corpse.DumpCorpse(corpse, target.CurrentCell);

                    target.CurrentCell.Items.Remove(corpse);

                    target.WriteToDisplay("You have been eaten by " + (killer as NPC).longDesc + "!");

                    Rules.EnterUnderworld(target as PC);
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static bool EnterHell(PC chr)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["HellEnabled"].ToLower() == "false")
            {
                chr.WriteToDisplay("This feature is currently disabled.");
                return false;
            }
            return true;
        }

        public static bool EnterUnderworld(PC chr)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UnderworldEnabled"].ToLower() == "false")
            {
                chr.WriteToDisplay("This feature is currently disabled.");
                return false;
            }

            try
            {
                if (!chr.IsDead) { chr.SendShout("a thunderclap!"); }

                chr.WriteToDisplay("The world dissolves around you.");

                // Clear any warmed Spell.
                chr.preppedSpell = null;

                // Drop held Items to the ground.
                if(chr.RightHand != null){chr.CurrentCell.Add(chr.RightHand);chr.UnequipRightHand(chr.RightHand);}
                if(chr.LeftHand != null){chr.CurrentCell.Add(chr.LeftHand);chr.UnequipLeftHand(chr.LeftHand);}

                // Drop worn Items to the ground.
                if (chr.wearing.Count > 0)
                {
                    foreach(Item wItem in chr.wearing)
                        chr.CurrentCell.Add(wItem);

                    chr.wearing.Clear();
                }

                // Drop belt Items to ground.
                if(chr.beltList.Count > 0)
                {
                    foreach(Item bItem in chr.beltList)
                        chr.CurrentCell.Add(bItem);

                    chr.beltList.Clear();
                }

                // Drop sack Items to ground.
                if(chr.sackList.Count > 0)
                {
                    foreach(Item sItem in chr.sackList)
                        chr.CurrentCell.Add(sItem);

                    chr.sackList.Clear();
                }

                // Drop pouch Items to ground.
                if (chr.pouchList.Count > 0)
                {
                    foreach (Item pouchItem in chr.pouchList)
                        chr.CurrentCell.Add(pouchItem);

                    chr.pouchList.Clear();
                }

                // Drop ring Items to ground.
                if (chr.RightRing1!=null){chr.CurrentCell.Add(chr.RightRing1);chr.RightRing1=null;}
                if(chr.RightRing2!=null){chr.CurrentCell.Add(chr.RightRing2);chr.RightRing2=null;}
                if(chr.RightRing3!=null){chr.CurrentCell.Add(chr.RightRing3);chr.RightRing3=null;}
                if(chr.RightRing4!=null){chr.CurrentCell.Add(chr.RightRing4);chr.RightRing4=null;}
                if(chr.LeftRing1!=null){chr.CurrentCell.Add(chr.LeftRing1);chr.LeftRing1=null;}
                if(chr.LeftRing2!=null){chr.CurrentCell.Add(chr.LeftRing2);chr.LeftRing2=null;}
                if(chr.LeftRing3!=null){chr.CurrentCell.Add(chr.LeftRing3);chr.LeftRing3=null;}
                if(chr.LeftRing4!=null){chr.CurrentCell.Add(chr.LeftRing4);chr.LeftRing4=null;}

                foreach (Effect effect in chr.EffectsList.Values)
                    effect.StopCharacterEffect();

                // Confirm effects list is cleared.
                chr.EffectsList.Clear();

                // remove character from current location and place at Underworld entrance
                chr.CurrentCell = Cell.GetCell(chr.FacetID, Land.ID_UNDERWORLD, Map.ID_PRAETOSEBA, 25, 24, 40);
                // record current stats and set Underworld stats
                (chr as PC).UW_hitsMax = chr.HitsMax;
                (chr as PC).UW_hitsAdjustment = chr.HitsAdjustment;
                chr.HitsMax = 25;
                chr.HitsAdjustment = 0;
                chr.Hits = 25;
                (chr as PC).UW_manaMax = chr.ManaMax;
                (chr as PC).UW_manaAdjustment = chr.ManaAdjustment;
                chr.ManaMax = 0;
                chr.ManaAdjustment = 0;
                chr.Mana = 0;
                (chr as PC).UW_staminaMax = chr.StaminaMax;
                (chr as PC).UW_staminaAdjustment = chr.StaminaAdjustment;
                chr.StaminaMax = 10;
                chr.Stamina = 10;
                chr.StaminaAdjustment = 0;
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return false;
            }

            return true;
        }

        public static void ReturnFromUnderworld(Character chr)
        {
            chr.SendShout("a thunderclap!");
            chr.WriteToDisplay("The world dissolves around you.");
            //break hide on a portal
            if(chr.IsHidden){chr.IsHidden = false;}
            //clear any warmed spells
            chr.preppedSpell = null;
            //drop held items to ground
            if(chr.RightHand != null){chr.CurrentCell.Add(chr.RightHand);chr.UnequipRightHand(chr.RightHand);}
            if(chr.LeftHand != null){chr.CurrentCell.Add(chr.LeftHand);chr.UnequipLeftHand(chr.LeftHand);}
            //drop worn items to ground
            if(chr.wearing.Count > 0)
            {
                foreach(Item wItem in chr.wearing)
                {
                    chr.CurrentCell.Add(wItem);
                }
                chr.wearing.Clear();
            }
            //drop belt items to ground
            if(chr.beltList.Count > 0)
            {
                foreach(Item bItem in chr.beltList)
                {
                    chr.CurrentCell.Add(bItem);
                }
                    chr.beltList.Clear();
            }
            //drop sack contents to ground
            if(chr.sackList.Count > 0)
            {
                foreach(Item sItem in chr.sackList)
                {
                    chr.CurrentCell.Add(sItem);
                }
                chr.sackList.Clear();
            }
            //drop pouch contents to ground
            if (chr.pouchList.Count > 0)
            {
                foreach (Item pouchItem in chr.pouchList)
                {
                    chr.CurrentCell.Add(pouchItem);
                }
                chr.pouchList.Clear();
            }
            //drop rings to ground
            if (chr.RightRing1!=null){chr.CurrentCell.Add(chr.RightRing1);chr.RightRing1=null;}
            if(chr.RightRing2!=null){chr.CurrentCell.Add(chr.RightRing2);chr.RightRing2=null;}
            if(chr.RightRing3!=null){chr.CurrentCell.Add(chr.RightRing3);chr.RightRing3=null;}
            if(chr.RightRing4!=null){chr.CurrentCell.Add(chr.RightRing4);chr.RightRing4=null;}
            if(chr.LeftRing1!=null){chr.CurrentCell.Add(chr.LeftRing1);chr.LeftRing1=null;}
            if(chr.LeftRing2!=null){chr.CurrentCell.Add(chr.LeftRing2);chr.LeftRing2=null;}
            if(chr.LeftRing3!=null){chr.CurrentCell.Add(chr.LeftRing3);chr.LeftRing3=null;}
            if(chr.LeftRing4!=null){chr.CurrentCell.Add(chr.LeftRing4);chr.LeftRing4=null;}
            //remove effects
            chr.EffectsList.Clear();

            chr.HitsMax = (chr as PC).UW_hitsMax;
            chr.HitsAdjustment = (chr as PC).UW_hitsAdjustment;
            chr.Hits = chr.HitsFull;
            chr.ManaMax = (chr as PC).UW_manaMax;
            chr.ManaAdjustment = (chr as PC).UW_manaAdjustment;
            chr.Mana = chr.ManaFull;
            chr.StaminaMax = (chr as PC).UW_staminaMax;
            chr.StaminaAdjustment = (chr as PC).UW_staminaAdjustment;
            chr.Stamina = chr.StaminaMax;
            chr.Age = 10;
            chr.Constitution += 8;
            if (chr.Constitution > chr.Land.MaxAbilityScore)
                chr.Constitution = chr.Land.MaxAbilityScore;

            if ((chr as PC).currentKarma > 0 || chr.Alignment != Globals.eAlignment.Lawful)
            {
                // character is evil
                if (chr.Alignment == Globals.eAlignment.Evil)
                {
                    if (World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).EvilResX != -1)
                    {
                        chr.CurrentCell = Cell.GetCell(0, 0, 0,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).EvilResX,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).EvilResY,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).EvilResZ);
                    }
                    else
                    {
                        // default
                        chr.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                    }
                }
                else if (chr.BaseProfession == Character.ClassType.Thief) // character is a thief (likely neutral)
                {
                    if (World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ThiefResX != -1)
                    {
                        chr.CurrentCell = Cell.GetCell(0, 0, 0,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ThiefResX,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ThiefResY,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ThiefResZ);
                    }
                    else
                    {
                        // default
                        chr.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                    }
                }
                else // catch all, send to karma resurrection point which is probably the graveyard
                {
                    if (World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).KarmaResX != -1)
                    {
                        chr.CurrentCell = Cell.GetCell(0, 0, 0,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).KarmaResX,
                            World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).KarmaResY,
                             World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).KarmaResZ);
                    }
                    else
                    {
                        chr.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                        return;
                    }
                }
            }
            else // character has no karma and is lawful
            {
                if (World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ResX != -1)
                {
                    chr.CurrentCell = Cell.GetCell(0, 0, 0,
                        World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ResX,
                        World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ResY,
                         World.GetFacetByID(0).GetLandByID(0).GetMapByID(0).ResZ);
                }
                else
                {
                    chr.CurrentCell = Cell.GetCell(0, 0, 0, 23, 8, 0);
                }
            }
        }

        /// <summary>
        /// One Way Portal logic commented out on 12/10/2015 Eb -- portalling between the lands will be allowed for now
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="land"></param>
        public static void OneWayPortal(PC pc, Land land)
        {
            pc.lockerList.Clear();
            pc.bankGold = 0;
            pc.Save();
        }

        /// <summary>
        /// Returns true if the perception check is passed. Currently uses 2d20, tallies wisdom and intelligence.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static bool CheckPerception(Character chr)
        {
            if(chr.IsUndead || EntityLists.UNDEAD.Contains(chr.entity))
            {
                if (!chr.IsSpellWarmingProfession && !EntityLists.UNIQUE.Contains(chr.entity))
                    return false;
            }

            int intelligence = GetFullAbilityStat(chr, Globals.eAbilityStat.Intelligence);
            int wisdom = GetFullAbilityStat(chr, Globals.eAbilityStat.Wisdom);

            // low intelligence or wisdom means no true perception
            if (intelligence <= 5 || wisdom <= 5)
                return false;

            int totalCheck = intelligence + wisdom;

            if (chr.HasEffect(Effect.EffectTypes.Contagion))
                totalCheck -= 5;

            if (chr.HasEffect(Effect.EffectTypes.Cynosure))
                totalCheck -= 5;

            if (chr.HasEffect(Effect.EffectTypes.Drudgery))
                totalCheck -= 5;

            return RollD(2, 20) < totalCheck;
        }

        public static int CalculateKillsPerHour(PC chr)
        {
            if (chr.RoundsPlayed < 1)
                return chr.Kills;

            long kills = chr.Kills;
            long roundsPlayed = chr.RoundsPlayed;

            if (kills < 1)
                kills = 1;

            double roundsPerHour = 3600 / (DragonsSpineMain.MasterRoundInterval / 1000);

            if (chr.lastOnline < new DateTime(2019, 7, 8)) // Date of the round interval change.
                roundsPerHour = 3600 / 5; // was 5 second rounds

            if (roundsPlayed < roundsPerHour)
                roundsPlayed = (long)roundsPerHour;

            return (int)(kills / (roundsPlayed / roundsPerHour));
        }

        /// <summary>
        /// Check for spell failure on d20 roll if caster magic level is lower than spell required level.
        /// </summary>
        /// <param name="casterSkillLevel">Spell caster's magic level.</param>
        /// <param name="spellRequiredLevel">Spell required level to cast.</param>
        /// <returns>Returns true if the spell casting fails.</returns>
        public static bool CheckSpellFailure(int casterSkillLevel, int spellRequiredLevel)
        {
            if (casterSkillLevel < spellRequiredLevel && Rules.RollD(1, 20) < spellRequiredLevel)
                return true;

            return false;
        }
    }
}
