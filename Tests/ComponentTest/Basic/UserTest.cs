// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Api.Client;
using Api.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ComponentTest.Basic
{
    [TestClass]
    [TestCategory("Component")]
    public class UserTest
    {
        private static IHttpClientFactory _httpClientFactory = null!;
        private ApiClient _client = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            ServiceCollection services = new();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            AnonymousAuthenticationProvider provider = new();
            HttpClientRequestAdapter adapter = new(authenticationProvider: provider, httpClient: httpClient);
            _client = new(adapter);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public async Task CreateUser()
        {
            try
            {
                var response = await _client.Users.PostAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                if (string.IsNullOrEmpty(response.Id))
                {
                    Assert.Fail(response.ToString());
                    return;
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task GetUserList()
        {
            try
            {
                var response = await _client.Users.GetAsUserListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                if ((response.Count is not int count) ||
                    (response.Users is not List<User> users))
                {
                    Assert.Fail(response.ToString());
                    return;
                }
                Assert.AreEqual(count, users.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task GetUser()
        {
            string id;
            try
            {
                var response = await _client.Users.PostAsync();
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Users[id].GetAsUserGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                Assert.AreEqual(id, response.Id);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task GetUser_NotFound()
        {
            try
            {
                Guid guid = Guid.NewGuid();
                string id = guid.ToString();
                await _client.Users[id].GetAsUserGetResponseAsync();
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task GetUser_InvalidId()
        {
            try
            {
                await _client.Users["invalid"].GetAsUserGetResponseAsync();
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task UpdateUser()
        {
            string id;
            try
            {
                var response = await _client.Users.PostAsync();
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            UserName userName;
            try
            {
                UserUpdateRequest request = new()
                {
                    Name = new()
                    {
                        First = "Test",
                        Middle = "M",
                        Last = "User",
                        Display = "Test User"
                    }
                };
                await _client.Users[id].PatchAsync(request);
                userName = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Users[id].GetAsUserGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(userName.First, response.Name?.First);
                Assert.AreEqual(userName.Middle, response.Name?.Middle);
                Assert.AreEqual(userName.Last, response.Name?.Last);
                Assert.AreEqual(userName.Display, response.Name?.Display);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateUser_Overwrite()
        {
            string id;
            try
            {
                var response = await _client.Users.PostAsync();
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                UserUpdateRequest request = new()
                {
                    Name = new()
                    {
                        First = "Test",
                        Middle = "M",
                        Last = "User",
                        Display = "Test User"
                    }
                };
                await _client.Users[id].PatchAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            UserName userName = new()
            {
                Display = "TEST"
            };
            try
            {
                UserUpdateRequest request = new()
                {
                    Name = userName
                };
                await _client.Users[id].PatchAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Users[id].GetAsUserGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(userName.Display, response.Name?.Display);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateUser_NotFound()
        {
            try
            {
                Guid guid = Guid.NewGuid();
                string id = guid.ToString();
                UserUpdateRequest request = new();
                await _client.Users[id].PatchAsync(request);
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task UpdateUser_InvalidId()
        {
            try
            {
                UserUpdateRequest request = new();
                await _client.Users["invalid"].PatchAsync(request);
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task DeleteUser()
        {
            string id;
            try
            {
                var response = await _client.Users.PostAsync();
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
            try
            {
                await _client.Users[id].DeleteAsync();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                await _client.Users[id].GetAsUserGetResponseAsync();
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task DeleteUser_NotFound()
        {
            try
            {
                Guid guid = Guid.NewGuid();
                string id = guid.ToString();
                await _client.Users[id].DeleteAsync();
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        [TestMethod]
        public async Task DeleteUser_InvalidId()
        {
            try
            {
                await _client.Users["invalid"].DeleteAsync();
                Assert.Fail();
                return;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
