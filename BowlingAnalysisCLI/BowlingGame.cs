using System;

namespace BowlingAnalysisCLI
{

    public class BowlingGame
    {
        private Frame[] scorecard;   // The set of frames bowled
        private int frames;          // The number of frames bowled so far (the frame the last throw was added to)
        private int finalScore = -1; // We don't need to compute it every time if the game hasn't changed.

        public BowlingGame()
        {
            frames = 1;
            scorecard = new Frame[10];
            scorecard[0] = new Frame(1);
        }

        /**
         * Print out a readable representation of a game:
         */
        public override String ToString()
        {
            String toReturn = "";
            for (int i = 0; i < 10; i++)
            {
                if (i < frames)
                {
                    toReturn = toReturn + scorecard[i] + "  ";
                }
                else
                {
                    toReturn = toReturn + Frame.getEmptyFrameString() + "  ";
                }
            }

            if (frames == 10)
            {
                Console.WriteLine(String.Format(" => FinalScore: {0}", getFinalScore()));
            }
            return toReturn;
        }

        /**
         * Return the final score for this scorecard (if complete), or -1 if not.
         */
        public int getFinalScore()
        {
            if (frames == 10)
            {
                return computeFinalScore();
            }
            else
            {
                return -1;
            }
        }

        /**
         * Return the current frame (ie the frame the last throw was added to.
         */
        public int getCurrentFrame()
        {
            return this.frames;
        }

        /**
         * Add the results of a throw to the game.
         */
        public void addThrow(Frame.FrameValue t)
        {
            Frame newFrame = scorecard[frames-1].addThrow(t);
            if (newFrame != null)
            {
                this.frames = newFrame.getFrameNumber();
                scorecard[frames - 1] = newFrame;
            }
        }

        /**
         * Return a reference to the indicated frame. May be null if not enough throws have been added yet.
         */
        public Frame getFrame(int frameID)
        {
            if (frameID < 1 || frameID > 10)
            {
                throw new Exception(String.Format("Requested invalid frame number {0} - must be between 1 and 10.", frameID));
            }
            return scorecard[frameID - 1];
        }

        /**
         * Tally up the score for the final frame:
         */
        private int computeFinalScore()
        {
            int cumulative = 0;
            if (frames < 10)
            {
                return -1;
            }
            if (finalScore == -1)
            {
                // Starting at the first frame, add them up:
                foreach (Frame f in this.scorecard)
                {
                    if (!f.canScore())
                    {
                        throw new Exception(String.Format("Game is complete, but cannot score frame {0}", f.getFrameNumber()));
                    }
                    cumulative += f.getFrameScore();
                    f.setCumulativeScore(cumulative);
                }
            }
            return cumulative;
        }
    }

}
