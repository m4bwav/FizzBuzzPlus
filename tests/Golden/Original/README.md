# The 2014 source, frozen

Byte-for-byte copies of the files at commit `d13d130` (2014-04-12, the last 2014 push), taken with `git show d13d130:<path> > <file>`. This repository never published a package, so this source is the reference the golden capture compiles, in place of a published package. Never edit these files; the golden test and the capture compile them unchanged.

| File | From | Git blob (equal to `git hash-object` of the copy) | SHA-256 |
|---|---|---|---|
| FizzBuzzProcessor.cs | FizzBuzzLibrary/FizzBuzzProcessor.cs | aa159b7ea8d38085210a17af15428cfc0f133a23 | 1b1fbe284c810e576625bf4f4afab94e5a131d238f1651721298ac0970bace13 |
| Program.cs | FizzBuzzWithOutput/Program.cs | 3b00fc9fc68166269e77fac21178bbd015184878 | cfc38f0ccd5d1248e5a5637c4b637eab415140600df931ad7a76ff31d0527ca6 |
| FizzBuzzCustomBehavior.cs | FizzBuzzTest/FizzBuzzCustomBehavior.cs | b3c0c2d02531080517ec8890251006a66355e727 | 81bcdb0fc801910c6981c2878fc42f84edcd3ba8a2e86ee59d7ad908d0d8e5b6 |
| FizzBuzzDefaultBehavior.cs | FizzBuzzTest/FizzBuzzDefaultBehavior.cs | ff5a0bfbcb7a45c765f433b5f5f1398d8a21af80 | 3ffdb8b78fe8b0dd215a04f006cb9fceba370174db016093724f66c037881ad7 |

Check: `git rev-parse d13d130:FizzBuzzLibrary/FizzBuzzProcessor.cs` and `git hash-object tests/Golden/Original/FizzBuzzProcessor.cs` print the same id (the same for the other three). The files keep their UTF-8 byte order mark and LF line endings; `.gitattributes` marks this folder `-text` so no checkout rewrites them.

Related: see also [../Capture/Program.cs](../Capture/Program.cs) (the capture that compiles these files).
