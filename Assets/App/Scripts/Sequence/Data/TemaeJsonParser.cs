using System;
using Newtonsoft.Json;

namespace TemaeTrainer.Sequence.Data
{
    public static class TemaeJsonParser
    {
        public static TemaeDocument Parse(string json)
        {
            var document = JsonConvert.DeserializeObject<TemaeDocument>(json);
            if (document == null)
                throw new ArgumentException("JSON could not be parsed into a TemaeDocument.", nameof(json));
            return document;
        }
    }
}
