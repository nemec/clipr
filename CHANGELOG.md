###2017-01-02 1.6.0

* Add ability to mask password.
* Add "optional" value constraint for reference or nullable types.
* Add section in help generation for required named arguments.
* Fixed bug in .Net Core localization resource name.

###2016-08-28 1.5.1

* Rework fluent parser into separate ParserBuilder class.
  Now all "configuration" happens there and a parser is built
  from the resulting config.
* Further strengthen the separation between "config", which defines
  the argument configuration properties, and "context", which orchestrates
  the parsing state machine. Allows the parser to be (more) threadsafe --
  no guarantees that it's truly threadsafe right now.
* Add new Parser Option that allows partial (prefix) matching on long named
  arguments, so long as the name is unambiguous. For example, with an argument
  named "checkout" you could use "--check" so long as there isn't another
  conflicting argument name (such as "checkin").
* Simplified AutomatedHelpGenerator, exposing the parser's configuration
  object so that you can generate help messages on-demand.
* Ported code to .NET Core. I saw a bunch of weird bugs where the code would
  compile but fail at runtime with a `MissingMethodException`, so there may
  still be issues in there.
* Add the ability to translate help information from Resource files (#26).
* Scale width of help screen to the terminal size (#29)

###2015-04-24 1.4.7

* Fix bug in sample where negative numbers in varargs parsing
(like a List<>) would prematurely terminate the parser.

###2015-03-28 1.4.6

* Relax restrictions preventing NumArgs=0 when it's the lower bound
  (NumArgsConstraint.AtLeast). It is still disallowed for the exact
  count and the upper bound.

###2014-08-21 1.4.5

* Implement static enumeration parsing (see below)

###2014-08-15 1.4.4

* Implement required named arguments (via property)

###2014-07-30 1.4.1

* Implemented ability to assign TypeConverters on a per-property basis
  to allow overriding built-in types.
* Fix bug when serializing to Lists.
* Set Constraint to accept 1+ parameters when action is Append or AppendConst.

###2013-09-04 1.4.0

* Added Dictionary Backend

###2013-09-01 1.3.0

* Fleshed out Fluent interface.
* Reorganized imports so attributes are part of the base clipr namsepace.
* Added support for Verbs (sub-options).


###2013-06-07 1.2.0 

* Added alternative Fluent interface for defining commands
* Fixed bug where varargs parameters would consume arguments that start with -.

###2013-01-16 

* Added TryParse method
* Renamed ParseStrict to StrictParse (for consistency's sake)
* Reordered any public parse methods where T object came first 
  (also to retain consistency across the public API).

###2013-01-13

* Initial release with automated help generation.