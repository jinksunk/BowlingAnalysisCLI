using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BowlingAnalysisCLI;

namespace BowlingAnalysisCLITests
{
    [TestClass]
    public class BowlingGameTests
    {
        [TestMethod]
        public void TestBowlingGameCreation()
        {
            // Ensure constructor initializes correctly.
            BowlingGame bg = new BowlingGame();

            Assert.AreEqual(1, bg.getCurrentFrame(), "Bowling game initilized to incorrect frame ID.");
            Assert.AreEqual(-1, bg.getFinalScore(), "Bowling game initialized incorrect final score.");

        }

        [TestMethod]
        public void TestGetFinalScore()
        {
            BowlingGame bg = new BowlingGame();

            // Populate an entire game:
            while (bg.getCurrentFrame() < 10)
            {
                addRandomValidThrow(bg);
            }

            // In the tenth frame, we have at most two more throws:
            addRandomValidThrow(bg);
            if (!bg.getFrame(10).canScore())
            {
                // One more throw is needed:
                addRandomValidThrow(bg);
            }

            // Now, compute the score:
            int finalScore = bg.getFinalScore();
            Assert.IsTrue(finalScore >= 0 && finalScore <= 300, String.Format("Invalid final score {0}", finalScore));

            // Independently compute score:
            int independentScore = 0;
            for (int i = 1; i <= 10; i++)
            {
                Frame cur = bg.getFrame(i);
                independentScore += cur.getFrameScore();
            }
            Assert.AreEqual(independentScore, bg.getFinalScore(), "Final score computed by game, and sum of frame scores are not equal.");

            Console.WriteLine(bg);
        }

        private void addRandomValidThrow(BowlingGame bg)
        {
            // Continue adding throws until we start the 10th frame.
            bool throwAdded = false;
            while (!throwAdded)
            {
                try
                {
                    bg.addThrow(Frame.generateRandomThrow());
                    throwAdded = true;
                }
                catch (InvalidThrowException e)
                {
                    Console.WriteLine(String.Format("Throw not valid for frame; re-throwing: {0}", e.Message));
                }
            }
        }

        [TestMethod]
        public void TestAddThrow()
        {
            BowlingGame bg = new BowlingGame();
            // Test addThrow(...)
            bg.addThrow(Frame.FrameValue.Strike);
            Assert.AreEqual(1, bg.getCurrentFrame(), "Incorrect frame ID for current frame after throw added.");
            bg.addThrow(Frame.FrameValue.Seven);
            Assert.AreEqual(2, bg.getCurrentFrame(), "Incorrect frame ID after second throw added.");

            // Test getFrame(...)
            Frame first = bg.getFrame(1);
            Assert.AreEqual(1, first.getFrameNumber(), "Wrong frame returned when ID 1 was requested.");
        }
    }
}
