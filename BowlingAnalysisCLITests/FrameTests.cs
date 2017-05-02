using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BowlingAnalysisCLI;

namespace BowlingAnalysisCLITests
{
    [TestClass]
    public class FrameTests
    {
        [TestMethod]
        public void TestFrameCreation()
        {
            // Test that frames can be created, have scores added to them, and a correct frame total is computed:
            Console.WriteLine("Beginning test...");
            Frame f1 = new Frame(1);
            Frame f2 = f1.addThrow(Frame.FrameValue.One);
            Assert.IsNull(f2,"Failed: addThrow(...) produced a new Frame for first throw.");

            f2 = f1.addThrow(Frame.FrameValue.Two);
            Assert.IsNull(f2, "Failed: addThrow(...) produced a new Frame for second throw.");

            f2 = f1.addThrow(Frame.FrameValue.Three);
            Assert.IsNotNull(f2, "Failed: addThrow(...) did not produce a new Frame for third throw.");

            Assert.IsTrue(f1.canScore(), "Failed: f1.canScore() is false, but should be true.");
 
            int f1score = f1.getFrameScore();
            Assert.AreEqual(3, f1score, String.Format("Failed: f1 returned score of {0}; should have been 3", f1score));

            Assert.IsFalse(f2.canScore(),"Failed: f2.canScore() is true, but should be false.");
        }

        [TestMethod]
        public void TestAddFrameThrow()
        {
            Frame test1 = new Frame(1);
            // Check to make sure invalid throws are caught:
            test1.addThrow(Frame.FrameValue.Eight);
            try
            {
                test1.addThrow(Frame.FrameValue.Eight);
                Assert.Fail("Failed to throw exception when sum of frame > 10");
            } catch (InvalidThrowException e)
            {

            }

            try
            {
                test1.addThrow(Frame.FrameValue.Strike);
                Assert.Fail("Failed to throw exception when sum of frame > 10");
            }
            catch (InvalidThrowException e)
            {

            }

            try
            {
                Frame test2 = new Frame(1);
                test2.addThrow(Frame.FrameValue.Spare);
                Assert.Fail("Failed to throw exception when sum of frame > 10");
            }
            catch (InvalidThrowException e)
            {

            }

            Frame test10 = new Frame(10);

            try
            {
                test10.addThrow(Frame.FrameValue.Spare);
                Assert.Fail("Failed to throw exception when spare is at top of frame 10");
            }
            catch (InvalidThrowException e)
            {

            }

            try
            {
                test10.addThrow(Frame.FrameValue.Eight);
                test10.addThrow(Frame.FrameValue.Three);
                Assert.Fail("Failed to throw exception when sum of frame > 10");
            }
            catch (InvalidThrowException e)
            {

            }

            try
            {
                test10.addThrow(Frame.FrameValue.Strike);
                Assert.Fail("Failed to throw exception when strike added as second value in tenth frame.");
            }
            catch (InvalidThrowException e)
            {

            }

        }

        [TestMethod]
        public void TestSpareScoring()
        {
            Frame spare1 = new Frame(1);
            spare1.addThrow(Frame.FrameValue.Six);
            spare1.addThrow(Frame.FrameValue.Spare);
            Assert.IsFalse(spare1.canScore(), "Failed: spare1.canScore() is true, but should be false.");

            Frame spare2 = spare1.addThrow(Frame.FrameValue.Five);
            Assert.IsTrue(spare1.canScore(),"Failed: spare1.canScore() is false, but should be true.");

            Assert.AreEqual(15, spare1.getFrameScore(), String.Format("Failed: spare1.frameScore() is {0} (should be 15).", spare1.getFrameScore()));
        }

        [TestMethod]
        public void TestStrikeScoring()
        {
            Frame strike1 = new Frame(1);
            strike1.addThrow(Frame.FrameValue.Strike);
            Frame strike2 = strike1.addThrow(Frame.FrameValue.Five);

            Assert.IsFalse(strike1.canScore(),"Failed: strike1.canScore() is true, but should be false.");

            strike2.addThrow(Frame.FrameValue.Gutter);

            Assert.IsTrue(strike1.canScore(),"Failed: strike1.canScore() is false, but should be true.");

            Assert.AreEqual(strike1.getFrameScore(),15, String.Format("Failed: spare1.frameScore() is %d (should be 15).", strike1.getFrameScore()));
        }

        [TestMethod]
        public void TestTenthFrame()
        {
            Frame cur = new Frame(1);
            Frame next = cur;
            for (int i = 0; i < 12; i++)
            {
                next = cur.addThrow(Frame.FrameValue.Strike);
                if (next != null)
                {
                    Console.Write(cur.ToString() + "  ");
                    cur = next;
                }
            }
            Console.WriteLine(cur.ToString());

            Assert.IsTrue(cur.canScore(), String.Format("canScore() == false for tenth frame, but should be true."));
            int finalScore = cur.getFrameScore();
            Assert.AreEqual(30, finalScore, String.Format("Tenth frame score is {0} (should be 30).", finalScore));

            Frame scoreableTenth = new Frame(10);
            scoreableTenth.addThrow(Frame.FrameValue.Eight);
            scoreableTenth.addThrow(Frame.FrameValue.Spare);
            Assert.IsFalse(scoreableTenth.canScore(), "scoreableTenth.canScore() == true, but should be false");
            scoreableTenth.addThrow(Frame.FrameValue.Five);
            Assert.IsTrue(scoreableTenth.canScore(), "scoreableTenth.canScore() == false, but should be true");
            Assert.AreEqual(15, scoreableTenth.getFrameScore(), String.Format("Score in tenth is {0}, but should be 15", scoreableTenth.getFrameScore()));

            scoreableTenth = new Frame(10);
            scoreableTenth.addThrow(Frame.FrameValue.Seven);
            scoreableTenth.addThrow(Frame.FrameValue.Gutter);
            Assert.IsTrue(scoreableTenth.canScore(), "scoreableTenth.canScore() == false, but should be true");
            Assert.AreEqual(7, scoreableTenth.getFrameScore(), String.Format("Score in tenth is {0}, but should be 7", scoreableTenth.getFrameScore()));
        }
    }
}
