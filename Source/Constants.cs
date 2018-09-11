using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.MongoDB;

namespace Dolittle.Runtime.Events.MongoDB
{
    /// <summary>
    /// Generic Constants for use within the <see cref="IEventStore" />
    /// </summary>
    public class Constants 
    {
        #pragma warning disable 1591
        public const string EVENTSOURCE_ID = "eventsource_id";     
        public const string VERSION = "version"; 
        public const string MAJOR_VERSION = "major"; 
        public const string MINOR_VERSION = "minor"; 
        public const string REVISION = "revision"; 
        public const string GENERATION = "generation";
        public const string EVENT_SOURCE_ARTIFACT = "event_source_artifact";
        public const string ID = "_id";
        public const string CORRELATION_ID = "correlation_id";
        public const string ERROR = "err";
        public const string QUERY_EVENT_ARTIFACT = "events.event_artifact";
        #pragma warning restore 1591
    }

    /// <summary>
    /// Constants related to the Version of the Event
    /// </summary>
    public class VersionConstants
    {
        #pragma warning disable 1591
        public const string COMMIT = "commit";
        public const string SEQUENCE = "sequence";
        public const string SNAPSHOT = "shapshot";
        public const string EVENT_COUNT = "total_events";
        #pragma warning disable 1591
    } 


    /// <summary>
    /// Constants related to the Event Metadata
    /// </summary>
    public class EventConstants 
    {
        #pragma warning disable 1591
        public const string CAUSED_BY = "caused_by";
        public const string OCCURRED = "occurred";
        public const string EVENT = "event";
        public const string SHA = "SHA";

        public const string EVENT_ARTIFACT = "event_artifact";
        #pragma warning disable 1591
    }

    /// <summary>
    /// Constants related to the Commit
    /// </summary>
    public class CommitConstants 
    {
        #pragma warning disable 1591
        public const string COMMIT_ID = "commit_id";
        public const string TIMESTAMP = "timestamp";
        public const string VERSION = "version";
        public const string EVENTS = "events";
        public static string INSERT_COMMIT => $@"
    function insert_commit(commit) {{
        var result;
        while (true) {{
            var newer_version;
            db.{EventStoreConfig.COMMITS}.find( {{ eventsource_id: commit.eventsource_id, commit: {{ $gte: commit.commit }} }} ).sort({{commit:-1}}).limit(1).forEach(v => newer_version = v);
            if(newer_version){{
                result = {{ err: {{ {VersionConstants.COMMIT}:newer_version.commit }} }};
                break;
            }}

            var cursor = db.{EventStoreConfig.COMMITS}.find({{}}, {{ _id: 1 }} ).sort( {{ _id: -1 }} ).limit(1);
            var seq = cursor.hasNext() ? cursor.next()._id + 1 : 1;
            commit._id = NumberLong(seq);
            db.{EventStoreConfig.COMMITS}.insert(commit);
            var err = db.getLastErrorObj();
            if(err && err.code) {{
                if(err.code == 11000 && err.err.indexOf('$_id_') != -1 ){{
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

        public const int CONCURRENCY_EXCEPTION = 11000; 
        #pragma warning disable 1591
    }
}
