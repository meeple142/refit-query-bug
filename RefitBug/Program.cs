using Refit;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RefitBug
{
    // my type that has an array of ints
    public class MyTypeWithAnIEnumieable
    {
        public bool ThisIsBool { get; set; }
        [Query(CollectionFormat.Multi)]
        public int[] ListOfInts { get; set; }
    }

    public interface IGetStuff
    {
        [Get("/things/{words}")]
        Task<string> GetThings(string words, MyTypeWithAnIEnumieable query);
    }

    // just so we can see the url before it sends it
    class CustomHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine(Uri.UnescapeDataString( request.RequestUri.ToString()));
            return await base.SendAsync(request, cancellationToken);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient(new CustomHttpClientHandler())
            {
                BaseAddress = new Uri("https://example.com")
            };

            var gitHubApi = RestService.For<IGetStuff>( client);
            var q = new MyTypeWithAnIEnumieable()
            {
                ThisIsBool = true,
                ListOfInts = new int[] { 4, 5, 6 }
            };

            try
            {
                var things = gitHubApi.GetThings("words", q).Result;
                // the expected decoded url should be
                // https://example.com/things/words?ThisIsBool=True&ListOfInts=4&ListOfInts=5&ListOfInts=6
                // but we get
                // https://example.com/things/words?ThisIsBool=True&ListOfInts=System.Int32[]
            }
            catch (AggregateException error)
            {
                // because example.com returns 404s
                Console.WriteLine(error.Message);
            }
        }
    }
}
