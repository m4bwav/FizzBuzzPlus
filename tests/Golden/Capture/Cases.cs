#nullable disable

// Every case of the golden capture of the 2014 FizzBuzzPlus source (frozen in ../Original, commit d13d130). Touches only
// public names of FizzBuzzLibrary, so the golden test compiles this file unchanged against the new library and compares
// its answers with the recording. Each case records the exact text written to the TextWriter, the exception (if any)
// with the phase it came from, and whether the capture's guard had to stop an output that does not end by itself.
// Characters are built from char codes, never typed as escapes (skill L-050), so no editor can rewrite them.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FizzBuzzLibrary;

namespace GoldenCapture
{
    public sealed class Case
    {
        public string Group;
        public string Name;
        public object Result;
    }

    /// <summary>Thrown by the capture's writer to stop an output that would never end; never by the library.</summary>
    public sealed class CaptureLimitException : Exception
    {
        public CaptureLimitException(int lines)
            : base("the capture stopped the output after " + lines.ToString(CultureInfo.InvariantCulture) + " lines")
        {
            Lines = lines;
        }

        public int Lines { get; }
    }

    /// <summary>A TextWriter that keeps the text, optionally logs each call, stops endless output, and can run a hook.</summary>
    public sealed class RecordingWriter : TextWriter
    {
        private readonly StringBuilder _text = new StringBuilder();
        private readonly int _lineLimit;
        private int _lines;

        public RecordingWriter(int lineLimit)
            : base(CultureInfo.InvariantCulture)
        {
            _lineLimit = lineLimit;
        }

        public List<object> Calls { get; } = new List<object>();

        public bool LogCalls { get; set; }

        public int FailOnWriteNumber { get; set; }

        public Action<string> OnWrite { get; set; }

        public int Writes { get; private set; }

        public int Flushes { get; private set; }

        public string Text => _text.ToString();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            Record("Write(char)", value.ToString());
        }

        public override void Write(string value)
        {
            Record("Write(string)", value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Record("Write(char[])", new string(buffer, index, count));
        }

        public override void Flush()
        {
            Flushes++;
            if (LogCalls)
            {
                Calls.Add("Flush()");
            }
        }

        private void Record(string method, string value)
        {
            Writes++;
            if (FailOnWriteNumber == Writes)
            {
                throw new IOException("the capture's writer failed on purpose");
            }

            if (LogCalls)
            {
                Calls.Add(method + " " + (value == null ? "null" : Cases.Quote(value)));
            }

            if (value == null)
            {
                return;
            }

            _text.Append(value);
            OnWrite?.Invoke(value);
            foreach (var c in value)
            {
                if (c == (char)10)
                {
                    _lines++;
                }
            }

            if (_lines >= _lineLimit)
            {
                throw new CaptureLimitException(_lines);
            }
        }
    }

    public static class Cases
    {
        private const int NormalLimit = 1000;
        private static readonly string LF = new string((char)10, 1);
        private static readonly string CR = new string((char)13, 1);
        private static readonly List<Case> All = new List<Case>();

        /// <summary>The console app to run: an .exe, or a .dll started with "dotnet". Set before Run().</summary>
        public static string AppPath;

        public static List<Case> Run()
        {
            All.Clear();
            ApiCases();
            DefaultRuleCases();
            CustomRuleCases();
            EdgeRangeCases();
            RuleShapeCases();
            NullCases();
            AliasingCases();
            WriterCases();
            CultureCases();
            LargeRangeCases();
            AppCases();
            return All;
        }

        // ---------- helpers ----------

        public static string Quote(string s)
        {
            return ((char)34).ToString() + s + ((char)34).ToString();
        }

        private static void Add(string group, string name, object result)
        {
            All.Add(new Case { Group = group, Name = name, Result = result });
        }

        private static Dictionary<long, string> Rules(params object[] pairs)
        {
            var d = new Dictionary<long, string>();
            for (var i = 0; i < pairs.Length; i += 2)
            {
                d.Add(Convert.ToInt64(pairs[i], CultureInfo.InvariantCulture), (string)pairs[i + 1]);
            }

            return d;
        }

