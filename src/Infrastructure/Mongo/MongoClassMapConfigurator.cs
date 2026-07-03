using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Mongo;

/// <summary>
/// Registers BSON class maps for Domain entities so the Domain layer stays free of any
/// MongoDB.Driver/Bson attribute dependency. Must run once before the first Mongo operation.
/// </summary>
public static class MongoClassMapConfigurator
{
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
        {
            return;
        }

        ConventionRegistry.Register(
            "PoliticalNewsConventions",
            new ConventionPack { new CamelCaseElementNameConvention(), new IgnoreExtraElementsConvention(true) },
            _ => true);

        // Stored as the string "Rss"/"Api", not the default int32, so the origin of an article is
        // legible straight out of a Mongo query/Compass view without cross-referencing the enum.
        BsonSerializer.RegisterSerializer(typeof(ArticleSourceType), new EnumSerializer<ArticleSourceType>(BsonType.String));

        BsonClassMap.RegisterClassMap<NewsArticle>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        });

        BsonClassMap.RegisterClassMap<CrawlHistory>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        });

        BsonClassMap.RegisterClassMap<CrawlLock>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(null);
        });

        BsonClassMap.RegisterClassMap<RssRawResponse>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        });

        BsonClassMap.RegisterClassMap<FeedSource>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        });

        BsonClassMap.RegisterClassMap<FeedErrorLog>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
        });
    }
}
