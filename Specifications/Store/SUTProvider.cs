using System;
using System.Linq;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store.Specs;
using Dolittle.Applications;
using Moq;
using Dolittle.Runtime.Events.MongoDB.Specs;

namespace Dolittle.Runtime.Events.Store.MongoDB.Specs
{
    public class SUTProvider : IProvideTheEventStore
    {
        ILogger _logger;

        public SUTProvider()
        {
            var logger_mock = new Mock<ILogger>();
            logger_mock.Setup(l => l.Error(Moq.It.IsAny<Exception>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>(),Moq.It.IsAny<string>()))
                .Callback<Exception,string,string,int,string>((ex,msg,fp,ln,m) => Console.WriteLine(ex.ToString()));
            logger_mock.Setup(l => l.Debug(Moq.It.IsAny<string>(),Moq.It.IsAny<string>(),Moq.It.IsAny<int>(),Moq.It.IsAny<string>()))
                .Callback<string,string,int,string>((msg,fp,ln,m) => Console.WriteLine(msg));
            _logger = logger_mock.Object;
        }

        public IEventStore Build() => new test_mongodb_event_store(new a_mongo_db_connection(),_logger);
    }
}