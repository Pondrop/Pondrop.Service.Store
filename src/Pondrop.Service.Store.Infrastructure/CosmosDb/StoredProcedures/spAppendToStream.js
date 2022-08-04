function appendToStream(streamId, expectedVersion, events) {
    const parsedEvents = JSON.parse(events);
    if (!parsedEvents || parsedEvents.length < 1) {
        throw new Error("Unable to parse events.");
    }
    
    if (!parsedEvents.every(event => event.streamId === streamId)) {
        throw new Error("Added events cannot cross streams.");
    }
    
    var versionQuery =
        {
            'query' : 'SELECT Max(e.sequenceNumber) FROM events e WHERE e.streamId = @streamId',
            'parameters' : [{ 'name': '@streamId', 'value': streamId }]
        };

    const success = __.queryDocuments(__.getSelfLink(), versionQuery,
        function (err, items, options) {
            if (err) {
                throw new Error("Unable to get stream sequence: " + err.message);
            }

            if (!items || !items.length) {
                throw new Error("No results from stream query.");
            }

            const latestSeqNum = items[0].$1;

            // Concurrency check.
            if ((!latestSeqNum && expectedVersion === 0) || (latestSeqNum === expectedVersion))
            {
                // Everything's fine, bulk insert the events.
                JSON.parse(events).forEach(event => __.createDocument(__.getSelfLink(), event));

                __.response.setBody(true);
            }
            else {
                __.response.setBody(false);
            }
        });

    if (!success) {
        throw new Error('Appending events failed.');
    }
        
}