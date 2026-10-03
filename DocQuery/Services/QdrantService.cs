using Qdrant.Client;
using Qdrant.Client.Grpc;
using DocQuery.Models;

namespace DocQuery.Services;

public class QdrantService
{
    private readonly QdrantClient _client;

    private const string CollectionName = "docquery_documents";

    public QdrantService(IConfiguration config)
    {
        var host = config["Qdrant:Host"] ?? "localhost";
        var apiKey = config["Qdrant:ApiKey"];
        var useHttps = config.GetValue<bool>("Qdrant:UseHttps");

       _client = new QdrantClient(host, 6334, https: useHttps,
       apiKey: string.IsNullOrEmpty(apiKey) ? null : apiKey);

    }

    public async Task CreateCollectionAsync()
    {
        // Step 1: Check whether the collection exists
        var collections = await _client.ListCollectionsAsync();

        if (!collections.Contains(CollectionName))
        {
            // Step 2: Create the collection if missing
            await _client.CreateCollectionAsync(
                CollectionName,
                new VectorParams
                {
                    Size = 768,
                    Distance = Distance.Cosine
                });
        }

        // Step 3: Get collection information
        var collectionInfo =
            await _client.GetCollectionInfoAsync(CollectionName);

        // Step 4: Check whether the documentId index exists
        if (!collectionInfo.PayloadSchema.ContainsKey("documentId"))
        {
            // Step 5: Create the index if missing
            await _client.CreatePayloadIndexAsync(
                CollectionName,
                "documentId",
                PayloadSchemaType.Keyword
            );
        }

        if (!collectionInfo.PayloadSchema.ContainsKey("userId"))
        {
            await _client.CreatePayloadIndexAsync(
                CollectionName,
                "userId",
                PayloadSchemaType.Keyword
            );
        }
    }

    public async Task ResetCollectionAsync()
    {
        await _client.DeleteCollectionAsync(CollectionName);

        await _client.CreateCollectionAsync(
            CollectionName,
            new VectorParams
            {
                Size = 768,
                Distance = Distance.Cosine
            });
    }

    public async Task InsertDocumentAsync(
    ulong id,
    float[] embedding,
    string text,
    string documentName,
    int pageNumber,
    Guid documentId,
    string userId)
    {
        var point = new PointStruct
        {
            Id = new PointId
            {
                Num = id
            },
            Vectors = embedding,
            Payload =
        {
            ["text"] = text,
            ["document"] = documentName,
            ["pageNumber"]= pageNumber,
            ["documentId"]= documentId.ToString(),
            ["userId"] = userId
        }
        };

        await _client.UpsertAsync(
            CollectionName,
            new[]
            {
            point
            });
    }


    public async Task<List<DocumentList>> GetAllDocumentsAsync(string userId)
    {
        var uniqueDocuments = new Dictionary<string, string>();

        PointId offset = default;

        while (true)
        {
            var result = await _client.ScrollAsync(
                collectionName: CollectionName,
                limit: 100,
                offset: offset,
                filter: BuildFilter(userId)
            );

            foreach (var point in result.Result)
            {
                if (point.Payload.TryGetValue("document", out var documentValue) &&
                    point.Payload.TryGetValue("documentId", out var documentIdValue))
                {
                    var documentName = documentValue.StringValue;
                    var documentId = documentIdValue.StringValue;

                    if (!uniqueDocuments.ContainsKey(documentId))
                    {
                        uniqueDocuments[documentId] = documentName;
                    }
                }
            }

            if (result.NextPageOffset == null)
            {
                break;
            }

            offset = result.NextPageOffset;
        }

        return uniqueDocuments
            .Select(kvp => new DocumentList
            {
                DocumentId = kvp.Key,
                DocumentName = kvp.Value
            })
            .ToList();
    }
    public async Task<List<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        ulong limit = 3,
        string documentId = "",
        string userId = "")
    {
        var results = await _client.SearchAsync(
            CollectionName,
            queryEmbedding,
            filter: BuildFilter(userId, documentId),
            limit: limit);

        return results
            .Select(result =>
            {
                var text = result.Payload.TryGetValue("text", out var textValue)
                    ? textValue.StringValue
                    : string.Empty;

                var document = result.Payload.TryGetValue("document", out var documentValue)
                    ? documentValue.StringValue
                    : string.Empty;

                var pageNumber = result.Payload.TryGetValue(
                    "pageNumber",
                    out var pageNumberValue)
                    ? pageNumberValue.IntegerValue
                    : 0;

                return new SearchResult
                {
                    Text = text,
                    Document = document,
                    Score = result.Score,
                    PageNumber = (int)pageNumber
                };
            })
            .ToList();
    }

    public async Task<bool> DocumentBelongsToUserAsync(Guid documentId, string userId)
    {
        var result = await _client.ScrollAsync(
            collectionName: CollectionName,
            filter: BuildFilter(userId, documentId.ToString()),
            limit: 1,
            offset: default);

        return result.Result.Count > 0;
    }

    public async Task DeleteDocumentAsync(Guid documentId, string userId)
    {
        await _client.DeleteAsync(
            CollectionName,
            BuildFilter(userId, documentId.ToString()));
    }

    private static Filter BuildFilter(string userId, string? documentId = null)
    {
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "userId",
                Match = new Match { Keyword = userId }
            }
        });

        if (!string.IsNullOrWhiteSpace(documentId))
        {
            filter.Must.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = "documentId",
                    Match = new Match { Keyword = documentId }
                }
            });
        }

        return filter;
    }
}
