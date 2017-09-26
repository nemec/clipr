Title: Named and Positional Arguments
Description: Understanding the different types of command line arguments.
---


# Named and Positional Arguments

There are two types of arguments that may be defined: Named and Positional
arguments.

* Named arguments are those set off by short names or long names
  (such as `-v` or `--verbose`). They are optional (in that the parser 
  will not show an error if one is not specified) unless explicitly
  marked as required and may
  be given in any order (eg. the parser does not care if you use
  `-v --input file.txt` or `--input file.txt -v`).
* Positional arguments, on the other hand, are always required and must be
  given in the correct order (indicated by the Index property when defining
  the argument). Since they have a defined order, there is no need to tag
  them with short names or long names: just put them in the argument list
  and they will be parsed automatically.
* While positional arguments are required to be given in *order*, they may
  be freely intermixed with named arguments. For example, the following are
  all valid ways to call the example above and will give the same output:

      clipr.Sample.exe -v -v out.txt 2 3
      clipr.Sample.exe out.txt -v -v 2 3
      clipr.Sample.exe --verbose out.txt 2 -v 3

* Since short arguments must be a single character, any short argument
  with an action that does not consume values (StoreConst, StoreTrue,
  StoreFalse, AppendConst, Count) may be combined together. `-vvs` is
  functionally the same as `-v -v -s`.
* Similarly, short arguments *with* values may be input without a space
  between the flag and the first value (`-fmyfilename.txt`).