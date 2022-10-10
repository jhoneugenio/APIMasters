using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace APIMasters
{
    [TestClass]
    public class HttpClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{PetEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task AssignmentNumber2()
        {
            #region create data

            var categoryName = "Cat";
            var petName = "Maven";
            var tagName = "Cute";
            var newStatus = "available";

            Category newCategory = new Category()
            {
                Id = 6969,
                Name = categoryName
            };

            Category tempTag = new Category()
            {
                Id = 7070,
                Name = tagName
            };

            Category[] newTag = { tempTag };

            // Create Json Object
            PetModel petData = new PetModel()
            {
                Id = 4444,
                Category = newCategory,
                Name = petName,
                PhotoUrls = new string[] { "https://cutepets.com/" },
                Tags = newTag,
                Status = newStatus
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(PetEndpoint), postRequest);

            #endregion

            #region send put request to update data

            // Update value of userData
            petData = new PetModel()
            {
                Id = 5555, //update
                Category = newCategory,
                Name = petName,
                PhotoUrls = new string[] { "https://petscute.com/" }, //update
                Tags = newTag,
                Status = "sold" //update
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{PetEndpoint}"), postRequest);

            // Get Status Code
            var putStatusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));
            var getStatusCode = getResponse.StatusCode;

            // Deserialize Content
            var listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, putStatusCode, "Put Status code is not equal to 200");
            Assert.AreEqual(HttpStatusCode.OK, getStatusCode, "Get Status code is not equal to 200");
            Assert.AreEqual(petData.Id, listPetData.Id, "Id is not equal on updated value");
            Assert.AreEqual(petData.Category.Id, listPetData.Category.Id, "Category Id is not equal on updated value");
            Assert.AreEqual(petData.Category.Name, listPetData.Category.Name, "Category Name is not equal on updated value");
            Assert.AreEqual(petData.Name, listPetData.Name, "Name is not equal on updated value");
            Assert.AreEqual(petData.PhotoUrls[0], listPetData.PhotoUrls[0], "PhotoUrls is not equal on updated value");
            Assert.AreEqual(petData.Tags[0].Id, listPetData.Tags[0].Id, "Tags Id is not equal on updated value");
            Assert.AreEqual(petData.Tags[0].Name, listPetData.Tags[0].Name, "Tags Name is not equal on updated value");
            Assert.AreEqual(petData.Status, listPetData.Status, "Status is not equal on updated value");

            #endregion

            #region cleanupdata

            // Add data to cleanup list
            cleanUpList.Add(listPetData);

            #endregion
        }

    }
}