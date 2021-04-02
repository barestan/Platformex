using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orleans;
using Platformex.Application;

// ReSharper disable once CheckNamespace
namespace Platformex
{
    public static class QueryExtensions
    {
        public static string CalculateMd5Hash(string input)
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

        private static string GenerateQueryId(object query)
        {
            var json = JsonConvert.SerializeObject(query);
            return CalculateMd5Hash(json);
        }
        public static Task<TResult> QueryAsync<TResult>(this IGrainFactory platform, IQuery<TResult> query) 
        {
            var id = GenerateQueryId(query);
            var queryGarin = platform.GetGrain<IQueryHandler<TResult>>(id);
            return queryGarin.QueryAsync(query);
        }
    }
}