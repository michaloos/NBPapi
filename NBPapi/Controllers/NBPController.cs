using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using NBPapi.Models;

namespace NBPapi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NBPapiController : ControllerBase
    {
        private static string urlA = "http://api.nbp.pl/api/exchangerates/tables/a";
        private static string urlB = "http://api.nbp.pl/api/exchangerates/tables/b";

        private readonly IMemoryCache? _memoryCache;
        public NBPapiController(IMemoryCache memory) =>
            _memoryCache = memory;
        /// <summary>
        /// Służy do wgrania danych do pamięci cache jeżeli takie dane w danej chwili nie istnieją
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheObj"></param>
        private void LoadToCacheRates(string key, List<Rates> cacheObj)
        {
            if (_memoryCache.TryGetValue(key, out List<Rates> CacheList) == false)
            {
                CacheList = cacheObj;
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _memoryCache.Set(key, CacheList, cacheEntryOptions);
            }
        }

        /// <summary>
        /// Służy do zgrania danych z pamięci cache, jeżeli takie dane w danej chwili nie istnieją 
        /// pobierane są z API NBP na nowo
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<Rates> LoadFromCacheRates(string key)
        {
            var list = _memoryCache.Get<List<Rates>>(key);
            if (list != null)
            {
                return list;
            }
            else
            {
                char sign = key[key.Length - 1];
                switch (sign)
                {
                    case 'A':
                        list = LoadTable(urlA)[0].rates;
                        LoadToCacheRates("listRatesA", list);
                        break;
                    case 'B':
                        list = LoadTable(urlB)[0].rates;
                        LoadToCacheRates("listRatesA", list);
                        break;
                    case 's':
                        var listA = LoadTable(urlA)[0].rates;
                        var listB = LoadTable(urlB)[0].rates;
                        listB.ForEach(item => listA.Add(item));
                        list = listA;
                        LoadToCacheRates("listRatesA", listA);
                        LoadToCacheRates("listRatesB", listB);
                        LoadToCacheRates("fullListRates", list);
                        break;
                }
                return list;
            }
        }

        /// <summary>
        /// Pobiera tabelę z walutami oraz ich kursem z API NBP
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<RootTable> LoadTable(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var resultString = client.GetStringAsync(url).Result;
            var table = JsonConvert.DeserializeObject<List<RootTable>>(resultString);
            if (table == null)
                return null;
            return table;
        }

        /// <summary>
        /// Zwraca wszystkie trzy literowe kody walut
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Zostały zwrócone wszystkie trzy literowe kody walut</response>
        /// <response code="404">Nic nie zostało znalezione</response>
        // GET: api/<NBPCodesController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IEnumerable<string> Get()
        {
            //pobieram z cache
            var FullListRates = LoadFromCacheRates("fullListRates");

            List<string> stringCode = new List<string>();

            foreach (var rate in FullListRates)
            {
                stringCode.Add(rate.code);
            }
            return stringCode;
        }

        /// <summary>
        /// Zwraca kurs waluty podanej jako trzy literowy kod w parametrze
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <response code="200">Został zwrócony kurs danej waluty</response>
        /// <response code="400">Podano nie poprawny parametr</response>
        /// <response code="404">Wprowadzona waluta nie została znaleziona</response>
        // GET api/<NBPCodesController>/PLN
        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string code)
        {
            if (code.Length != 3)
            {
                return BadRequest();
            }

            var FullListRates = LoadFromCacheRates("fullListRates");

            if (FullListRates == null)
                return NotFound();

            foreach (var obj in FullListRates)
            {
                if (obj.code.ToUpper() == code.ToUpper())
                    return Ok(obj.mid);
                else
                    continue;
            }
            return NotFound();
        }

        /// <summary>
        /// Zwraca przeliczoną wartość waluty na złotówki, przy podaniu trzy literowego kodu oraz kwoty danej waluty
        /// </summary>
        /// <param name="code"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <response code="200">Została zwrócona wartość w złotówkach</response>
        /// <response code="400">Podano nie poprawne parametry</response>
        /// <response code="404">Wprowadzona waluta nie została znaleziona</response>
        // GET api/<NBPCodesController>/PLN/value
        [HttpGet("{code}/{value}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string code, double value)
        {
            if (code.Length != 3 || value < 0)
            {
                return BadRequest();
            }

            var FullListRates = LoadFromCacheRates("fullListRates");

            if (FullListRates == null)
                return NotFound();

            foreach (var obj in FullListRates)
            {
                if (obj.code.ToUpper() == code.ToUpper())

                    return Ok(Math.Round(value * obj.mid, 2));
                else
                    continue;
            }
            return NotFound();
        }
    }
}
