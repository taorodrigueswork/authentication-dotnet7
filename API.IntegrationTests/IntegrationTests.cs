//using API.IntegrationTests.Authentication;
//using AutoFixture;
//using Entities.Constants;
//using Entities.DTO.Request.Day;
//using Entities.DTO.Request.Person;
//using Entities.Entity;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Newtonsoft.Json;
//using Persistence.Context;
//using System.Data.Common;
//using System.Net;
//using System.Net.Http.Json;
//using System.Text;
//using API.Configurations;

//namespace API.IntegrationTests;

//[TestFixture]
//[SingleThreaded]
//[NonParallelizable]
//public class IntegrationTests
//{
//    private const string ApiV1Day = "/api/v1.0/Day";
//    private const string ApiV1Person = "/api/v1.0/Person";

//    protected WebApplicationFactory<Program> Factory;
//    protected HttpClient Client;
//    protected ApiContext? Context;
//    protected Fixture? Fixture;

//    #region SetUp

//    [OneTimeSetUp]
//    public void OneTimeSetUp()
//    {
//        UnitTestDetector.IsInUnitTest = true;
//        Factory = new WebApplicationFactory<Program>();
//        Factory = Factory.WithWebHostBuilder(builder =>
//        {
//            builder.ConfigureServices(services =>
//            {
//                _ = services.RemoveAll(typeof(DbContextOptions));
//                _ = services.RemoveAll(typeof(ApiContext));
//                _ = services.RemoveAll(typeof(DbContextOptions<ApiContext>));
//                _ = services.RemoveAll(typeof(DbConnection));

//                services.AddSingleton<DbConnection, SqliteConnection>(container =>
//                {
//                    var connection = new SqliteConnection("DataSource=:memory:");
//                    connection.Open();

//                    return connection;
//                });

//                services.AddDbContext<ApiContext>((container, options) =>
//                {
//                    var connection = container.GetRequiredService<DbConnection>();
//                    options.UseSqlite(connection);
//                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
//                });

//                services.Configure<JwtBearerOptions>(
//                    JwtBearerDefaults.AuthenticationScheme,
//                    options =>
//                    {
//                        options.Configuration = new OpenIdConnectConfiguration
//                        {
//                            Issuer = JwtTokenProvider.Issuer,
//                        };
//                        options.TokenValidationParameters.ValidIssuer = JwtTokenProvider.Issuer;
//                        options.TokenValidationParameters.ValidAudience = JwtTokenProvider.Issuer;
//                        options.Configuration.SigningKeys.Add(JwtTokenProvider.SecurityKey);
//                    }
//                );
//            });
//        });
//        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions()
//        {
//            AllowAutoRedirect = false
//        });
//    }

//    [SetUp]
//    public void SetUp()
//    {
//        Thread.Sleep(8000);
//        var scope = Factory.Services.CreateScope();
//        Context = scope.ServiceProvider.GetRequiredService<ApiContext>()!;
//        Context?.Database.EnsureCreated();
//        Fixture = CustomAutoDataAttribute.CreateOmitOnRecursionFixture();

//    }

//    //	Marks a method that should be called after each test method. One such method should be present before each test class.
//    [TearDown]
//    public void TearDown()
//    {
//        Context?.Database.EnsureDeleted();
//    }

//    [OneTimeTearDown]
//    public async ValueTask DisposeAsync()
//    {
//        GC.SuppressFinalize(this);
//        await Factory.DisposeAsync();
//    }

//    #endregion

//    #region DayControler

//    //[NonParallelizable]
//    //[Test, CustomAutoData]
//    //public async Task GetDayFromId_Success(DayEntity dayEntity)
//    //{
//    //    // Arrange
//    //    dayEntity.Id = 9000;
//    //    Context?.Day?.Add(dayEntity);
//    //    await Context?.SaveChangesAsync();

//    //    // Act
//    //    var response = await Client?
//    //        .WithJwtBearerToken(token => token.WithRole(Roles.User))
//    //        .GetAsync($"{ApiV1Day}/{dayEntity.Id}")!;

