Title: About This Project
---

This command line parser library is very much inspired by the
[argparse](http://docs.python.org/2/library/argparse.html) library
for Python. It's one of the easiest parser libraries I've used while
still offering a comprehensive set of powerful features.

I've borrowed and ported many of its features to C# taking advantage of
C#'s static typing features where it makes sense. Thanks to the powerful
nature of the Reflection and TypeDescriptor assemblies, it's quite easy
to take the arguments already sent to any Console project and fill a
class with those values, even if they contain custom Types.

## Why another command line parser?

As of the time of this writing, clipr has been in development
off and on for almost five years, so it's not exactly a newcomer
to the ecosystem. I began development due to a change in direction
of another prominent command line parsing library that I had been
using. I was new to C#, but had extentive experience with Python
and the Python library `argparse` so I decided to bring a piece of
Python over to .NET. Beyond the API design choices, I believe
clipr offers a superior feature set vs. other libraries while
maintaining a high level of usability.

## What does clipr do better?

* It offers a high amount of flexibility for consumers of your application,
  allowing them to freely intermix named parameters with positionals and
  bunch up switches to reduce typing (e.g. `-bme` vs. `-b -m -e`).
* An extensive set of actions lets you fine-tune how values are stored in
  your Options class' properties. Supports lists of values and storing
  constants *or* the parsed values.
* Built-in support for password masking to keep secrets out of your
  users' terminal history.
* Customizable Verbs that support arbitrary nesting and non-standard
  constructors via the (optional) use of Factories.
* Comprehensive automatic Help and Version information with localization
  support.
* Support for a form of the [type safe enum](https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/)
  pattern in addition to regular enums for "constrained values".
* Flexible `TypeConverter` support that allows you to provide custom parsing
  for any type, even built-ins, with minimal fuss. You can even parse
  two arguments of the same type with different converters of you wanted to.