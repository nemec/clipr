* Creating a new parser no longer automatically validates the configuration of the object.
  Must call parser.ValidateConfig() or parser.AssertConfig() manually (or skip it in your production build).
* Object is now passed to the Parse method rather than the constructor.
* Configuration is now accessed by method `parser.BuildConfig()` rather than the `Config` property.