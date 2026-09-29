#nullable disable

// Golden capture of the 2014 FizzBuzzPlus source (package-modernize, repository variant, Phase 0).
// Usage: run ../capture.sh, which checks the frozen source against its git blob id, builds ../OriginalApp and runs
//   dotnet run -c Release -f net48 -- <output.json> <OriginalApp exe or dll>   (and the same with -f net10.0)
// Writes UTF-8 without BOM, LF line endings, ASCII only. Never edit a recording; never regenerate it from new code.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GoldenCapture
{
    public static class Program
    {
        // The compiled file, tests/Golden/Original/FizzBuzzProcessor.cs: commit d13d130's FizzBuzzLibrary/FizzBuzzProcessor.cs.
        private const string SourceBlob = "aa159b7ea8d38085210a17af15428cfc0f133a23";
        private const string SourceSha256 = "1b1fbe284c810e576625bf4f4afab94e5a131d238f1651721298ac0970bace13";

        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("usage: Capture <output.json> <OriginalApp exe or dll>");
                return 2;
            }

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Cases.AppPath = Path.GetFullPath(args[1]);
            var cases = Cases.Run();

            var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux";

            var root = new JsonObject();
            root.Add("source", "FizzBuzzLibrary/FizzBuzzProcessor.cs at commit d13d130 (2014-04-12), frozen in tests/Golden/Original");
            root.Add("sourceGitBlob", SourceBlob);
            root.Add("sourceSha256", SourceSha256);
            root.Add("app", "FizzBuzzWithOutput/Program.cs at commit d13d130, built by tests/Golden/OriginalApp for the same runtime");
            root.Add("runtime", RuntimeInformation.FrameworkDescription);
            root.Add("os", os);
            root.Add("process", Environment.Is64BitProcess ? "64-bit" : "32-bit");
            root.Add("culture", "invariant, except the culture cases");
            root.Add("captured", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            root.Add("note", "Golden outputs of the 2014 FizzBuzzPlus source, recorded by tests/Golden/Capture. Never edit; never regenerate from new code.");
            root.Add("caseCount", cases.Count);
            var list = new List<object>();
            foreach (var c in cases)
            {
                var o = new JsonObject();
                o.Add("group", c.Group);
                o.Add("name", c.Name);
                o.Add("result", c.Result);
                list.Add(o);
            }

            root.Add("cases", list);

            File.WriteAllText(args[0], Json.Write(root), new UTF8Encoding(false));
            Console.Error.WriteLine("wrote " + cases.Count + " cases to " + args[0]);
            return 0;
        }
    }
}
