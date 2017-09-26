Title: Mutually Exclusive Arguments
---

Named arguments can be given a `MutuallyExclusiveGroupAttribute`. If multiple
named arguments belong to the same group and the user tries to specify more
than one, a parser error is generated. Groups can also be marked as Required.
If at least one `MutuallyExclusiveGroupAttribute` for a group is required and
the user does *not* provide one of the member arguments, an error is also
generated.