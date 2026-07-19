using System.Collections.Generic;
using Newtonsoft.Json;

namespace TemaeTrainer.Sequence.Data
{
    public class TemaeDocument
    {
        [JsonProperty("temae")] public string Temae { get; set; }
        [JsonProperty("school")] public string School { get; set; }
        [JsonProperty("calibration")] public CalibrationConfig Calibration { get; set; }
        [JsonProperty("sections")] public List<SectionData> Sections { get; set; } = new();
    }

    public class CalibrationConfig
    {
        [JsonProperty("tatamiSize")] public TatamiSizeData TatamiSize { get; set; }
        [JsonProperty("requiredTatami")] public List<string> RequiredTatami { get; set; } = new();
        [JsonProperty("layout")] public List<TatamiLayoutEntry> Layout { get; set; } = new();
    }

    public class TatamiSizeData
    {
        [JsonProperty("length")] public float Length { get; set; }
        [JsonProperty("width")] public float Width { get; set; }
    }

    public class TatamiLayoutEntry
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("adjacentTo")] public string AdjacentTo { get; set; }
        [JsonProperty("edge")] public string Edge { get; set; }
        [JsonProperty("rotated90")] public bool Rotated90 { get; set; }
    }

    public class SectionData
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("label")] public string Label { get; set; }
        [JsonProperty("steps")] public List<StepData> Steps { get; set; } = new();
    }

    public class StepData
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("note")] public string Note { get; set; }
        [JsonProperty("footsteps")] public List<FootstepData> Footsteps { get; set; } = new();
        [JsonProperty("tools")] public List<ToolData> Tools { get; set; } = new();
    }

    public class FootstepData
    {
        [JsonProperty("foot")] public string Foot { get; set; }
        [JsonProperty("tatami")] public string Tatami { get; set; }
        [JsonProperty("u")] public float U { get; set; }
        [JsonProperty("v")] public float V { get; set; }
        [JsonProperty("yawDeg")] public float YawDeg { get; set; }
        [JsonProperty("order")] public int Order { get; set; }
    }

    public class ToolData
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("state")] public string State { get; set; }
    }
}
