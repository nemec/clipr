Title: Action Types
Description: Customizing how values are stored in an argument.
---

Storing argument values isn't enough. There are a number of actions that
can be performed when the user specifies a named argument.

* Store: Exactly what you'd expect, this action stores the value(s) given
  after the argument in a property and is the default action.
* StoreConst: Instead of letting the user choose which value is stored, a value
  stored in the Attribute's Const property will be used. Intended to avoid
  `if(opts.FlagWasSet){ opts.SomeOtherProperty = MyConstantValue; }` --
  when parsing is finished, you should be ready to roll! Additionally, as long
  as the value stored in Const is convertible to the property type (through
  TypeDescriptor magic, more on that later), the property will be filled when
  the flag is set. Since Attribute properties may only be filled with
  compile-time constants, this allows you to use the same string serialization
  tricks for const arguments that you can use to transform argument strings
  into custom type objects.
* StoreTrue: Shortcut for constructing a boolean StoreConst. Lets you
  specify command line flags that consume no values (eg. `myprog.exe -v`).
* StoreFalse: The opposite of StoreTrue. Since booleans are value types with
  a default value of false already, it's worth noting here that nullable
  boolean properties work just fine, too (`bool? MyFlag { get; set; }`), which
  can be used to detect the presence of the "false" flag.
* Append: Like Store, but if a user specifies the flag more than once the
  parsed values are appended to the existing ones rather than replacing them
  (eg. `myprog.exe -f file1.txt -f file2.txt`). The backing property must
  be some sort of IList.
* AppendConst: Combine StoreConst and Append and you've got it.
* Count: Counts the number of times the flag was specified as an argument.
  Good for specifying a "level" (like verbosity). There is no way to limit
  the number of times a user specifies the argument.