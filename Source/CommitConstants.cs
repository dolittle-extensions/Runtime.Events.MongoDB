// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Store.MongoDB;

#pragma warning disable CA1707, SA1310, CS1591, SA1600

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Constants related to the Commit.
    /// </summary>
    public static class CommitConstants
    {
        public const string COMMIT_ID = "commit_id";
        public const string TIMESTAMP = "timestamp";
        public const string VERSION = "version";
        public const string EVENTS = "events";
        public const int CONCURRENCY_EXCEPTION = 11000;

        public static string INSERT_COMMIT => $@"
    function insert_commit(commit) {{
        var result;
        while (true) {{
            var newer_version;
            db.{EventStoreMongoDBConfiguration.COMMITS}.find( {{ {Constants.EVENTSOURCE_ID}: commit.{Constants.EVENTSOURCE_ID}, {Constants.EVENT_SOURCE_ARTIFACT}: commit.{Constants.EVENT_SOURCE_ARTIFACT}, {VersionConstants.COMMIT}: {{ $gte: commit.{VersionConstants.COMMIT} }} }} ).sort({{commit:-1}}).limit(1).forEach(v => newer_version = v);
            if(newer_version){{
                result = {{ err: {{ {VersionConstants.COMMIT}:newer_version.commit }} }};
                break;
            }}

            var cursor = db.{EventStoreMongoDBConfiguration.COMMITS}.find({{}}, {{ _id: 1 }} ).sort( {{ _id: -1 }} ).limit(1);
            var seq = cursor.hasNext() ? cursor.next()._id + 1 : 1;
            commit._id = NumberLong(seq);
            db.{EventStoreMongoDBConfiguration.COMMITS}.insert(commit);
            var err = db.getLastErrorObj();
            if(err && err.code) {{
                if(err.code == {CONCURRENCY_EXCEPTION} && err.err.indexOf('$_id_') != -1 ){{
                    continue;
                }}
                else{{
                    result = err ;
                    break;
                }}
            }}
            result = {{ _id: commit._id }};
            break;
        }}
        return result;
    }}";
    }
}
