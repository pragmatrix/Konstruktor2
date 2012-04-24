Konstruktor is a constructor dependency injector for .NET.

Konstruktor features:

- Constructor injection
- Open generic interfaces and generators
- Automatic factory generation and injection via `Func<>` and `Func<,>`, and to manage the ownership of generated objects, `Func<Owned<>>`
- Automatic lifetime management by supporting `IDisposable`
- Hierarchical lifetime scoping
- Thread-safe
- Specification of default implementations by attributes.

Konstruktor does not support:

- field / property injection (though it can be done by specifying generator methods).
- XML configuration

For more information and some documentation snippets, take a look at the [unit tests](https://github.com/pragmatrix/Konstruktor2/tree/master/Konstruktor2.Tests) or check out [my blog](http://www.replicator.org/category/project/konstruktor).

