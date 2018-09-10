using System;
using Dolittle.Logging;
using Dolittle.Runtime.Events.MongoDB.Specs;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Processing.InMemory.Specs;
using Moq;

namespace Dolittle.Runtime.Events.Processing.MongoDB.Specs
{
    public class SUTProvider : IProvideTheOffsetRepository
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

        public IEventProcessorOffsetRepository Build() => new test_mongodb_offset_repository(new a_mongo_db(),_logger);
    }
}