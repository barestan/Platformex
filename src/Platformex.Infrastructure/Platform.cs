using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans;
using Platformex.Application;

namespace Platformex.Infrastructure
{
    public class Platform : IPlatform
    {
        public Definitions Definitions { get; } = new Definitions();

        private IGrainFactory _grainFactory;

        public void RegisterApplicationParts<T>()
        {
            Definitions.RegisterApplicationParts(typeof(T).Assembly);
        }

        public TAggregate GetAggregate<TAggregate>(string id) where TAggregate : IAggregate => _grainFactory.GetGrain<TAggregate>(id);

        public void RegisterAggregate<TIdentity, TAggragate, TState>()
            where TIdentity : Identity<TIdentity>
            where TAggragate : class, IAggregate<TIdentity>
            where TState : AggregateState<TIdentity, TState>
        {
            var aggregateInterfaceType = typeof(TAggragate).GetInterfaces()
                .First(i => i.GetInterfaces().Any(j=> j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregate<>)));
            var info = new AggregateDefinition(typeof(TIdentity), typeof(TAggragate),
                aggregateInterfaceType, typeof(TState));

            Definitions.Register(info);
        }

        internal void SetServiceProvider(IServiceProvider provider)
        {
            _grainFactory = provider.GetService<IGrainFactory>();
        }

        private string CalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        private string GenerateQueryId(object query)
        {
            var json = JsonConvert.SerializeObject(query);
            return CalculateMd5Hash(json);
        }
        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query) 
        {
            var id = GenerateQueryId(query);
            var queryGarin = _grainFactory.GetGrain<IQueryHandler<TResult>>(id);
            return queryGarin.QueryAsync(query);
        }

    }
}