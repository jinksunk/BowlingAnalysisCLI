using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BowlingAnalysisCLI
{

    /**
     * A frame consists of a top and a bottom, and contains 
     * pre-valued scores for each. A frame also includes a 
     * scored value, assessed once possible spares and strikes
     * are resolved.
     */

    public class Frame
    {
        public enum FrameValue
        {
            Gutter,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Strike,
            Spare
        }

        // Translate enum values to characters.
        private static readonly Dictionary<FrameValue?, String> valToChar = new Dictionary<FrameValue?, String>
    {
        { FrameValue.Gutter, "0" },
        { FrameValue.One, "1" },
        { FrameValue.Two, "2" },
        { FrameValue.Three, "3" },
        { FrameValue.Four, "4" },
        { FrameValue.Five, "5" },
        { FrameValue.Six, "6" },
        { FrameValue.Seven, "7" },
        { FrameValue.Eight, "8" },
        { FrameValue.Nine, "9" },
        { FrameValue.Spare, "/" },
        { FrameValue.Strike, "X" }
    };

        int FRAMEPENDING = 301; // A flag value to signify that the frame is not yet complete.

        private FrameValue? top;
        private FrameValue? bottom;
        private FrameValue? tenthBottom;

        private int frameScore; // The total value of *this frame* (not cumulative, but it is potentially dependent on other frames.)
        private Frame nextFrame;
        private int frameNumber; // Things are a little different in the 10th frame.
        private int cumulativeScore; // Used by BowlingGame to keep track of per-frame cumulatives.

        // Initialize a random number generator to generate random throws:
        private static Random rd = new Random();

        /**
         * Returns a string representation of a frame suitable for display.
         */
        public static String getEmptyFrameString()
        {
            return "[ - | - ]";
        }

        /**
         * Return a random (uniform distribution) throw:
         */
         public static FrameValue generateRandomThrow()
        {
            return (FrameValue)rd.Next(0, 12);
        }

        /**
         * Create a new frame with the given frame number (1-10). Note: after the first frame, the 'addThrow(...)' method
         * will automatically generate addition frames as necessary and when valid.
         */
        public Frame(int frameNumber)
        {
            nextFrame = null;
            frameScore = FRAMEPENDING;
            cumulativeScore = -1;
            top = bottom = tenthBottom = null;

            if (frameNumber <= 10 && frameNumber >= 1)
            {
                this.frameNumber = frameNumber;
            }
            else
            {
                throw new Exception(String.Format("{0} is not a valid frame number (0 < n <= 10).", frameNumber));
            }
        }

        /**
         * Returns true if this frame can be scored; false if we are waiting for more throws.
         */
        public bool canScore()
        {
            bool toReturn = true;

            if ( frameNumber == 10 && (top == null 
                                      || bottom == null 
                                      || (top == FrameValue.Strike || bottom == FrameValue.Strike || bottom == FrameValue.Spare) && tenthBottom == null 
                                      )
               )
            {
                toReturn = false;
            }
            else if (frameNumber != 10 
                     && (
                          (bottom == FrameValue.Spare                             // If there is a spare and no next throw
                           &&
                            (nextFrame == null || nextFrame.top == null)
                          ) 
                       || (top == FrameValue.Strike                               // If there is a strike and no next-next throw
                           &&
                            (nextFrame == null 
                             || (nextFrame.top != FrameValue.Strike && nextFrame.bottom == null) 
                             || (
                                  nextFrame.top == FrameValue.Strike
                                   && (nextFrame.getNextFrame() == null || nextFrame.getNextFrame().top == null)
                                )
                            )
                          ) 
                       || (top == null) 
                       || (top != FrameValue.Strike && bottom == null)
                     )
                   )
            {
                toReturn = false;
            }
            return toReturn;
        }

        /**
         * Returns the next frame, if present, or 'null' if not.
         */
        public Frame getNextFrame()
        {
            return nextFrame;
        }

        /**
         * Set the next frame to 'successor'.
         */
        public void setNextFrame(Frame successor)
        {
            this.nextFrame = successor;
        }

        public int getFrameNumber()
        {
            return frameNumber;
        }

        /**
         * Set the cumulative score to 's'
         */
        public void setCumulativeScore(int s)
        {
            if (s >= 0 && s <= 300)
            {
                cumulativeScore = s;
            }
            else
            {
                throw new Exception(String.Format("Cumulative score {0} is not a valid bowling score (0<=s<=300)", s));
            }
        }

        /**
         * Return the cumulative score for this frame. Note: no validity checks are made; this should be fixed
         */
        public int getCumulativeScore()
        {
            return cumulativeScore;
        }

        /**
         * Add the result of a throw to this frame; if added, null is returned (no new frame is needed):
         * if we were already at the bottom of the frame, a new frame is created and returned with the throw added.
         */
        public Frame addThrow(FrameValue f)
        {
            Frame toReturn = null;

            // First, ensure the throw is valid; a spare cannot be at the top of the frame, or follow a 
            // strike/spare in the 10th frame.

            // Similarly, a Strike can only occur as top, or following a Strike / Spare in the tenth frame
            if (f == FrameValue.Spare)
            {
                if (top == null || (frameNumber == 10 && (top == FrameValue.Strike && bottom == null || bottom == FrameValue.Strike)))
                {
                        throw new InvalidThrowException("Cannot add a spare at the top of a frame.");
                }
            } else if (f == FrameValue.Strike)
            {
                if (
                     ( frameNumber == 10 
                       && ( top != FrameValue.Strike && top != null
                            && (bottom == null 
                                || bottom != FrameValue.Spare)
                           )
                     ) || frameNumber != 10 && top != null && top != FrameValue.Strike && bottom == null
                   )
                {
                    throw new InvalidThrowException("Cannot add a strike at the bottom half of a frame (unless 10th and following a strike / spare).");
                } 
            } else if (top != null && top != FrameValue.Strike && bottom == null && (getThrowPins(top) + getThrowPins(f) > 10))
            {
                throw new InvalidThrowException("Cannnot add a second value which causes the frame total to exceed 10.");
            }

            if (top == null)
            {
                top = f;
            }
            else if (bottom == null && (top != FrameValue.Strike || frameNumber == 10))
            {
                if (getThrowPins(top) + getThrowPins(f) == 10)
                {
                    f = FrameValue.Spare;
                }
                bottom = f;
            }
            else if (frameNumber == 10)
            {
                if (tenthBottom == null)
                {
                    tenthBottom = f;
                } else
                {
                    throw new ThrowsExceededException("Cannot add more throws to the 10th frame.");
                }
            }
            else
            {
                toReturn = new Frame(this.frameNumber + 1);
                toReturn.addThrow(f);
                setNextFrame(toReturn);
            }
            return toReturn;
        }

        /**
         * Return the score of this frame, or throw an exception if the score cannot yet be computed.
         */
        public int getFrameScore()
        {
            if (!canScore())
            {
                throw new Exception(String.Format("Attempted to get frame score for frame {0} before it could be scored.", frameNumber));
            }

            int fscore = 0;

            if (frameNumber == 10)
            {
                if (getNumThrows() == 3)
                {
                    fscore = getThrowPins(this.tenthBottom);
                }

                Console.WriteLine(String.Format("[D]: getFrameScore: {0} + {1} + {2}", getThrowPins(top), getThrowPins(bottom), fscore));
                fscore += getThrowPins(this.bottom) + getThrowPins(this.top);
            }
            else if (top == FrameValue.Strike)
            {
                // A little convoluted if the top of the next frame is also a strike:
                FrameValue? nextThrow = this.getNextFrame().getTop();
                FrameValue? nextNextThrow;

                if (nextThrow == FrameValue.Strike)
                {
                    nextNextThrow = getNextFrame().getNextFrame().getTop();
                }
                else
                {
                    nextNextThrow = getNextFrame().getBottom();
                }
                // TODO: We could handle spares more elegantly here
                if (nextNextThrow == FrameValue.Spare)
                {
                    int sumval = 10 - getThrowPins(nextThrow);
                    Console.WriteLine(String.Format("[D]: getFrameScore: {0} + {1} + {2}", getThrowPins(top), getThrowPins(nextThrow), sumval));
                    fscore = getThrowPins(top) + getThrowPins(nextThrow) + sumval;
                }
                else
                {
                    Console.WriteLine(String.Format("[D]: getFrameScore: {0} + {1} + {2}", getThrowPins(top), getThrowPins(nextThrow), getThrowPins(nextNextThrow)));
                    fscore = getThrowPins(top) + getThrowPins(nextThrow) + getThrowPins(nextNextThrow);
                }
            }
            else if (bottom == FrameValue.Spare)
            {
                FrameValue? nextThrow = this.getNextFrame().getTop();
                Console.WriteLine(String.Format("[D]: getFrameScore: {0} + {1} + {2}", getThrowPins(top), getThrowPins(bottom), getThrowPins(nextThrow)));
                fscore = getThrowPins(top) + getThrowPins(bottom) + getThrowPins(nextThrow);
            }
            else
            {
                Console.WriteLine(String.Format("[D]: getFrameScore: {0} + {1}", getThrowPins(top), getThrowPins(bottom)));
                fscore = getThrowPins(this.bottom) + getThrowPins(this.top);
            }

            return fscore;
        }

        /**
         * Returns the number of throws in this frame. May be 0 if none are added yet, 1 (if a strike), 2, or 3 (if 10th frame)
         */
        private int getNumThrows()
        {
            return 3 - (((top == null) ? 1 : 0) + ((bottom == null) ? 1 : 0) + ((tenthBottom == null) ? 1 : 0));
        }

        private int getThrowPins(FrameValue? throwValue)
        {
            if (throwValue == FrameValue.Spare)
            {
                return 10 - getThrowPins(top);
            }
            else if (throwValue == null)
            {
                throw new Exception("Cannot get frame value for 'null'");
            }
            else
            {
                return (int)throwValue;
            }
        }

        public FrameValue? getTop()
        {
            return top;
        }

        public FrameValue? getBottom()
        {
            return bottom;
        }

        public FrameValue? getTenthBottom()
        {
            return tenthBottom;
        }

        /**
         *  Print out a nice string representation of the frame, e.g.:
         *  * [6 | 9] 
         *  * [3 | /] 
         *  * [X | -]
         *  * [/ | X | 3] (10th only)
         *  
         *  TODO: Check for null values.
         */
        public override String ToString()
        {
            String toReturn;
            String suffix = " ";

            if (frameNumber == 10)
            {
                if (tenthBottom == null)
                {
                    suffix = " | - ";
                } else
                {
                    suffix = String.Format(" | {0}", valToChar[tenthBottom]);
                }
            }

            if (top == null)
            {
                toReturn = "[ - | -" + suffix + "]";
            } else if (bottom == null)
            {
                toReturn = "[" + valToChar[top] + " | -" + suffix + "]";
            } else
            {
                toReturn = "[" + valToChar[top] + " | " + valToChar[bottom] + suffix + "]";
            }

            return toReturn;
        }
    }

    [Serializable]
   public class ThrowsExceededException : Exception
    {
        public ThrowsExceededException()
        {
        }

        public ThrowsExceededException(string message) : base(message)
        {
        }

        public ThrowsExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ThrowsExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class InvalidThrowException : Exception
    {
        public InvalidThrowException()
        {
        }

        public InvalidThrowException(string message) : base(message)
        {
        }

        public InvalidThrowException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidThrowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

