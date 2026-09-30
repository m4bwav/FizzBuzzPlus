# Security policy

## Reporting a vulnerability

Report it privately through GitHub: open the repository's **Security** tab and choose **Report a vulnerability**. Please do not open a public issue for a security problem.

A confirmed problem is fixed in a new release, and the advisory is published once that release is out.

## Supported versions

Only the latest major version (2.x) gets security fixes.

## What this project is not

The library writes only to the `TextWriter` it is given. It reads no files, makes no network requests and uses no reflection. Its work grows with the range the caller asks for: `Execute(long.MinValue, long.MaxValue)` writes 2^64 lines, by design. A caller that takes a range from untrusted input must bound it. The program `fizzbuzzplus` behaves the same way.
