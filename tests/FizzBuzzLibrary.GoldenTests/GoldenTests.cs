using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using GoldenCapture;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FizzBuzzLibrary.GoldenTests
{
    /// <summary>
    /// Replays the Phase 0 capture against the new library and app and compares every case with the 2014 source's recording
    /// for this OS. The allowed differences live in Exceptions/2.0.0.json, one entry per case, each naming the plan's ruled
    /// change (E1 to E7) and the new answer. Set FIZZBUZZPLUS_WRITE_DIFFERENCES to a file path to write every differing
    /// case there for review (never in CI): the exceptions file is written by hand from that list, like code.
    /// </summary>
    [TestClass]
    public sealed class GoldenTests
    {
        private static readonly HashSet<string> RuledChanges = new(StringComparer.Ordinal) { "E1", "E2", "E3", "E4", "E5", "E6", "E7" };

        private static List<Case> _actual = new();
        private static Dictionary<string, string> _recorded = new(StringComparer.Ordinal);
        private static Dictionary<string, string> _recorded2014Runtime = new(StringComparer.Ordinal);
        private static Dictionary<string, (string Id, string Result)> _exceptions = new(StringComparer.Ordinal);

        private static string Os =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux";

        [ClassInitialize]
        public static void RunCapture(TestContext context)
        {
            _ = context;
            Cases.AppPath = Path.Combine(AppContext.BaseDirectory, "fizzbuzzplus.dll");
            var saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                _actual = Cases.Run();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = saved;
            }

            _recorded = ReadRecording("1.0.0.net10.0-" + Os + ".json");
            _recorded2014Runtime = ReadRecording("1.0.0.net48-windows.json");

            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Exceptions", "2.0.0.json")));
            _exceptions = doc.RootElement.GetProperty("cases").EnumerateArray().ToDictionary(
                c => Key(c.GetProperty("group").GetString()!, c.GetProperty("name").GetString()!),
                c => (c.GetProperty("change").GetString()!, Strip(c.GetProperty("result").GetRawText())),
                StringComparer.Ordinal);
        }

        [TestMethod]
        public void Every_recorded_case_is_answered_as_in_2014_or_as_ruled()
        {
            Assert.HasCount(_recorded.Count, _actual, "the capture ran a different number of cases than it recorded");
            var failures = new List<string>();
            var differences = new List<Case>();
            foreach (var c in _actual)
            {
                var key = Key(c.Group, c.Name);
                if (!_recorded.TryGetValue(key, out var recorded))
                {
                    failures.Add(key + ": not in the recording");
                    continue;
                }

                var actual = Strip(Json.Write(c.Result));
                if (actual != recorded)
                {
                    differences.Add(c);
                }

                if (_exceptions.TryGetValue(key, out var ruled))
                {
                    if (actual != ruled.Result)
                    {
                        failures.Add(key + " (" + ruled.Id + "): expected the ruled answer " + ruled.Result + " but got " + actual);
                    }
                    else if (actual == recorded)
                    {
                        failures.Add(key + " (" + ruled.Id + "): the ruled answer equals the recording on " + Os + "; the exception is not needed here");
                    }
                }
                else if (actual != recorded)
                {
                    failures.Add(key + ": expected " + recorded + " but got " + actual);
                }
            }

            WriteDifferences(differences);
            Assert.IsEmpty(failures, string.Join(Environment.NewLine, failures.Take(20)));
        }

        [TestMethod]
        public void Every_exception_names_a_ruled_change()
        {
            var unknown = _exceptions.Where(e => !RuledChanges.Contains(e.Value.Id)).Select(e => e.Key + " (" + e.Value.Id + ")").ToList();
            Assert.IsEmpty(unknown, string.Join(", ", unknown));
            var missing = _exceptions.Keys.Where(k => !_recorded.ContainsKey(k)).ToList();
            Assert.IsEmpty(missing, "exceptions for cases that were never recorded: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void Culture_cases_answer_what_the_2014_code_answered_on_its_own_runtime()
        {
            // E2: numbers are written with the invariant culture, which is what .NET Framework wrote in every culture.
            foreach (var c in _actual.Where(c => c.Group == "culture"))
            {
                var key = Key(c.Group, c.Name);
                using var actual = JsonDocument.Parse(Json.Write(c.Result));
                using var framework = JsonDocument.Parse(_recorded2014Runtime[key]);
                foreach (var property in actual.RootElement.EnumerateObject().Where(p => p.Name != "negativeSign"))
                {
                    Assert.AreEqual(Strip(framework.RootElement.GetProperty(property.Name).GetRawText()), Strip(property.Value.GetRawText()), key + " / " + property.Name);
                }
            }
        }

        private static Dictionary<string, string> ReadRecording(string file)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", file)));
            return doc.RootElement.GetProperty("cases").EnumerateArray().ToDictionary(
                c => Key(c.GetProperty("group").GetString()!, c.GetProperty("name").GetString()!),
                c => Strip(c.GetProperty("result").GetRawText()),
                StringComparer.Ordinal);
        }

        private static string Key(string group, string name)
        {
            return group + " | " + name;
        }

        /// <summary>Removes the whitespace between JSON tokens, keeping strings exactly as written (escapes included).</summary>
        private static string Strip(string json)
        {
            var sb = new StringBuilder(json.Length);
            var inString = false;
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                if (inString)
                {
                    sb.Append(ch);
                    if (ch == (char)92 && i + 1 < json.Length)
                    {
                        sb.Append(json[++i]);
                    }
                    else if (ch == (char)34)
                    {
                        inString = false;
                    }
                }
                else if (ch == (char)34)
                {
                    inString = true;
                    sb.Append(ch);
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private static void WriteDifferences(List<Case> differences)
        {
            var path = Environment.GetEnvironmentVariable("FIZZBUZZPLUS_WRITE_DIFFERENCES");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var list = new List<object>();
            foreach (var c in differences)
            {
                var o = new JsonObject();
                o.Add("group", c.Group);
                o.Add("name", c.Name);
                o.Add("change", _exceptions.TryGetValue(Key(c.Group, c.Name), out var ruled) ? ruled.Id : "E?");
                o.Add("result", c.Result);
                list.Add(o);
            }

            var root = new JsonObject();
            root.Add("note", "Differences from the 2014 recording on " + Os + "; review each before copying it into Exceptions/2.0.0.json.");
            root.Add("cases", list);
            File.WriteAllText(path, Json.Write(root), new UTF8Encoding(false));
        }
    }
}
