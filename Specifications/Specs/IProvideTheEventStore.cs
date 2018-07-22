using System;
using System.Linq;
using System.Reflection;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.Specs
{
    public interface IProvideTheEventStore 
    {
        IEventStore Build();
    }
}