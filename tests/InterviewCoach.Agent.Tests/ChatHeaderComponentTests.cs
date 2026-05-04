using Bunit;

using InterviewCoach.WebUI.Components.Pages.Chat;

using Shouldly;

using Xunit;

namespace InterviewCoach.Agent.Tests;

public class ChatHeaderComponentTests : TestContext
{
    [Fact]
    public void Given_IsEmpty_True_When_Rendered_Then_Download_Button_Is_Disabled()
    {
        // Arrange & Act
        var cut = RenderComponent<ChatHeader>(parameters => parameters
            .Add(p => p.IsEmpty, true));

        // Assert
        var downloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Download"));
        downloadButton.ShouldNotBeNull();
        downloadButton!.HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void Given_IsEmpty_False_When_Rendered_Then_Download_Button_Is_Enabled()
    {
        // Arrange & Act
        var cut = RenderComponent<ChatHeader>(parameters => parameters
            .Add(p => p.IsEmpty, false));

        // Assert
        var downloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Download"));
        downloadButton.ShouldNotBeNull();
        downloadButton!.HasAttribute("disabled").ShouldBeFalse();
    }

    [Fact]
    public void Given_Default_ChatHeader_When_Rendered_Then_New_Chat_And_Download_Buttons_Are_Present()
    {
        // Arrange & Act
        var cut = RenderComponent<ChatHeader>();

        // Assert
        cut.FindAll("button").Count.ShouldBe(2);
        cut.FindAll("button").Any(b => b.TextContent.Contains("New chat")).ShouldBeTrue();
        cut.FindAll("button").Any(b => b.TextContent.Contains("Download")).ShouldBeTrue();
    }
}