//    //    // Assert
//    //    response.EnsureSuccessStatusCode();
//    //    var responseJson = await response.Content.ReadFromJsonAsync<DayEntity>();
//    //    Assert.AreEqual(dayEntity.Id, responseJson?.Id);
//    //    Assert.AreEqual(dayEntity.Day, responseJson?.Day);
//    //    Assert.AreEqual(dayEntity.Schedule.Id, responseJson?.Schedule.Id);
//    //    Assert.Greater(dayEntity.People.Count, 1);
//    //}

//    [NonParallelizable]
//    [Test]
//    public async Task GetDayFromId_NotFound()
//    {
//        // Act
//        var response = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .GetAsync($"{ApiV1Day}/0")!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
//    }

//    [NonParallelizable]
//    [Test]
//    public async Task AddDay_ValidInput_Async()
//    {
//        // Arrange
//        var personEntity = CreatePersonAndScheduleEntity(out var scheduleEntity, 50);

//        Context?.SaveChanges();

//        DayDtoRequest dayDto = Fixture?.Build<DayDtoRequest>()
//                                ?.With(p => p.People, new List<int>() { personEntity.Id })
//                                ?.With(p => p.ScheduleId, scheduleEntity.Id)
//                                ?.Create()!;

//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(dayDto);
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PostAsync(ApiV1Day, content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
//        var responseJson = await response.Content.ReadFromJsonAsync<DayEntity>();

//        Assert.IsNotNull(responseJson?.Id);
//        Assert.GreaterOrEqual(responseJson?.People.Count, 1);
//        Assert.AreEqual(dayDto?.Day, responseJson?.Day);
//        Assert.AreEqual(dayDto?.ScheduleId, responseJson?.Schedule.Id);
//    }

//    [NonParallelizable]
//    [Test]
//    public async Task AddDay_InvalidInput_Async()
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(
//            new DayDtoRequest() { ScheduleId = 0, People = new List<int>(), Day = DateTime.Now });
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PostAsync(ApiV1Day, content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//        var responseString = await response.Content.ReadAsStringAsync();

//        Assert.IsNotNull(responseString);
//        Assert.IsTrue(responseString.Contains("The field People must be a string or array type with a minimum length of '1'."));
//        Assert.IsTrue(responseString.Contains("The field ScheduleId must be between 1"));
//    }

//    [NonParallelizable]
//    [Test, CustomAutoData]
//    public async Task DeleteDayAsync_ShouldReturn_Status204NoContent_WhenDayExists(DayEntity dayEntity)
//    {
//        //Arrange
//        dayEntity.Id = 6000;
//        Context?.Day?.Add(dayEntity);
//        Context?.SaveChanges();

//        //Act
//        var result = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .DeleteAsync($"{ApiV1Day}/{dayEntity.Id}")!;

//        //Assert
//        Assert.IsNotNull(result);
//        Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task DeleteDayAsync_ShouldReturn_Status404NotFound_WhenDayDoesNotExist()
//    {
//        //Act
//        var result = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .DeleteAsync($"{ApiV1Day}/1")!;

//        //Assert
//        Assert.IsNotNull(result);
//        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
//    }

//    //[Test]
//    //[NonParallelizable]
//    //public async Task UpdateDayAsync_ReturnsOkObjectResult_WhenDayIsUpdated()
//    //{
//    //    // Arrange
//    //    var personEntity = CreatePersonAndScheduleEntity(out var scheduleEntity, 52);
//    //    var personEntity2 = CreatePersonAndScheduleEntity(out var scheduleEntity2, 53);
//    //    personEntity.Days.Clear();
//    //    personEntity2.Days.Clear();
//    //    scheduleEntity.Days.Clear();
//    //    scheduleEntity2.Days.Clear();

//    //    DayEntity dayEntity = new()
//    //    {
//    //        People = new List<PersonEntity> { personEntity },
//    //        Schedule = scheduleEntity,
//    //        Day = DateTime.Now,
//    //        Id = 7000
//    //    };

//    //    Context?.Day?.Add(dayEntity);
//    //    Context?.Person?.Add(personEntity2);
//    //    Context?.Schedule?.Add(scheduleEntity2);
//    //    Context?.SaveChanges();

//    //    DayDtoRequest dayDto = new()
//    //    {
//    //        People = new List<int>() { personEntity2.Id },
//    //        Day = DateTime.Now,
//    //        ScheduleId = scheduleEntity2.Id
//    //    };

//    //    var jsonContent = JsonConvert.SerializeObject(dayDto);

//    //    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//    //    // Act
//    //    var response = await Client?
//    //        .WithJwtBearerToken(token => token.WithRole(Roles.User))
//    //        .PutAsync($"{ApiV1Day}/{dayEntity.Id}", content)!;

//    //    // Assert
//    //    response.EnsureSuccessStatusCode();
//    //    var responseJson = await response.Content.ReadFromJsonAsync<DayEntity>();

//    //    Assert.IsNotNull(responseJson);
//    //    Assert.AreEqual(dayDto?.People?.FirstOrDefault(), responseJson?.People?.FirstOrDefault()?.Id);
//    //    Assert.AreNotSame(personEntity?.Id, responseJson?.People?.FirstOrDefault()?.Id);
//    //    Assert.AreEqual(dayDto?.ScheduleId, responseJson?.Schedule?.Id);
//    //    Assert.AreNotSame(scheduleEntity?.Id, responseJson?.Schedule?.Id);
//    //}

//    [Test, CustomAutoData]
//    [NonParallelizable]
//    public async Task UpdateDayAsync_ReturnsNotFoundResult_WhenDayDoesNotExist(DayDtoRequest dayDto)
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(dayDto);
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PutAsync($"{ApiV1Day}/-1", content)!;

//        // Assert
//        Assert.IsNotNull(response);
//        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task UpdateDayAsync_ReturnsUnprocessableEntityObjectResult_WhenModelIsInvalid()
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(
//            new DayDtoRequest() { ScheduleId = 0, People = new List<int>(), Day = DateTime.Now });
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client?
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PutAsync($"{ApiV1Day}/1", content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//        var responseString = await response.Content.ReadAsStringAsync();

//        Assert.IsNotNull(responseString);
//        Assert.IsTrue(responseString.Contains("The field People must be a string or array type with a minimum length of '1'."));
//        Assert.IsTrue(responseString.Contains("The field ScheduleId must be between 1"));
//    }

//    private PersonEntity CreatePersonAndScheduleEntity(out ScheduleEntity scheduleEntity, int id)
//    {
//        PersonEntity personEntity = Fixture.Create<PersonEntity>();
//        scheduleEntity = Fixture.Create<ScheduleEntity>();

//        personEntity.Id = id;
//        scheduleEntity.Id = id;

//        Context?.Person?.Add(personEntity);
//        Context?.Schedule?.Add(scheduleEntity);
//        return personEntity;
//    }

//    #endregion

//    #region PersonController


//    [Test, CustomAutoData]
//    [NonParallelizable]
//    public async Task GetPersonFromId_Success(PersonEntity personEntity)
//    {
//        // Arrange
//        personEntity.Id = 9000;
//        personEntity.Days = new List<DayEntity>();
//        Context?.Person?.Add(personEntity);
//        Context?.SaveChanges();

//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .GetAsync($"{ApiV1Person}/{personEntity.Id}")!;

//        // Assert
//        response.EnsureSuccessStatusCode();
//        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();
//        Assert.AreEqual(personEntity.Id, responseJson?.Id);
//        Assert.AreEqual(personEntity.Name, responseJson?.Name);
//        Assert.AreEqual(personEntity.Email, responseJson?.Email);
//        Assert.AreEqual(personEntity.Phone, responseJson?.Phone);
//    }

//    [Test]
//    public async Task GetPersonFromId_NotFound()
//    {
//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .GetAsync($"{ApiV1Person}/0");

//        // Assert
//        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
//    }

//    //[Test]
//    //[NonParallelizable]
//    //public async Task GetPersonFromId_Unauthorized()
//    //{
//    //    // Act
//    //    var response = await Client.GetAsync($"{ApiV1Person}/0");

//    //    // Assert
//    //    Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
//    //}

//    [Test, CustomAutoData]
//    [NonParallelizable]
//    public async Task AddPerson_ValidInput_Async(PersonDtoRequest personDto)
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(personDto);
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client?.PostAsync(ApiV1Person, content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
//        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();

//        Assert.IsNotNull(responseJson?.Id);
//        Assert.AreEqual(personDto.Name, responseJson?.Name);
//        Assert.AreEqual(personDto.Email, responseJson?.Email);
//        Assert.AreEqual(personDto.Phone, responseJson?.Phone);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task AddPerson_InvalidInput_Async()
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(new PersonDtoRequest() { Email = "", Name = "", Phone = "" });
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PostAsync(ApiV1Person, content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//        var responseString = await response.Content.ReadAsStringAsync();

//        Assert.IsNotNull(responseString);
//        Assert.IsTrue(responseString.Contains("The Name field is required."));
//        Assert.IsTrue(responseString.Contains("The Email field is required."));
//        Assert.IsTrue(responseString.Contains("The Phone field is required."));
//    }

//    [Test, CustomAutoData]
//    [NonParallelizable]
//    public async Task DeletePersonAsync_ShouldReturn_Status204NoContent_WhenPersonExists(PersonEntity personEntity)
//    {
//        //Arrange
//        personEntity.Days = new List<DayEntity>();
//        personEntity.Id = 8000;
//        Context?.Person?.Add(personEntity);
//        Context?.SaveChanges();

//        //Act
//        var result = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .DeleteAsync($"{ApiV1Person}/{personEntity.Id}")!;

//        //Assert
//        Assert.IsNotNull(result);
//        Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task DeletePersonAsync_ShouldReturn_Status404NotFound_WhenPersonDoesNotExist()
//    {
//        //Act
//        var result = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .DeleteAsync($"{ApiV1Person}/-1")!;

//        //Assert
//        Assert.IsNotNull(result);
//        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
//    }

//    [Test, CustomAutoData]
//    [NonParallelizable]
//    public async Task UpdatePersonAsync_ReturnsOkObjectResult_WhenPersonIsUpdated(PersonEntity personEntity)
//    {
//        // Arrange
//        personEntity.Days = new List<DayEntity>();
//        Context?.Person?.Add(personEntity);
//        Context?.SaveChanges();

//        PersonDtoRequest personDto = new() { Email = "NewEmail", Name = "NewName", Phone = "NewPhone" };
//        var jsonContent = JsonConvert.SerializeObject(personDto);

//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PutAsync($"{ApiV1Person}/{personEntity.Id}", content)!;

//        // Assert
//        response.EnsureSuccessStatusCode();
//        var responseJson = await response.Content.ReadFromJsonAsync<PersonEntity>();

//        Assert.IsNotNull(responseJson);
//        Assert.AreEqual(personEntity.Id, responseJson?.Id);
//        Assert.AreEqual(personDto.Name, responseJson?.Name);
//        Assert.AreEqual(personDto.Email, responseJson?.Email);
//        Assert.AreEqual(personDto.Phone, responseJson?.Phone);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task UpdatePersonAsync_ReturnsNotFoundResult_WhenPersonDoesNotExist()
//    {
//        // Arrange
//        PersonDtoRequest personDto = new() { Email = "NewEmail", Name = "NewName", Phone = "NewPhone" };
//        var jsonContent = JsonConvert.SerializeObject(personDto);

//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PutAsync($"{ApiV1Person}/-1", content)!;

//        // Assert
//        Assert.IsNotNull(response);
//        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
//    }

//    [Test]
//    [NonParallelizable]
//    public async Task UpdatePersonAsync_ReturnsUnprocessableEntityObjectResult_WhenModelIsInvalid()
//    {
//        // Arrange
//        var jsonContent = JsonConvert.SerializeObject(new PersonDtoRequest() { Email = "", Name = "", Phone = "" });
//        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//        // Act
//        var response = await Client
//            .WithJwtBearerToken(token => token.WithRole(Roles.User))
//            .PutAsync($"{ApiV1Person}/1", content)!;

//        // Assert
//        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
//        var responseString = await response.Content.ReadAsStringAsync();

//        Assert.IsNotNull(responseString);
//        Assert.IsTrue(responseString.Contains("The Name field is required."));
//        Assert.IsTrue(responseString.Contains("The Email field is required."));
//        Assert.IsTrue(responseString.Contains("The Phone field is required."));
//    }

//    #endregion
//}