function upsertMaterializedView(id, expectedVersion, item) {
    const parsedItem = JSON.parse(item);
    if (!item) {
        throw new Error("Unable to parse item.");
    }
    
    var itemQuery =
        {
            'query' : 'SELECT * FROM c WHERE c.id = @id AND c.atSequence = @expectedVersion',
            'parameters' : [{ 'name': '@id', 'value': id }, { 'name': '@expectedVersion', 'value': expectedVersion }]
        };

    const success = __.queryDocuments(__.getSelfLink(), itemQuery,
        function (err, items, options) {
            if (err) {
                throw new Error("Unable to get item: " + err.message);
            }

            if (!items || !items.length) {
                throw new Error("No results from query.");
            }

            const item = items[0];

            // Concurrency check.
            if (item)
            {
                // Everything's fine, bulk insert the events.
                __.replaceDocument(
                    item._self,
                    parsedItem,
                    function(err, newDoc) {
                        if (err) throw new Error('Error: ' + err.message);
                        __.response.setBody(newDoc);
                    });
            }
            else {
                __.response.setBody(null);
            }
        });

    if (!success) {
        throw new Error('Upserting materialized item failed.');
    }
        
}