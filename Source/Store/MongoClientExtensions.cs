// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Adds ability to eval and execute a command on the mongodb server expressed as javascript.
    /// </summary>
    /// <remarks>
    /// Inspired by: https://gist.github.com/jamesikanos/b5897b1693b5c3dd1f87.
    /// </remarks>
    public static class MongoClientExtensions
    {
        /// <summary>
        /// Evaluates the specified javascript within a MongoDb database.
        /// </summary>
        /// <param name="database">MongoDb Database to execute the javascript.</param>
        /// <param name="javascript">Javascript to execute.</param>
        /// <param name="args">Optional arguments that can be provided to the javascript function to execute.</param>
        /// <returns>A <see cref="BsonValue" /> result.</returns>
        public static BsonValue Eval(this IMongoDatabase database, string javascript, IEnumerable<BsonValue> args = null)
        {
            if (!(database.Client is MongoClient client))
                throw new ArgumentException("Client is not a MongoClient");

            var op = GetEvalOperation(database, javascript, args);

            using (var sessionHandle = new CoreSessionHandle(NoCoreSession.Instance))
            {
                using (var writeBinding = new WritableServerBinding(client.Cluster, sessionHandle))
                {
                    return op.Execute(writeBinding, CancellationToken.None);
                }
            }
        }

        static EvalOperation GetEvalOperation(IMongoDatabase mongoDB, string javascript, IEnumerable<BsonValue> args)
        {
            var function = new BsonJavaScript(javascript);
            var op = new EvalOperation(mongoDB.DatabaseNamespace, function, null)
            {
                Args = args
            };
            return op;
        }
    }
}