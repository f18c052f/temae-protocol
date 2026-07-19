using NUnit.Framework;
using TemaeTrainer.UI;

namespace TemaeTrainer.Tests
{
    public class GazeDwellTimerTests
    {
        [Test]
        public void Tick_NotGazing_ReturnsFalseAndZeroProgress()
        {
            var timer = new GazeDwellTimer(1f);

            var fired = timer.Tick(false, 0.5f);

            Assert.IsFalse(fired);
            Assert.AreEqual(0f, timer.Progress);
        }

        [Test]
        public void Tick_GazingBelowThreshold_AccumulatesProgressWithoutFiring()
        {
            var timer = new GazeDwellTimer(1f);

            var fired = timer.Tick(true, 0.4f);

            Assert.IsFalse(fired);
            Assert.AreEqual(0.4f, timer.Progress, 0.0001f);
        }

        [Test]
        public void Tick_GazingReachesThreshold_FiresOnceAndClampsProgress()
        {
            var timer = new GazeDwellTimer(1f);

            timer.Tick(true, 0.6f);
            var fired = timer.Tick(true, 0.6f);

            Assert.IsTrue(fired);
            Assert.AreEqual(1f, timer.Progress, 0.0001f);
        }

        [Test]
        public void Tick_ContinuingGazeAfterFire_DoesNotFireAgain()
        {
            var timer = new GazeDwellTimer(1f);
            timer.Tick(true, 1.1f);

            var fired = timer.Tick(true, 0.5f);

            Assert.IsFalse(fired);
        }

        [Test]
        public void Tick_LookAwayThenBack_FiresAgain()
        {
            var timer = new GazeDwellTimer(1f);
            timer.Tick(true, 1.1f);
            timer.Tick(false, 0f);

            timer.Tick(true, 0.9f);
            var fired = timer.Tick(true, 0.2f);

            Assert.IsTrue(fired);
        }

        [Test]
        public void Constructor_NonPositiveDwellSeconds_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GazeDwellTimer(0f));
        }
    }
}
