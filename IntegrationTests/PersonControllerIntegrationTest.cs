using Entities.DTO.Request.Person;
using IntegrationTests.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Persistence.Context;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Entities.Constants;

namespace IntegrationTests;

[TestFixture]
[SingleThreaded]
public class PersonControllerIntegrationTest : BaseConfigurationIntegrationTest
{
    private HttpClient? _client;
    private ApiContext? _context;
    private const string ApiV1Person = "/api/v1.0/Person";

    [SetUp]
    public void SetUp()
    {
        WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });

        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApiContext>()!;
    }

    //	Marks a method that should be called after each test method. One such method should be present before each test class.
    [TearDown]
    public void TearDown()
    {
        _context?.Database.EnsureDeleted();
    }

    [Test, CustomAutoData]
    public async Task GetPersonFromId_Success(PersonEntity personEntity)
    {
        // Arrange
        //_context?.Database.EnsureCreated();
        //_context?.Person?.Add(personEntity);
        //_context?.SaveChanges();

        // Act
        var response = await _client?.WithJwtBearerToken(token => token.WithRole(Roles.Admin)).GetAsync($"{ApiV1Person}/{personEntity.Id}")!;

        // Assert
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();
        Assert.AreEqual(personEntity.Id, responseJson?.Id);
        Assert.AreEqual(personEntity.Name, responseJson?.Name);
        Assert.AreEqual(personEntity.Email, responseJson?.Email);
        Assert.AreEqual(personEntity.Phone, responseJson?.Phone);
    }

    [TestMethod]
    public async Task GetPersonFromId_NotFound()
    {
        // Act
        var response = await _client?.GetAsync($"{ApiV1Person}/0")!;

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test, CustomAutoData]
    public async Task AddPerson_ValidInput_Async(PersonDtoRequest personDto)
    {
        // Arrange
        var jsonContent = JsonConvert.SerializeObject(personDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client?.PostAsync(ApiV1Person, content)!;

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();

        Assert.IsNotNull(responseJson?.Id);
        Assert.AreEqual(personDto.Name, responseJson?.Name);
        Assert.AreEqual(personDto.Email, responseJson?.Email);
        Assert.AreEqual(personDto.Phone, responseJson?.Phone);
    }

    [Test]
    public async Task AddPerson_InvalidInput_Async()
    {
        // Arrange
        var jsonContent = JsonConvert.SerializeObject(new PersonDtoRequest() { Email = "", Name = "", Phone = "" });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client?.PostAsync(ApiV1Person, content)!;

        // Assert
        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseString);
        Assert.IsTrue(responseString.Contains("The Name field is required."));
        Assert.IsTrue(responseString.Contains("The Email field is required."));
        Assert.IsTrue(responseString.Contains("The Phone field is required."));
    }

    [Test, CustomAutoData]
    public async Task DeletePersonAsync_ShouldReturn_Status204NoContent_WhenPersonExists(PersonEntity personEntity)
    {
        //Arrange
        _context?.Database.EnsureCreated();
        _context?.Person?.Add(personEntity);
        _context?.SaveChanges();

        //Act
        var result = await _client?.DeleteAsync($"{ApiV1Person}/{personEntity.Id}")!;

        //Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Test]
    public async Task DeletePersonAsync_ShouldReturn_Status404NotFound_WhenPersonDoesNotExist()
    {
        //Act
        var result = await _client?.DeleteAsync($"{ApiV1Person}/-1")!;

        //Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Test, CustomAutoData]
    public async Task UpdatePersonAsync_ReturnsOkObjectResult_WhenPersonIsUpdated(PersonEntity personEntity)
    {
        // Arrange
        _context?.Database.EnsureCreated();
        personEntity.Days = new List<DayEntity>();
        _context?.Person?.Add(personEntity);
        _context?.SaveChanges();

        PersonDtoRequest personDto = new() { Email = "NewEmail", Name = "NewName", Phone = "NewPhone" };
        var jsonContent = JsonConvert.SerializeObject(personDto);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client?.PutAsync($"{ApiV1Person}/{personEntity.Id}", content)!;

        // Assert
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();

        Assert.IsNotNull(responseJson);
        Assert.AreEqual(personEntity.Id, responseJson?.Id);
        Assert.AreEqual(personDto.Name, responseJson?.Name);
        Assert.AreEqual(personDto.Email, responseJson?.Email);
        Assert.AreEqual(personDto.Phone, responseJson?.Phone);
    }

    [Test]
    public async Task UpdatePersonAsync_ReturnsNotFoundResult_WhenPersonDoesNotExist()
    {
        // Arrange
        PersonDtoRequest personDto = new() { Email = "NewEmail", Name = "NewName", Phone = "NewPhone" };
        var jsonContent = JsonConvert.SerializeObject(personDto);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client?.PutAsync($"{ApiV1Person}/-1", content)!;

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task UpdatePersonAsync_ReturnsUnprocessableEntityObjectResult_WhenModelIsInvalid()
    {
        // Arrange
        var jsonContent = JsonConvert.SerializeObject(new PersonDtoRequest() { Email = "", Name = "", Phone = "" });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client?.PutAsync($"{ApiV1Person}/1", content)!;

        // Assert
        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseString);
        Assert.IsTrue(responseString.Contains("The Name field is required."));
        Assert.IsTrue(responseString.Contains("The Email field is required."));
        Assert.IsTrue(responseString.Contains("The Phone field is required."));
    }
}