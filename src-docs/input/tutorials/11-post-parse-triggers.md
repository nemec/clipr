Title: Post-Parse Methods
Description: Understanding how to execute code after parsing.
---

Generally, the bulk of your application code is executed once
parsing has completely finished - usually within your `Main` method.
This ensures that all of the tasks that are part of parsing have
finished and your "Options" class is fully built. For example:

```csharp
var options = CliParser.Parse<MyOptions>(args);
RunApplication(options);
```

However, using the `PostParseAttribute` you can also mark parameterless
methods that will run
automatically once parsing is completed. When PostParse methods are
nested within Verbs, they will be executed in order from innermost class
to outermost, which means that whenever a PostParse method is executed, the
configuration class *and* all its verbs will be fully initialized by that
point.

This can offer a sort of Inversion of Control - if you set up your application's
execution context inside one of these parse methods, clipr will launch your
application as soon as parsing is complete with the fully-hydrated Options
class injected as `this`.
```csharp
internal class ParserWithPostParse
{
    [PositionalArgument(0)]
    public string Arg { get; set; }

    [PostParse]
    public void SetPostParse()
    {
        // You can access all arguments here
        RunApplication(this);
    }
}
```

```csharp
CliParser.Parse<ParserWithPostParse>("argument".Split());
```