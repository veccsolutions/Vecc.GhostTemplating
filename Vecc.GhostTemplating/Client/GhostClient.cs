using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vecc.GhostTemplating.Client
{
    public class GhostClient
    {
        private readonly HttpClient _adminClient;
        private readonly HttpClient _contentClient;
        private readonly string _contentKey;
        private readonly ILogger<GhostClient> _logger;

        public GhostClient(ILogger<GhostClient> logger, IOptions<TemplatingOptions> options)
        {
            _contentKey = options.Value.ContentKey;

            _adminClient = new HttpClient();
            _adminClient.BaseAddress = new Uri(options.Value.ApiUrl + "admin/");

            _contentClient = new HttpClient();
            _contentClient.BaseAddress = new Uri(options.Value.ApiUrl + "content/");

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = (long)(DateTime.UtcNow - epoch).TotalSeconds;

            var keyParts = options.Value.AdminKey.Split(':');
            var key = Enumerable.Range(0, keyParts[1].Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(keyParts[1].Substring(x, 2), 16))
                     .ToArray();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyParts[1])) { KeyId = keyParts[0] };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateJwtSecurityToken(audience: "/v3/admin/",
                expires: DateTime.UtcNow.AddMinutes(5).AddSeconds(-30),
                issuedAt: DateTime.UtcNow.AddSeconds(-30),
                notBefore: DateTime.UtcNow.AddSeconds(-30),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));

            var hasher = new System.Security.Cryptography.HMACSHA256(key);
            Console.WriteLine(securityToken.EncodedHeader + "." + securityToken.EncodedPayload);
            var hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(securityToken.EncodedHeader + "." + securityToken.EncodedPayload));
            var hash = Convert.ToBase64String(hashBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            Console.WriteLine(hash);
            Console.WriteLine(securityToken.EncodedHeader + "." + securityToken.EncodedPayload + "." + hash);
            _adminClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Ghost", securityToken.EncodedHeader + "." + securityToken.EncodedPayload + "." + hash);
            _logger = logger;
        }

        public async Task<Author[]> GetAuthorsAsync()
        {
            var result = await _contentClient.GetAsync("authors/?include=count.posts&limit=all&key=" + _contentKey);
            try
            {
                result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();

                var authors = JsonSerializer.Deserialize<AuthorsResponse>(content);
                return authors.Authors;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to get authors {@response}", await result.Content.ReadAsStringAsync());
                throw;
            }
        }

        public async Task<Page[]> GetPagesAsync()
        {
            var result = await _adminClient.GetAsync("pages/?include=authors,tags&formats=html,plaintext&limit=all");
            try
            {
                result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();

                var pages = JsonSerializer.Deserialize<PagesResponse>(content);
                return pages.Pages;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to get pages {@response}", await result.Content.ReadAsStringAsync());
                throw;
            }
        }

        public async Task<Post[]> GetPostsAsync()
        {
            var result = await _adminClient.GetAsync("posts/?include=authors,tags&formats=html,plaintext,mobiledoc&limit=all");
            try
            {
                result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();

                var posts = JsonSerializer.Deserialize<PostsResponse>(content);
                return posts.Posts;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to get posts {@response}", await result.Content.ReadAsStringAsync());
                throw;
            }
        }

        public async Task<Settings> GetSettingsAsync()
        {
            var result = await _contentClient.GetAsync("settings/?limit=all&key=" + _contentKey);
            try
            {
                result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();

                var settings = JsonSerializer.Deserialize<SettingsResponse>(content);
                return settings.Settings;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to get settings {@response}", await result.Content.ReadAsStringAsync());
                throw;
            }
        }

        public async Task<Tag[]> GetTagsAsync()
        {
            var result = await _contentClient.GetAsync("tags/?include=count.posts&limit=all&key=" + _contentKey);
            try
            {
                result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();

                var tags = JsonSerializer.Deserialize<TagsResponse>(content);
                return tags.Tags;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to get tags {@response}", await result.Content.ReadAsStringAsync());
                throw;
            }
        }
    }
}
