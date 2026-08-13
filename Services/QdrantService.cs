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

    public async Task InsertDocumentAsync(
    ulong id,
    float[] embedding,
    string text,
    string documentName)
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
            ["document"] = documentName
        }
        };

        await _client.UpsertAsync(
            CollectionName,
            new[]
            {
            point
            });
    }

    public async Task<List<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        ulong limit = 3)
    {
        var results = await _client.SearchAsync(
            CollectionName,
            queryEmbedding,
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

                return new SearchResult
                {
                    Text = text,
                    Document = document,
                    Score = result.Score
                };
            })
            .ToList();
    }
}