using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ClientFixture : IDisposable
    {
        private static readonly ConnectionIssuesHandler s_httpHandler;

        private readonly IPictureparkService _client;
        private readonly TestConfiguration _configuration;
        private readonly ConcurrentQueue<string> _createdUserIds = new ConcurrentQueue<string>();

        static ClientFixture()
        {
            s_httpHandler = new ConnectionIssuesHandler(new PictureparkRetryHandler());
        }

        public ClientFixture()
        {
#if NET452
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;
#endif

            var assemblyDirectory = Path.GetFullPath(Path.GetDirectoryName(typeof(ClientFixture).GetTypeInfo().Assembly.Location));
            ProjectDirectory = Path.GetFullPath(assemblyDirectory + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(ProjectDirectory + "../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../");

            if (!File.Exists(ProjectDirectory + "Configuration.json"))
                ProjectDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../../../");

            if (!Directory.Exists(TempDirectory))
                Directory.CreateDirectory(TempDirectory);

            var configurationJson = File.ReadAllText(ProjectDirectory + "Configuration.json");
            _configuration = JsonConvert.DeserializeObject<TestConfiguration>(configurationJson);

            _client = GetLocalizedPictureparkService("en");
        }

        public string ProjectDirectory { get; }

        public string TempDirectory => ProjectDirectory + "/Temp";

        public string ExampleFilesBasePath => ProjectDirectory + "/ExampleData/Pool";

        public string ExampleSchemaBasePath => ProjectDirectory + "/ExampleData/Schema";

        public TestConfiguration Configuration => _configuration;

        public IPictureparkService Client => _client;

        public Lazy<CustomerInfo> CustomerInfo =>
            new Lazy<CustomerInfo>(() => _client.Info.GetInfoAsync().GetAwaiter().GetResult());

        public string DefaultLanguage => CustomerInfo.Value.LanguageConfiguration.DefaultLanguage;

        public async Task<ContentSearchResult> GetRandomContentsAsync(string searchString, int limit, IReadOnlyList<ContentType> contentTypes = null)
        {
            return await RandomHelper.GetRandomContentsAsync(_client, searchString, limit, contentTypes);
        }

        public async Task<string> GetRandomContentIdAsync(string searchString, int limit)
        {
            return await RandomHelper.GetRandomContentIdAsync(_client, searchString, limit);
        }

        public async Task<string> GetRandomContentPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomContentPermissionSetIdAsync(_client, limit);
        }

        public async Task<string> GetRandomSchemaPermissionSetIdAsync(int limit)
        {
            return await RandomHelper.GetRandomSchemaPermissionSetIdAsync(_client, limit);
        }

        public virtual void Dispose()
        {
            if (!_createdUserIds.IsEmpty)
            {
                foreach (var createdUserId in _createdUserIds)
                {
                    Client.User.DeleteAsync(createdUserId, new UserDeleteRequest()).GetAwaiter().GetResult();
                }
            }

            _client.Dispose();
        }

        public PictureparkService GetLocalizedPictureparkService(string language)
        {
            var authClient = new AccessTokenAuthClient(_configuration.Server, _configuration.AccessToken, _configuration.CustomerAlias);

            var settings = new PictureparkServiceSettings(authClient)
            {
                DisplayLanguage = language,
                HttpTimeout = TimeSpan.FromMinutes(5)
            };

            var httpClient = new HttpClient(s_httpHandler) { Timeout = settings.HttpTimeout };

            return new PictureparkService(settings, httpClient);
        }

        public async Task<IReadOnlyList<UserDetail>> CreateAndActivateUsers(int userCount)
        {
            var usersToCreate = Enumerable.Range(0, userCount).Select(_ =>
                new UserCreateRequest
                {
                    FirstName = "Test",
                    LastName = "User",
                    EmailAddress = $"test.user_{Guid.NewGuid()}@test.picturepark.com",
                    LanguageCode = "en"
                }).ToArray();

            var userCreationTasks = usersToCreate.Select(userRequest => Client.User.CreateAsync(userRequest));

            var createdUsers = await Task.WhenAll(userCreationTasks);

            var searchRes = await Client.User.SearchAsync(new UserSearchRequest
            {
                Filter = FilterBase.FromExpression<User>(u => u.EmailAddress, usersToCreate.Select(u => u.EmailAddress).ToArray())
            });

            searchRes.Results.Select(e => e.EmailAddress).Should()
                .BeEquivalentTo(usersToCreate.Select(e => e.EmailAddress), "all users should have been created");

            foreach (var createdUser in createdUsers)
            {
                await Client.User.ReviewAsync(createdUser.Id, new UserReviewRequest
                {
                    Reviewed = true
                });
            }

            var createdUserIds = createdUsers.Select(u => u.Id);
            var reviewedUsers = (await Client.User.GetManyAsync(createdUserIds)).ToArray();

            reviewedUsers.Should()
                .HaveCount(createdUsers.Length, "all previously created users should have been retrieved");

            reviewedUsers.Should().OnlyContain(
                u => u.AuthorizationState == AuthorizationState.Reviewed, "all invited users should be reviewed after review");

            foreach (var reviewedUser in reviewedUsers)
            {
                _createdUserIds.Enqueue(reviewedUser.Id);
            }

            return reviewedUsers;
        }

        public async Task<UserDetail> CreateAndActivateUser()
        {
            return (await CreateAndActivateUsers(1)).Single();
        }
    }
}
