// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.ResourceTypes.Configuration;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Represents the connection to the EventStore MongoDB database.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="configurationWrapper"><see cref="IConfigurationFor{T}"/> <see cref="EventStoreConfiguration"/>.</param>
        public Connection(IConfigurationFor<EventStoreConfiguration> configurationWrapper)
        {
            var config = configurationWrapper.Instance;
            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                var s = MongoClientSettings.FromUrl(new MongoUrl(config.Host));
                if (config.UseSSL)
                {
                    s.UseSsl = true;
                    s.SslSettings = new SslSettings
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                        CheckCertificateRevocation = false
                    };
                }

                Server = new MongoClient(s);
            }
            else
            {
                Server = new MongoClient(config.ConnectionString);
            }

            Database = Server.GetDatabase(config.Database);
        }

        /// <summary>
        /// Gets the configured <see cref="IMongoDatabase"/>.
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        /// Gets the <see cref="IMongoClient"/>.
        /// </summary>
        public IMongoClient Server { get; }
    }
}