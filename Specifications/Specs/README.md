# Dolittle.Runtime.Events.Store.Specs

This is a set of acceptance tests that should be implemented for an EventStore implementation.

This is not a .csproj.  Instead it should be included in the implementation as a submodule.

```

git submodule add https://github.com/dolittle/Runtime.Events.Store.Specs.git Specifications/Specs

```

and then added as a Compile directive.

So, for example, the Submodule could be in Specifications/Specs and the following added to the 
implementation .csproj file:

```
  <ItemGroup>
    <Compile Include="./Specs/**/*.cs" Exclude="./Specs/obj/**/*.cs;./Specs/bin/**/*.cs"/>
  </ItemGroup>
  
``` 

The implementation should implement the IProvideTheEventStore interface which has a single Build() method
that returns the implmentation of the EventStore that you wish to test.

``` 
using Dolittle.Runtime.Events.Store.Specs;

namespace Dolittle.Runtime.Events.Store.InMemory.Specs
{
    public class SUTProvider : IProvideTheEventStore
    {
        public IEventStore Build() => new InMemory.EventStore();
    }
}
``` 
