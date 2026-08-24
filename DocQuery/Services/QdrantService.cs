using Qdrant.Client;
using Qdrant.Client.Grpc;
using DocQuery.Models;

namespace DocQuery.Services;

public class QdrantService
{
    private readonly QdrantClient _client;

    private const string CollectionName = "docquery_documents";

    public QdrantService()
    {
        _client = new QdrantClient("localhost", 6334);
    }

    public async Task CreateCollectionAsync()
    {
        var collections = await _client.ListCollectionsAsync();

        if (collections.Contains(CollectionName))
        {
            return;
        }

        await _client.CreateCollectionAsync(
            CollectionName,
            new VectorParams
            {
                Size = 768,
                Distance = Distance.Cosine
            });
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
    Guid documentId)
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
            ["documentId"]= documentId.ToString()
        }
        };

        await _client.UpsertAsync(
            CollectionName,
            new[]
            {
            point
            });
    }


    public async Task<List<DocumentList>> GetAllDocumentsAsync()
    {
        var uniqueDocuments = new Dictionary<string, string>();

        PointId offset = default;

        while (true)
        {
            var result = await _client.ScrollAsync(
                collectionName: CollectionName,
                limit: 100,
                offset: offset
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
        string documentId = "")
    {
        var filter = new Filter
        {
            Must =
        {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "documentId",
                    Match = new Match
                    {
                        Keyword = documentId
                    }
                }
            }
        }
        };

        var results = await _client.SearchAsync(
            CollectionName,
            queryEmbedding,
            filter: filter,
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
}