        private static Dictionary<long, string> OldTestRules()
        {
            return Rules(2, "Even", 6, "Scary", 7, "Lucky");
        }

        private static string Where(Exception e)
        {
            var type = e.TargetSite?.DeclaringType;
            var ns = type?.Namespace ?? string.Empty;
            if (ns.StartsWith("GoldenCapture", StringComparison.Ordinal))
            {
                return "caller";
            }

            if (ns.StartsWith("FizzBuzzLibrary", StringComparison.Ordinal))
            {
                return "library";
            }

            return type == null ? "unknown" : type.Assembly.GetName().Name;
        }

        private static JsonObject Describe(Exception e, string phase)
        {
            var o = new JsonObject();
            o.Add("phase", phase);
            o.Add("type", e.GetType().FullName);
            o.Add("message", e.Message);
            o.Add("param", (e as ArgumentException)?.ParamName);
            o.Add("from", Where(e));
            return o;
        }

        /// <summary>Constructs a processor on a recording writer, runs the calls, and records text, calls and errors.</summary>
        private static JsonObject Run(Func<TextWriter, FizzBuzzProcessor> make, Action<FizzBuzzProcessor> act, int limit = NormalLimit, bool logCalls = false, Action<RecordingWriter> setup = null)
        {
            var w = new RecordingWriter(limit) { LogCalls = logCalls };
            setup?.Invoke(w);
            var result = new JsonObject();
            FizzBuzzProcessor p = null;
            JsonObject error = null;
            object stopped = null;
            try
            {
                p = make(w);
            }
            catch (Exception e)
            {
                error = Describe(e, "construct");
            }

            if (error == null)
            {
                try
                {
                    act(p);
                }
                catch (CaptureLimitException e)
                {
                    stopped = e.Lines;
                }
                catch (Exception e)
                {
                    error = Describe(e, "execute");
                }
            }

            result.Add("output", w.Text);
            if (logCalls)
            {
                result.Add("calls", w.Calls);
                result.Add("flushes", w.Flushes);
            }

            result.Add("error", error);
            result.Add("stoppedByCaptureAfterLines", stopped);
            return result;
        }

        private static JsonObject Default(Action<FizzBuzzProcessor> act, int limit = NormalLimit)
        {
            return Run(w => new FizzBuzzProcessor(w), act, limit);
        }

        private static JsonObject With(Dictionary<long, string> rules, Action<FizzBuzzProcessor> act, int limit = NormalLimit)
        {
            return Run(w => new FizzBuzzProcessor(rules, w), act, limit);
        }

        private static string N(long n)
        {
            return n.ToString(CultureInfo.InvariantCulture);
        }

        // ---------- the public surface ----------

        private static string TypeName(Type t)
        {
            if (t.IsGenericType)
            {
                var name = t.GetGenericTypeDefinition().FullName;
                name = name.Substring(0, name.IndexOf('`'));
                return name + "<" + string.Join(", ", t.GetGenericArguments().Select(TypeName)) + ">";
            }

            return t.FullName;
        }

        private static string Parameters(ParameterInfo[] ps)
        {
            return "(" + string.Join(", ", ps.Select(x => TypeName(x.ParameterType) + " " + x.Name)) + ")";
        }

