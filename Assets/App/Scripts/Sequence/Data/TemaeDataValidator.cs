using System.Collections.Generic;

namespace TemaeTrainer.Sequence.Data
{
    public enum ValidationSeverity
    {
        Error,
        Warning
    }

    public readonly struct ValidationIssue
    {
        public ValidationSeverity Severity { get; }
        public string Path { get; }
        public string Message { get; }

        public ValidationIssue(ValidationSeverity severity, string path, string message)
        {
            Severity = severity;
            Path = path;
            Message = message;
        }

        public override string ToString() => $"[{Severity}] {Path}: {Message}";
    }

    // Checks schema/reference integrity only. Content correctness (temae accuracy) is a
    // human-review concern per CLAUDE.md and is intentionally out of scope here.
    public static class TemaeDataValidator
    {
        private static readonly string[] ValidEdges = { "u0", "u1", "v0", "v1" };

        public static IReadOnlyList<ValidationIssue> Validate(TemaeDocument doc)
        {
            var issues = new List<ValidationIssue>();
            if (doc == null)
            {
                issues.Add(Error("$", "document is null"));
                return issues;
            }

            if (string.IsNullOrWhiteSpace(doc.Temae))
                issues.Add(Error("temae", "temae must not be empty"));
            if (string.IsNullOrWhiteSpace(doc.School))
                issues.Add(Error("school", "school must not be empty"));

            var requiredTatami = doc.Calibration?.RequiredTatami ?? new List<string>();
            if (doc.Calibration == null)
            {
                issues.Add(Error("calibration", "calibration is required"));
            }
            else
            {
                ValidateCalibration(doc.Calibration, requiredTatami, issues);
            }

            ValidateSections(doc.Sections, requiredTatami, issues);

            return issues;
        }

        private static void ValidateCalibration(CalibrationConfig calibration, List<string> requiredTatami, List<ValidationIssue> issues)
        {
            if (calibration.TatamiSize == null || calibration.TatamiSize.Length <= 0f || calibration.TatamiSize.Width <= 0f)
                issues.Add(Error("calibration.tatamiSize", "length/width must both be positive"));

            if (requiredTatami.Count == 0)
                issues.Add(Error("calibration.requiredTatami", "requiredTatami must not be empty"));
            else if (!requiredTatami.Contains("temae"))
                issues.Add(Error("calibration.requiredTatami", "requiredTatami must include \"temae\""));

            ValidateLayout(calibration.Layout ?? new List<TatamiLayoutEntry>(), requiredTatami, issues);
        }

        private static void ValidateLayout(List<TatamiLayoutEntry> layout, List<string> requiredTatami, List<ValidationIssue> issues)
        {
            for (var i = 0; i < layout.Count; i++)
            {
                var entry = layout[i];
                var path = $"calibration.layout[{i}]";

                if (string.IsNullOrWhiteSpace(entry.Id) || entry.Id == "temae")
                    issues.Add(Error($"{path}.id", "id must be non-empty and not \"temae\""));
                else if (!requiredTatami.Contains(entry.Id))
                    issues.Add(Error($"{path}.id", $"id \"{entry.Id}\" must be listed in requiredTatami"));

                if (System.Array.IndexOf(ValidEdges, entry.Edge) < 0)
                    issues.Add(Error($"{path}.edge", $"edge must be one of u0/u1/v0/v1, got \"{entry.Edge}\""));
            }

            // Detect cyclic / unresolved adjacentTo references (must form a DAG rooted at "temae").
            var defined = new HashSet<string> { "temae" };
            var pending = new List<TatamiLayoutEntry>(layout);
            var progressed = true;
            while (progressed && pending.Count > 0)
            {
                progressed = false;
                for (var i = pending.Count - 1; i >= 0; i--)
                {
                    if (!defined.Contains(pending[i].AdjacentTo)) continue;
                    defined.Add(pending[i].Id);
                    pending.RemoveAt(i);
                    progressed = true;
                }
            }
            foreach (var entry in pending)
                issues.Add(Error("calibration.layout", $"\"{entry.Id}\" has a cyclic or unresolved adjacentTo=\"{entry.AdjacentTo}\""));
        }

        private static void ValidateSections(List<SectionData> sections, List<string> requiredTatami, List<ValidationIssue> issues)
        {
            sections ??= new List<SectionData>();
            if (sections.Count == 0)
            {
                issues.Add(Error("sections", "sections must not be empty"));
                return;
            }

            var sectionIds = new HashSet<string>();
            var stepIds = new HashSet<string>();
            for (var si = 0; si < sections.Count; si++)
            {
                var section = sections[si];
                var sectionPath = $"sections[{si}]";

                if (string.IsNullOrWhiteSpace(section.Id))
                    issues.Add(Error($"{sectionPath}.id", "id must not be empty"));
                else if (!sectionIds.Add(section.Id))
                    issues.Add(Error($"{sectionPath}.id", $"duplicate section id \"{section.Id}\""));

                var steps = section.Steps ?? new List<StepData>();
                for (var sti = 0; sti < steps.Count; sti++)
                    ValidateStep(steps[sti], $"{sectionPath}.steps[{sti}]", requiredTatami, stepIds, issues);
            }
        }

        private static void ValidateStep(StepData step, string stepPath, List<string> requiredTatami, HashSet<string> stepIds, List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(step.Id))
                issues.Add(Error($"{stepPath}.id", "id must not be empty"));
            else if (!stepIds.Add(step.Id))
                issues.Add(Error($"{stepPath}.id", $"duplicate step id \"{step.Id}\""));

            var footsteps = step.Footsteps ?? new List<FootstepData>();
            if (string.IsNullOrWhiteSpace(step.Text) && footsteps.Count == 0)
                issues.Add(Warning(stepPath, "text and footsteps are both empty"));
            if (step.Tools is { Count: > 0 })
                issues.Add(Warning($"{stepPath}.tools", "tools is defined but ignored until Phase 3"));

            var seenOrders = new HashSet<int>();
            for (var fi = 0; fi < footsteps.Count; fi++)
            {
                var fs = footsteps[fi];
                var path = $"{stepPath}.footsteps[{fi}]";

                if (fs.Foot != "L" && fs.Foot != "R")
                    issues.Add(Error($"{path}.foot", $"foot must be \"L\" or \"R\", got \"{fs.Foot}\""));
                if (!requiredTatami.Contains(fs.Tatami))
                    issues.Add(Error($"{path}.tatami", $"tatami \"{fs.Tatami}\" is not listed in requiredTatami"));
                if (fs.U is < 0f or > 1f)
                    issues.Add(Error($"{path}.u", $"u must be within [0,1], got {fs.U}"));
                if (fs.V is < 0f or > 1f)
                    issues.Add(Error($"{path}.v", $"v must be within [0,1], got {fs.V}"));
                if (!seenOrders.Add(fs.Order))
                    issues.Add(Error($"{path}.order", $"duplicate order {fs.Order} within the same step"));
            }
        }

        private static ValidationIssue Error(string path, string message) => new(ValidationSeverity.Error, path, message);
        private static ValidationIssue Warning(string path, string message) => new(ValidationSeverity.Warning, path, message);
    }
}
