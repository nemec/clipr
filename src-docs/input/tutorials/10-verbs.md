Title: Verbs
Description: Understanding how to use verbs or action commands for input.
---

Verbs are the name given to a set of initial arguments that conditionally
parse a set of configuration options based on the given verb. Think
`git add` and `git commit`. Each has a different set of options that's
used in a different way. Each verb class is a valid configuration class,
so verbs are basically a way to combine multiple sets of configurations,
conditionally, into one. Imagine that in addition to `./git`, there was
also `./git-add` and `./git-commit` each with its own configuration classes.
With clipr, you can reuse those individual classes in `./git` as verbs.

By default, the parser can automatically create an instance of each verb
type as long as it has a parameterless default constructor. For more
complex verb types, the `CliParser` constructor accepts an implementation of
the `clipr.IOC.IVerbFactory` interface and will delegate to that interface
when it needs to create a verb. Your choice of IOC Container can also be
hooked into this Interface with an adapter. Also provided is a
`clipr.IOC.SimpleVerbFactory` implementation that allows you to define
a factory for each verb type in a collection initializer. The type in
the initializer is optional and, if missing, will be inferred from the
factory's return type.

```csharp
var factory = new SimpleVerbFactory
{
	{ () => new GitAdd(".") }
	{ typeof(GitCommit), () => new GitCommit() }
}
```

```csharp
public class GitOptions
{
	[Verb("add")]
	public GitAdd Add { get; set; }

	[Verb]  // The lowercased property name is used when no name is provided
	public GitCommit Commit { get; set; }
}
```

Some notes on verbs:

  * A configuration class cannot contain both positional parameters and
    verbs (although the verb itself may define its own positional parameters).
  * Verbs may be nested arbitrarily deep, so long as you adhere to the
    above requirement (although it's not recommended that you nest too
    deeply).
  * Multiple verb attributes may be registered for the same verb. These
    will act like aliases (`svn co` vs. `svn checkout`).
  * PostParse methods 