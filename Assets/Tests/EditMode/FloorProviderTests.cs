using NUnit.Framework;
using TemaeTrainer.Calibration.Floor;
using UnityEngine;

namespace TemaeTrainer.Tests
{
    public class FloorProviderTests
    {
        private class AlwaysFailFloorProvider : IFloorProvider
        {
            public bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint)
            {
                floorWorldPoint = default;
                return false;
            }

            public bool TryGetFloorY(out float floorY)
            {
                floorY = default;
                return false;
            }
        }

        private class FixedFloorProvider : IFloorProvider
        {
            private readonly float _y;
            public FixedFloorProvider(float y) => _y = y;

            public bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint)
            {
                floorWorldPoint = new Vector3(approxWorldPoint.x, _y, approxWorldPoint.z);
                return true;
            }

            public bool TryGetFloorY(out float floorY)
            {
                floorY = _y;
                return true;
            }
        }

        [Test]
        public void FallbackFlatFloorProvider_FirstPoint_DefinesFloorY()
        {
            var provider = new FallbackFlatFloorProvider();

            var success = provider.TryProjectToFloor(new Vector3(1f, 0.85f, 2f), out var projected);

            Assert.IsTrue(success);
            Assert.AreEqual(0.85f, projected.y);
        }

        [Test]
        public void FallbackFlatFloorProvider_SubsequentPoints_KeepFirstFloorY()
        {
            var provider = new FallbackFlatFloorProvider();
            provider.TryProjectToFloor(new Vector3(0f, 0.9f, 0f), out _);

            provider.TryProjectToFloor(new Vector3(3f, 1.4f, -1f), out var second);

            Assert.AreEqual(0.9f, second.y);
        }

        [Test]
        public void FallbackChainFloorProvider_PrimarySucceeds_UsesPrimaryResult()
        {
            var chain = new FallbackChainFloorProvider(new FixedFloorProvider(0.1f), new FixedFloorProvider(9f));

            chain.TryProjectToFloor(Vector3.zero, out var result);

            Assert.AreEqual(0.1f, result.y);
        }

        [Test]
        public void FallbackChainFloorProvider_PrimaryFails_DelegatesToFallback()
        {
            var chain = new FallbackChainFloorProvider(new AlwaysFailFloorProvider(), new FixedFloorProvider(0.9f));

            var success = chain.TryProjectToFloor(new Vector3(0f, 1.2f, 0f), out var result);

            Assert.IsTrue(success);
            Assert.AreEqual(0.9f, result.y);
        }

        [Test]
        public void FallbackChainFloorProvider_BothFail_ReturnsFalse()
        {
            var chain = new FallbackChainFloorProvider(new AlwaysFailFloorProvider(), new AlwaysFailFloorProvider());

            var success = chain.TryGetFloorY(out _);

            Assert.IsFalse(success);
        }
    }
}
