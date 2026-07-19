using NUnit.Framework;
using TemaeTrainer.Sequence.Data;

namespace TemaeTrainer.Tests
{
    public class TemaeJsonParserTests
    {
        private const string MinimalValidJson = @"{
            ""temae"": ""usucha_hiradate"",
            ""school"": ""urasenke"",
            ""calibration"": {
                ""tatamiSize"": { ""length"": 1.76, ""width"": 0.88 },
                ""requiredTatami"": [""temae"", ""approach1""],
                ""layout"": [
                    { ""id"": ""approach1"", ""adjacentTo"": ""temae"", ""edge"": ""v0"", ""rotated90"": false }
                ]
            },
            ""sections"": [
                {
                    ""id"": ""nyushitsu"",
                    ""label"": ""入室"",
                    ""steps"": [
                        {
                            ""id"": ""s001"",
                            ""text"": ""茶道口で一礼"",
                            ""note"": """",
                            ""footsteps"": [
                                { ""foot"": ""L"", ""tatami"": ""approach1"", ""u"": 0.2, ""v"": 0.5, ""yawDeg"": 0, ""order"": 1 }
                            ],
                            ""tools"": []
                        }
                    ]
                }
            ]
        }";

        [Test]
        public void Parse_MinimalValidJson_PopulatesAllFields()
        {
            var doc = TemaeJsonParser.Parse(MinimalValidJson);

            Assert.AreEqual("usucha_hiradate", doc.Temae);
            Assert.AreEqual("urasenke", doc.School);
            Assert.AreEqual(1.76f, doc.Calibration.TatamiSize.Length);
            Assert.AreEqual(2, doc.Calibration.RequiredTatami.Count);
            Assert.AreEqual("approach1", doc.Calibration.Layout[0].Id);
            Assert.AreEqual(1, doc.Sections.Count);
            Assert.AreEqual("s001", doc.Sections[0].Steps[0].Id);
            Assert.AreEqual("L", doc.Sections[0].Steps[0].Footsteps[0].Foot);
        }

        [Test]
        public void Parse_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => TemaeJsonParser.Parse(string.Empty));
        }

        [Test]
        public void Parse_MalformedJson_ThrowsJsonException()
        {
            // Newtonsoft throws the more specific JsonReaderException; Assert.Catch (unlike
            // Assert.Throws) also accepts derived exception types.
            Assert.Catch<Newtonsoft.Json.JsonException>(() => TemaeJsonParser.Parse("{ not valid json"));
        }
    }
}