        private static void ApiCases()
        {
            var lines = new List<string>();
            var types = typeof(FizzBuzzProcessor).Assembly.GetTypes()
                .Where(t => t.Namespace == "FizzBuzzLibrary" && (t.IsPublic || t.IsNestedPublic || t.IsNestedFamily))
                .OrderBy(t => t.FullName, StringComparer.Ordinal);
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var t in types)
            {
                var kind = t.IsInterface ? "interface" : t.IsValueType ? "struct" : (t.IsAbstract && t.IsSealed) ? "static class" : t.IsAbstract ? "abstract class" : t.IsSealed ? "sealed class" : "class";
                lines.Add(kind + " " + t.FullName + (t.BaseType != null ? " : " + TypeName(t.BaseType) : string.Empty));
                foreach (var c in t.GetConstructors(Flags).Where(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
                {
                    lines.Add("  ctor " + t.Name + Parameters(c.GetParameters()));
                }

                foreach (var m in t.GetMethods(Flags).Where(m => (m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly) && !m.IsSpecialName))
                {
                    lines.Add("  method " + (m.IsStatic ? "static " : string.Empty) + (m.IsVirtual ? "virtual " : string.Empty) + TypeName(m.ReturnType) + " " + m.Name + Parameters(m.GetParameters()));
                }

                foreach (var pr in t.GetProperties(Flags))
                {
                    var g = pr.GetMethod;
                    var s = pr.SetMethod;
                    var gv = g != null && (g.IsPublic || g.IsFamily);
                    var sv = s != null && (s.IsPublic || s.IsFamily);
                    if (gv || sv)
                    {
                        lines.Add("  property " + TypeName(pr.PropertyType) + " " + pr.Name + (gv ? " get" : string.Empty) + (sv ? " set" : string.Empty));
                    }
                }
            }

            Add("api", "public and protected surface of namespace FizzBuzzLibrary", lines);
        }

        // ---------- behaviour ----------

        private static void DefaultRuleCases()
        {
            Add("default", "Execute()", Default(p => p.Execute()));
            foreach (var n in new long[] { 1, 2, 3, 5, 9, 10, 15, 18, 20, 30, 45, 0, -1, -3, -5, -15, long.MinValue, long.MinValue + 1, long.MaxValue - 1 })
            {
                Add("default", "Execute(" + N(n) + ")", Default(p => p.Execute(n)));
            }

            foreach (var r in new[] { new[] { 1L, 15L }, new[] { 15L, 30L }, new[] { -5L, 5L }, new[] { 5L, 5L }, new[] { 0L, 0L }, new[] { 10L, 1L }, new[] { 0L, -1L }, new[] { long.MinValue, long.MinValue + 2 } })
            {
                var a = r[0];
                var b = r[1];
                Add("default", "Execute(" + N(a) + ", " + N(b) + ")", Default(p => p.Execute(a, b)));
            }

            Add("default", "Execute(3) then Execute(5) on one instance", Default(p =>
            {
                p.Execute(3);
                p.Execute(5);
            }));
            Add("default", "Execute() twice on one instance", Default(p =>
            {
                p.Execute();
                p.Execute();
            }));
        }

        private static void CustomRuleCases()
        {
            // The rules of the 2014 tests (FizzBuzzCustomBehavior): 2 Even, 6 Scary, 7 Lucky.
            Add("custom", "Execute()", With(OldTestRules(), p => p.Execute()));
            foreach (var n in new long[] { 1, 2, 3, 6, 7, 12, 14, 21, 42, 84, 0, -42 })
            {
                Add("custom", "Execute(" + N(n) + ")", With(OldTestRules(), p => p.Execute(n)));
            }

            Add("custom", "Execute(40, 45)", With(OldTestRules(), p => p.Execute(40, 45)));
            Add("custom", "the default rules passed explicitly: Execute(1, 15)", With(Rules(3, "Fizz", 5, "Buzz"), p => p.Execute(1, 15)));
        }

        private static void EdgeRangeCases()
        {
            // Outputs that never end by themselves: the loop's counter wraps from long.MaxValue to long.MinValue.
            Add("edge", "Execute(long.MaxValue) (stopped by the capture)", Default(p => p.Execute(long.MaxValue), 3));
            Add("edge", "Execute(long.MaxValue - 2, long.MaxValue) (stopped by the capture)", Default(p => p.Execute(long.MaxValue - 2, long.MaxValue), 6));
            Add("edge", "Execute(long.MinValue, long.MaxValue) (stopped by the capture)", Default(p => p.Execute(long.MinValue, long.MaxValue), 3));
            Add("edge", "Execute(long.MaxValue - 1, long.MaxValue - 1)", Default(p => p.Execute(long.MaxValue - 1, long.MaxValue - 1)));
            Add("edge", "Execute(long.MaxValue, long.MinValue)", Default(p => p.Execute(long.MaxValue, long.MinValue)));
            Add("edge", "Execute(long.MinValue, long.MinValue)", Default(p => p.Execute(long.MinValue, long.MinValue)));
        }

        private static void RuleShapeCases()
        {
            Add("rules", "empty rules: Execute(1, 5)", With(new Dictionary<long, string>(), p => p.Execute(1, 5)));
            Add("rules", "divisor 1: Execute(1, 3)", With(Rules(1, "One"), p => p.Execute(1, 3)));
            Add("rules", "divisor 0: Execute(1, 2)", With(Rules(0, "Zero"), p => p.Execute(1, 2)));
            Add("rules", "divisor 0 and 3 (0 sorts first): Execute(3)", With(Rules(3, "Fizz", 0, "Zero"), p => p.Execute(3)));
            Add("rules", "divisor 0 with an empty range: Execute(2, 1)", With(Rules(0, "Zero"), p => p.Execute(2, 1)));
            Add("rules", "negative divisor -3 and 3: Execute(-3, 3)", With(Rules(3, "Fizz", -3, "NegFizz"), p => p.Execute(-3, 3)));
            Add("rules", "divisor -1: Execute(long.MinValue)", With(Rules(-1, "MinusOne"), p => p.Execute(long.MinValue)));
            Add("rules", "divisor -1: Execute(long.MinValue + 1)", With(Rules(-1, "MinusOne"), p => p.Execute(long.MinValue + 1)));
            Add("rules", "divisor 2 and -1: Execute(long.MinValue)", With(Rules(2, "Even", -1, "MinusOne"), p => p.Execute(long.MinValue)));
            Add("rules", "divisor long.MinValue: Execute(long.MinValue), Execute(0), Execute(long.MaxValue - 1)", With(Rules(long.MinValue, "Min"), p =>
            {
                p.Execute(long.MinValue);
                p.Execute(0);
                p.Execute(long.MaxValue - 1);
            }));
            Add("rules", "divisor long.MaxValue: Execute(-long.MaxValue), Execute(long.MaxValue - 1)", With(Rules(long.MaxValue, "Max"), p =>
            {
                p.Execute(-long.MaxValue);
                p.Execute(long.MaxValue - 1);
            }));
            Add("rules", "insertion order 5 then 3: Execute(15)", With(Rules(5, "Buzz", 3, "Fizz"), p => p.Execute(15)));
            Add("rules", "a null word: Execute(3)", With(Rules(3, null), p => p.Execute(3)));
            Add("rules", "an empty word: Execute(3)", With(Rules(3, string.Empty), p => p.Execute(3)));
            Add("rules", "the same word twice: Execute(4)", With(Rules(2, "X", 4, "X"), p => p.Execute(4)));
            Add("rules", "a word with LF and one with CR LF: Execute(6)", With(Rules(2, "A" + LF + "B", 3, "C" + CR + LF + "D"), p => p.Execute(6)));
            Add("rules", "a word with a surrogate pair and one with a lone surrogate: Execute(6)", With(Rules(2, "Fizz" + new string(new[] { (char)0xD83D, (char)0xDE00 }), 3, new string((char)0xD800, 1)), p => p.Execute(6)));
            Add("rules", "divisors 1 to 12: Execute(60)", With(Enumerable.Range(1, 12).ToDictionary(i => (long)i, i => "d" + i.ToString(CultureInfo.InvariantCulture)), p => p.Execute(60)));
            Add("rules", "SortedDictionary with a descending comparer: Execute(15)", Run(w =>
            {
                var d = new SortedDictionary<long, string>(Comparer<long>.Create((a, b) => b.CompareTo(a)));
                d.Add(3, "Fizz");
                d.Add(5, "Buzz");
                return new FizzBuzzProcessor(d, w);
            }, p => p.Execute(15)));
            Add("rules", "ReadOnlyDictionary: Execute(15)", Run(w => new FizzBuzzProcessor(new ReadOnlyDictionary<long, string>(Rules(3, "Fizz", 5, "Buzz")), w), p => p.Execute(15)));
        }

        private static void NullCases()
        {
            Add("null", "rules null: Execute(1)", Run(w => new FizzBuzzProcessor(null, w), p => p.Execute(1)));
            Add("null", "rules null: Execute(2, 1) (empty range)", Run(w => new FizzBuzzProcessor(null, w), p => p.Execute(2, 1)));
            Add("null", "rules null: Execute()", Run(w => new FizzBuzzProcessor(null, w), p => p.Execute()));
            Add("null", "writer null, default rules: construct only", Run(w => new FizzBuzzProcessor((TextWriter)null), p => { }));
            Add("null", "writer null, default rules: Execute(1)", Run(w => new FizzBuzzProcessor((TextWriter)null), p => p.Execute(1)));
            Add("null", "writer null, default rules: Execute(2, 1) (empty range)", Run(w => new FizzBuzzProcessor((TextWriter)null), p => p.Execute(2, 1)));
            Add("null", "writer null, custom rules: Execute(1)", Run(w => new FizzBuzzProcessor(OldTestRules(), null), p => p.Execute(1)));
            Add("null", "rules and writer null: construct only", Run(w => new FizzBuzzProcessor(null, null), p => { }));
            Add("null", "rules and writer null: Execute(1)", Run(w => new FizzBuzzProcessor(null, null), p => p.Execute(1)));
        }

        private static void AliasingCases()
        {
            Add("aliasing", "rule added after construction: Execute(15)", Run(w =>
            {
                var d = Rules(3, "Fizz");
                var p = new FizzBuzzProcessor(d, w);
                d.Add(5, "Buzz");
                return p;
            }, p => p.Execute(15)));

            Dictionary<long, string> shared = null;
            Add("aliasing", "rule removed between two calls: Execute(3), Execute(3)", Run(w =>
            {
                shared = Rules(3, "Fizz");
                return new FizzBuzzProcessor(shared, w);
            }, p =>
            {
                p.Execute(3);
                shared.Remove(3);
                p.Execute(3);
            }));

            Add("aliasing", "rules cleared after construction: Execute(15)", Run(w =>
            {
                var d = Rules(3, "Fizz", 5, "Buzz");
                var p = new FizzBuzzProcessor(d, w);
                d.Clear();
                return p;
            }, p => p.Execute(15)));

            Dictionary<long, string> live = null;
            Add("aliasing", "rule added by the writer after the first line: Execute(1, 4)", Run(w =>
            {
                live = new Dictionary<long, string>();
                return new FizzBuzzProcessor(live, w);
            }, p => p.Execute(1, 4), NormalLimit, false, w => w.OnWrite = s =>
            {
                if (s == LF && !live.ContainsKey(2))
                {
                    live.Add(2, "Even");
                }
            }));

            Dictionary<long, string> during = null;
            Add("aliasing", "rule added by the writer while words are written: Execute(12), Execute(4)", Run(w =>
            {
                during = Rules(3, "Fizz", 6, "Six");
                return new FizzBuzzProcessor(during, w);
            }, p =>
            {
                p.Execute(12);
                p.Execute(4);
            }, NormalLimit, false, w => w.OnWrite = s =>
            {
                if (s == "Fizz" && !during.ContainsKey(4))
                {
                    during.Add(4, "Four");
                }
            }));
        }

        private static void WriterCases()
        {
            Add("writer", "calls, default rules: Execute(15)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(15), NormalLimit, true));
            Add("writer", "calls, default rules: Execute(1, 3)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(1, 3), NormalLimit, true));
            Add("writer", "calls, empty rules: Execute(1, 2)", Run(w => new FizzBuzzProcessor(new Dictionary<long, string>(), w), p => p.Execute(1, 2), NormalLimit, true));
            Add("writer", "calls, a null word: Execute(3)", Run(w => new FizzBuzzProcessor(Rules(3, null), w), p => p.Execute(3), NormalLimit, true));
            Add("writer", "calls, empty range: Execute(2, 1)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(2, 1), NormalLimit, true));
            Add("writer", "writer NewLine set to CR LF: Execute(3)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(3), NormalLimit, false, w => w.NewLine = CR + LF));
            Add("writer", "writer fails on its first write: Execute(3)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(3), NormalLimit, false, w => w.FailOnWriteNumber = 1));
            Add("writer", "writer fails on its third write: Execute(15, 16)", Run(w => new FizzBuzzProcessor(w), p => p.Execute(15, 16), NormalLimit, false, w => w.FailOnWriteNumber = 3));

            var disposed = new JsonObject();
            var sw = new StringWriter(CultureInfo.InvariantCulture);
            sw.Dispose();
            try
            {
                new FizzBuzzProcessor(sw).Execute(1);
                disposed.Add("error", null);
            }
            catch (Exception e)
            {
                disposed.Add("error", Describe(e, "execute"));
            }

            Add("writer", "a disposed StringWriter: Execute(1)", disposed);

            var sb = new StringBuilder();
            using (var real = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                new FizzBuzzProcessor(real).Execute(14, 16);
            }

            Add("writer", "a real StringWriter: Execute(14, 16)", sb.ToString());
        }

        private static void CultureCases()
        {
            // "Current Number: " + n formats n with the current culture, so the minus sign follows it.
            foreach (var name in new[] { string.Empty, "en-US", "sv-SE", "nb-NO", "he-IL", "fa-IR", "ar-SA", "de-DE" })
            {
                var saved = Thread.CurrentThread.CurrentCulture;
                try
                {
                    var culture = CultureInfo.GetCultureInfo(name);
                    Thread.CurrentThread.CurrentCulture = culture;
                    var r = new JsonObject();
                    r.Add("negativeSign", culture.NumberFormat.NegativeSign);
                    r.Add("default Execute(-15)", Default(p => p.Execute(-15)));
                    r.Add("default Execute(-2, 2)", Default(p => p.Execute(-2, 2)));
                    r.Add("default Execute(1234567)", Default(p => p.Execute(1234567)));
                    r.Add("default Execute(long.MinValue)", Default(p => p.Execute(long.MinValue)));
                    Add("culture", name.Length == 0 ? "invariant" : name, r);
                }
                catch (CultureNotFoundException e)
                {
                    Add("culture", name, Describe(e, "culture"));
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = saved;
                }
            }
        }

        private static void LargeRangeCases()
        {
            foreach (var custom in new[] { false, true })
            {
                var w = new RecordingWriter(200001);
                var p = custom ? new FizzBuzzProcessor(OldTestRules(), w) : new FizzBuzzProcessor(w);
                p.Execute(1, 100000);
                var text = w.Text;
                var r = new JsonObject();
                r.Add("length", text.Length);
                r.Add("lines", text.Count(c => c == (char)10));
                r.Add("sha256", Sha256(text));
                r.Add("last", text.Substring(text.LastIndexOf("Current Number: ", StringComparison.Ordinal)));
                Add("large", (custom ? "custom" : "default") + " rules: Execute(1, 100000)", r);
            }
        }

        private static string Sha256(string text)
        {
            using (var h = SHA256.Create())
            {
                return string.Concat(h.ComputeHash(Encoding.UTF8.GetBytes(text)).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        // ---------- the console app ----------

        private static void AppCases()
        {
            // Only the redirected case can be recorded: with a real console the 2014 app waits for a key after printing.
            Add("app", "no arguments, standard input redirected and empty", RunApp(string.Empty));
            Add("app", "arguments 1 5, standard input redirected and empty", RunApp("1 5"));
        }

        private static JsonObject RunApp(string arguments)
        {
            var r = new JsonObject();
            if (string.IsNullOrEmpty(AppPath))
            {
                r.Add("error", "Cases.AppPath was not set");
                return r;
            }

            var dll = AppPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
            var info = new ProcessStartInfo
            {
                FileName = dll ? "dotnet" : AppPath,
                Arguments = dll ? Quote(AppPath) + (arguments.Length > 0 ? " " + arguments : string.Empty) : arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            info.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
            info.EnvironmentVariables["DOTNET_NOLOGO"] = "1";
            using (var proc = Process.Start(info))
            {
                proc.StandardInput.Close();
                var stdout = proc.StandardOutput.ReadToEndAsync();
                var stderr = proc.StandardError.ReadToEndAsync();
                if (!proc.WaitForExit(60000))
                {
                    proc.Kill();
                    r.Add("error", "timed out after 60 s");
                    return r;
                }

                proc.WaitForExit();
                var err = stderr.Result.Replace(CR, string.Empty);
                var firstLine = err.Split((char)10).FirstOrDefault(l => l.Length > 0) ?? string.Empty;
                r.Add("stdout", stdout.Result);
                r.Add("exitCode", proc.ExitCode);
                r.Add("stderrFirstLine", firstLine);
            }

            return r;
        }
    }
}
