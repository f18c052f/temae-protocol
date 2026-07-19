using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TemaeTrainer.Sequence.Data;

namespace TemaeTrainer.Tests
{
    public class TemaeDataValidatorTests
    {
        private static TemaeDocument BuildValidDocument()
        {
            return new TemaeDocument
            {
                Temae = "usucha_hiradate",
                School = "urasenke",
                Calibration = new CalibrationConfig
                {
                    TatamiSize = new TatamiSizeData { Length = 1.76f, Width = 0.88f },
                    RequiredTatami = new List<string> { "temae", "approach1" },
                    Layout = new List<TatamiLayoutEntry>
                    {
                        new() { Id = "approach1", AdjacentTo = "temae", Edge = "v0", Rotated90 = false }
                    }
                },
                Sections = new List<SectionData>
                {
                    new()
                    {
                        Id = "nyushitsu",
                        Label = "入室",
                        Steps = new List<StepData>
                        {
                            new()
                            {
                                Id = "s001",
                                Text = "茶道口で一礼",
                                Footsteps = new List<FootstepData>
                                {
                                    new() { Foot = "L", Tatami = "approach1", U = 0.2f, V = 0.5f, YawDeg = 0, Order = 1 },
                                    new() { Foot = "R", Tatami = "approach1", U = 0.4f, V = 0.5f, YawDeg = 0, Order = 2 }
                                }
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void Validate_WellFormedDocument_HasNoErrors()
        {
            var issues = TemaeDataValidator.Validate(BuildValidDocument());
            Assert.IsFalse(issues.Any(i => i.Severity == ValidationSeverity.Error), string.Join("\n", issues));
        }

        [Test]
        public void Validate_FootstepReferencesUnknownTatami_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Sections[0].Steps[0].Footsteps[0].Tatami = "approach2";

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path.EndsWith(".tatami")));
        }

        [Test]
        public void Validate_DuplicateOrderInSameStep_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Sections[0].Steps[0].Footsteps[1].Order = 1;

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path.EndsWith(".order")));
        }

        [Test]
        public void Validate_UOutOfRange_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Sections[0].Steps[0].Footsteps[0].U = 1.5f;

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path.EndsWith(".u")));
        }

        [Test]
        public void Validate_DuplicateStepId_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Sections[0].Steps.Add(new StepData { Id = "s001", Text = "duplicate" });

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path.EndsWith(".id") && i.Message.Contains("duplicate step id")));
        }

        [Test]
        public void Validate_RequiredTatamiMissingTemae_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Calibration.RequiredTatami = new List<string> { "approach1" };

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path == "calibration.requiredTatami"));
        }

        [Test]
        public void Validate_LayoutWithCyclicAdjacentTo_ReportsError()
        {
            var doc = BuildValidDocument();
            doc.Calibration.RequiredTatami = new List<string> { "temae", "approach1", "approach2" };
            doc.Calibration.Layout = new List<TatamiLayoutEntry>
            {
                new() { Id = "approach1", AdjacentTo = "approach2", Edge = "v0" },
                new() { Id = "approach2", AdjacentTo = "approach1", Edge = "v0" }
            };

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Error && i.Path == "calibration.layout"));
        }

        [Test]
        public void Validate_EmptyTextAndFootsteps_ReportsWarningOnly()
        {
            var doc = BuildValidDocument();
            doc.Sections[0].Steps.Add(new StepData { Id = "s999", Text = "", Footsteps = new List<FootstepData>() });

            var issues = TemaeDataValidator.Validate(doc);

            Assert.IsFalse(issues.Any(i => i.Severity == ValidationSeverity.Error));
            Assert.IsTrue(issues.Any(i => i.Severity == ValidationSeverity.Warning));
        }
    }
}
