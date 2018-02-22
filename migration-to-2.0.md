* Creating a new parser no longer automatically validates the configuration of the object.
  Must call parser.ValidateConfig() or parser.AssertConfig() manually (or skip it in your production build).
* Object is now passed to the Parse method rather than the constructor.
* `ParserOptions` class has been renamed to `ParserSettings` to reduce ambiguity.
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.
* Parse method no longer returns the object, it now returns a ParseResult containing either
	a value, trigger, or list of errors.
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.
* Parse method no longer returns the object, it now returns a ParseResult containing either
	a value, trigger, or list of errors. The old behavior of accessing the value or throwing an exception
	can be used with the `GetValueOrThrow()` method on the ParseResult.
* Removed many overloads to the CliParser constructor. The parameters can now be set inside the `ParserSettings` instance.
* The `MutuallyExclusiveGroupAttribute` has been removed due to significant confusion about how it works.
	In its place is a validator. Use:

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