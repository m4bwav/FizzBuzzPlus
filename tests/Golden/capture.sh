#!/usr/bin/env bash
# Golden capture of the 2014 FizzBuzzPlus source (package-modernize, repository variant, Phase 0).
# Usage: tests/Golden/capture.sh [OUTDIR]   (from anywhere; OUTDIR defaults to tests/Golden)
# Checks the frozen source against its git blob ids, builds the 2014 console app from it, then records every case on
# net48 (Windows only) and net10.0 into 1.0.0.<tfm>-<os>.json. Run twice and compare: the output must be identical.
# The recordings in tests/Golden are made once, in Phase 0, and never regenerated from new code.
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
out="${1:-$here}"
mkdir -p "$out"

check() { # file expected-blob
  local got
  got="$(git hash-object "$here/Original/$1")"
  if [ "$got" != "$2" ]; then
    echo "Original/$1 is not the 2014 file: blob $got, expected $2" >&2
    exit 1
  fi
}
check FizzBuzzProcessor.cs aa159b7ea8d38085210a17af15428cfc0f133a23
check Program.cs 3b00fc9fc68166269e77fac21178bbd015184878

case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) os=windows; tfms="net48 net10.0" ;;
  Darwin) os=macos; tfms="net10.0" ;;
  *) os=linux; tfms="net10.0" ;;
esac

dotnet build "$here/OriginalApp/OriginalApp.csproj" -c Release -nologo -v quiet
for tfm in $tfms; do
  if [ "$tfm" = net48 ]; then app="$here/OriginalApp/bin/Release/net48/FizzBuzzWithOutput.exe"; else app="$here/OriginalApp/bin/Release/$tfm/FizzBuzzWithOutput.dll"; fi
  dotnet run --project "$here/Capture/Capture.csproj" -c Release -f "$tfm" -- "$out/1.0.0.$tfm-$os.json" "$app"
done
