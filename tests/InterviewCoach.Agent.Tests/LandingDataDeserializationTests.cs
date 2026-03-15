using System.Text.Json;

using InterviewCoach.WebUI.Services;

using Shouldly;

using Xunit;

namespace InterviewCoach.Agent.Tests;

public class LandingDataDeserializationTests
{
    private static readonly JsonSerializerOptions WebSerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Given_ServicesJsonFile_When_Deserialize_Invoked_Then_It_Should_Load_All_Service_Cards()
    {
        // Arrange
        var servicesJsonPath = GetRepoRelativePath("src", "InterviewCoach.WebUI", "wwwroot", "data", "services.json");
        var json = File.ReadAllText(servicesJsonPath);

        // Act
        var data = JsonSerializer.Deserialize<ServicesData>(json, WebSerializerOptions);

        // Assert
        data.ShouldNotBeNull();
        data.Cards.Length.ShouldBe(6);
        data.Cards.All(c => !string.IsNullOrWhiteSpace(c.Icon)).ShouldBeTrue();
        data.Cards.All(c => !string.IsNullOrWhiteSpace(c.Name)).ShouldBeTrue();
        data.Cards.All(c => !string.IsNullOrWhiteSpace(c.Description)).ShouldBeTrue();
    }

    [Fact]
    public void Given_ServicesJson_Without_Cards_When_Deserialize_Invoked_Then_It_Should_Default_To_Empty_Array()
    {
        // Arrange
        const string json = """
            {
              "description": "desc",
              "skills": ["skill"],
              "detailOrQuote": "quote"
            }
            """;

        // Act
        var data = JsonSerializer.Deserialize<ServicesData>(json, WebSerializerOptions);

        // Assert
        data.ShouldNotBeNull();
        data.Cards.ShouldNotBeNull();
        data.Cards.ShouldBeEmpty();
    }

    [Fact]
    public void Given_TechnologiesJsonFile_When_Deserialize_Invoked_Then_It_Should_Load_Bands_With_Technologies()
    {
        // Arrange
        var technologiesJsonPath = GetRepoRelativePath("src", "InterviewCoach.WebUI", "wwwroot", "data", "technologies.json");
        var json = File.ReadAllText(technologiesJsonPath);

        // Act
        var data = JsonSerializer.Deserialize<TechnologiesData>(json, WebSerializerOptions);

        // Assert
        data.ShouldNotBeNull();
        data.Bands.Length.ShouldBeGreaterThanOrEqualTo(3);
        data.Bands.All(b => !string.IsNullOrWhiteSpace(b.Label)).ShouldBeTrue();
        data.Bands.All(b => b.Technologies.Length > 0).ShouldBeTrue();
    }

    private static string GetRepoRelativePath(params string[] parts)
    {
        var projectDirectory = AppContext.BaseDirectory;
        var root = Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(root, Path.Combine(parts));
    }
}
