* Creating a new parser no longer automatically validates the configuration of the object.
  Must call parser.ValidateConfig() or parser.AssertConfig() manually (or skip it in your production build).
* Object is now passed to the Parse method rather than the constructor.
<<<<<<< ae379abdea1fbba8ea3a14832ac882659c41ea94
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.
* Parse method no longer returns the object, it now returns a ParseResult containing either
	a value, trigger, or list of errors.
=======
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.
>>>>>>> Separate integrity checking from constructor and pass obj in parse method.
