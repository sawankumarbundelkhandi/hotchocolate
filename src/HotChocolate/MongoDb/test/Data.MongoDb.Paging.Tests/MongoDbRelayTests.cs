using System.Threading.Tasks;
using HotChocolate.Data.MongoDb.Filters;
using HotChocolate.Execution;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Data.MongoDb.Paging;

public class MongoDbRelayTests
{
    [Fact]
    public async Task Return_BsonId()
    {
        Snapshot.FullName();

        IRequestExecutor executor = await new ServiceCollection()
            .AddTransient<OffsetPagingProvider, MongoDbOffsetPagingProvider>()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddType<FooType>()
#pragma warning disable CS8622
                .AddTypeConverter<ObjectId, string>(x => x.ToString())
#pragma warning restore CS8622
                .BuildRequestExecutorAsync();

        IExecutionResult result = await executor
            .ExecuteAsync(@"
                    {
                        foo {
                           id
                        }
                    }");

        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task Return_Node()
    {
        Snapshot.FullName();

        IRequestExecutor executor = await new ServiceCollection()
            .AddTransient<OffsetPagingProvider, MongoDbOffsetPagingProvider>()
            .AddGraphQL()
            .AddQueryType<Query>()
            .AddType<FooType>()
#pragma warning disable CS8622
                .AddTypeConverter<ObjectId, string>(x => x.ToString())
            .AddTypeConverter<string, ObjectId>(x => ObjectId.Parse(x.ToString()))
#pragma warning restore CS8622
                .AddGlobalObjectIdentification()
            .BuildRequestExecutorAsync();

        IExecutionResult result = await executor
            .ExecuteAsync(@"
                    {
                        node(id:""Rm9vCmQ2MGRmMTYyZWQwNzY2ZTE1Y2NlNmIxMGU="") {
                           id
                        }
                    }");

        result.ToJson().MatchSnapshot();
    }

    public class Query
    {
        public Foo GetFoo() => new Foo { Id = ObjectId.Parse("507f191e810c19729de860ea") };
    }

    public class FooType : ObjectType<Foo>
    {
        protected override void Configure(IObjectTypeDescriptor<Foo> descriptor)
        {
            descriptor.ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((_, objectId) => Task.FromResult<Foo?>(new Foo { Id = objectId }));
        }
    }

    public class Foo
    {
        [BsonId]
        public ObjectId Id { get; set; }
    }
}
