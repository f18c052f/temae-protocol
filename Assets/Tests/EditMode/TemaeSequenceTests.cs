using System.Collections.Generic;
using NUnit.Framework;
using TemaeTrainer.Sequence;
using TemaeTrainer.Sequence.Data;

namespace TemaeTrainer.Tests
{
    public class TemaeSequenceTests
    {
        // sectionA: s1, s2 / sectionB: s3 -- 3 flattened steps total, crossing one section boundary.
        private static TemaeDocument BuildTwoSectionDocument()
        {
            return new TemaeDocument
            {
                Temae = "usucha_hiradate",
                School = "urasenke",
                Sections = new List<SectionData>
                {
                    new()
                    {
                        Id = "sectionA",
                        Steps = new List<StepData>
                        {
                            new() { Id = "s1" },
                            new() { Id = "s2" }
                        }
                    },
                    new()
                    {
                        Id = "sectionB",
                        Steps = new List<StepData> { new() { Id = "s3" } }
                    }
                }
            };
        }

        [Test]
        public void Constructor_FlattensStepsAcrossSections_StartsAtFirstStep()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());

            Assert.AreEqual("s1", sequence.CurrentStep.Id);
            Assert.AreEqual("sectionA", sequence.CurrentSection.Id);
            Assert.AreEqual(0, sequence.StepIndex);
            Assert.AreEqual(3, sequence.StepCount);
            Assert.IsTrue(sequence.IsAtStart);
            Assert.IsFalse(sequence.IsAtEnd);
        }

        [Test]
        public void Next_CrossesSectionBoundary()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());

            sequence.Next(); // s2, still sectionA
            sequence.Next(); // s3, sectionB

            Assert.AreEqual("s3", sequence.CurrentStep.Id);
            Assert.AreEqual("sectionB", sequence.CurrentSection.Id);
            Assert.IsTrue(sequence.IsAtEnd);
        }

        [Test]
        public void Next_AtEnd_DoesNothingAndDoesNotRaiseEvent()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());
            sequence.Next();
            sequence.Next();
            Assert.IsTrue(sequence.IsAtEnd);

            var raised = false;
            sequence.StepChanged += _ => raised = true;
            sequence.Next();

            Assert.AreEqual("s3", sequence.CurrentStep.Id);
            Assert.IsFalse(raised);
        }

        [Test]
        public void Previous_AtStart_DoesNothingAndDoesNotRaiseEvent()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());
            Assert.IsTrue(sequence.IsAtStart);

            var raised = false;
            sequence.StepChanged += _ => raised = true;
            sequence.Previous();

            Assert.AreEqual("s1", sequence.CurrentStep.Id);
            Assert.IsFalse(raised);
        }

        [Test]
        public void RestartFromBeginning_ReturnsToFirstStepAndRaisesEvent()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());
            sequence.Next();
            sequence.Next();

            StepChangedEventArgs? lastArgs = null;
            sequence.StepChanged += args => lastArgs = args;
            sequence.RestartFromBeginning();

            Assert.AreEqual("s1", sequence.CurrentStep.Id);
            Assert.IsTrue(sequence.IsAtStart);
            Assert.IsNotNull(lastArgs);
            Assert.AreEqual(0, lastArgs.Value.StepIndex);
        }

        [Test]
        public void Previous_AfterNext_ReturnsToPreviousStep()
        {
            var sequence = new TemaeSequence(BuildTwoSectionDocument());
            sequence.Next();
            sequence.Previous();

            Assert.AreEqual("s1", sequence.CurrentStep.Id);
            Assert.IsTrue(sequence.IsAtStart);
        }

        [Test]
        public void Constructor_DocumentWithNoSteps_ThrowsArgumentException()
        {
            var empty = new TemaeDocument { Sections = new List<SectionData>() };
            Assert.Throws<System.ArgumentException>(() => new TemaeSequence(empty));
        }
    }
}
