Title: Force Positional Argument Parsing
---

If, for any reason, you want the parser to stop parsing named arguments
and count the rest as positional arguments, use a `--`. This is useful
in cases where you want a positional argument that begins with a `-`
(`./prog.exe -- --sometext--`) or when a named argument would otherwise
consume your positional arguments as one of its own
(`./prog.exe --consumes-optional-value -- positional`).