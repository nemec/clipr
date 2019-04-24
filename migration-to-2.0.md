# Migrating to Version 2.0

This document lists the breaking changes between version 1.x and 2.0 for those that
wish to upgrade.

* Creating a new parser will continue to automatically validates the configuration of the object, but this
  can be suppressed by calling parser.SuppressAttributeIntegrityCheck(). Suppressing the check could speed up
  launch time, but any errors made in your attributes could surface as *unhelful* errors while parsing.
  If you suppress automatic validation, you can call parser.PerformAttributeIntegrityCheck() or
  parser.PerformAttributeIntegrityCheckOrThrow() manually as well (or skip it in your production build, but run in Debug).
* Object is now passed to the Parse method rather than the constructor.
* `ParserOptions` class has been renamed to `ParserSettings` to reduce ambiguity between it and CLI Options.
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.
* Parse method no longer returns the object, it now returns a ParseResult containing either
	a value, trigger, or list of errors. The old behavior of accessing the value or throwing an exception
	can be used with the `GetValueOrThrow()` method on the ParseResult.
* Removed many overloads to the CliParser constructor. The parameters can now be set inside the `ParserSettings` instance.
* The `MutuallyExclusiveGroupAttribute` has been removed due to significant confusion about how it works.
	In its place you must implement your own validator that makes the appropriate checks. Use the following as an example:

```csharp
class MyValidator : IParseValidator<MyOptions>
{
	public ValidationResult Validate(MyOptions obj)
	{
		if(obj.Flag && obj.MyProperty == null)
		{
			var errs = new [] { new ValidationFailure(
				nameof(MyOptions.MyProperty),
				"MyProperty must have a value if Flag is set."
			) };
			return new ValidationResult(errs);
		}
		return new ValidationResult();  // Success
	}
}

static void Main(string[] args)
{
	var parser = new CliParser<MyOptions>();
	parser.Validator = new MyValidator();

	var result = parser.Parse(args, new MyOptions());
	result.Handle(
		obj => /* Success */,
		trigger => /* Triggers, if defined */,
		errs => /* List of parsing and/or validation errors */
	);
}
```
* The fluent parser has been completed. The syntax has been simplified
	and is slightly different from the alpha version. See the README for examples.
* The `LocalizeAttribute` was moved from the namespace `clipr.Attributes` to `clipr`.
* Removed the property `clipr.Core.IParser.OptionsType` as it was not being used by any
	consumer of the public interface. Inside the `clipr.Core.ParserConfig` derived class,
	you can find the old property at `clipr.Core.ParserConfig.RootMetadata.RootOptionType`.
* Renamed `clipr.IOC.IVerbFactory` interface to `IObjectFactory` (along with
	`ParameterlessVerbFactory` -> `ParameterlessObjectFactory` and 
	`SimpleVerbFactory` -> `SimpleObjectFactory` types) so that they can be reused for
	post-parse dependency injection as well. The property `IParserSettings.VerbFactory`
	remains under the same name.