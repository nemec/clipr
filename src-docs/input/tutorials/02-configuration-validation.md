Title: Validating Configuration
Description: How to use check that you have set up your Options class correctly.
---

# Integrity Checking

One of the most important non-functional features of a
library like this is making sure that you, the developer, don't have to
test the input with various garbage to make sure you've implemented the
library correctly. In that spirit, I've done my best to sanity check the
Attributes you place on your options class when the parser is initialized
(before any arguments are passed to it). You shouldn't have to wait until
a user passes the wrong arguments in to discover that you've defined a
duplicate argument short name or that the constant values you're storing
aren't actually convertible to the property type.
