# Interview Coach with Microsoft Agent Framework

This is a sample application practicing job interview using [Microsoft Agent Framework](https://aka.ms/agent-framework).

## Architecture

![Overall architecture](./assets/architecture.png)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](http://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [Azure Subscription (Free)](http://azure.microsoft.com/free)

## Getting Started

### Set `REPOSITORY_ROOT`

```bash
# zsh/bash
REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
```

```powershell
# PowerShell
$REPOSITORY_ROOT = git rev-parse --show-toplevel
```

### Download MarkItDown MCP Server

```bash
# zsh/bash
mkdir -p $REPOSITORY_ROOT/src/InterviewCoach.Mcp.MarkItDown && \
    git clone https://github.com/microsoft/markitdown $REPOSITORY_ROOT/src/InterviewCoach.Mcp.MarkItDown
```

```powershell
# PowerShell
New-Item -Type Directory -Path $REPOSITORY_ROOT/src/InterviewCoach.Mcp.MarkItDown -Force && `
    git clone https://github.com/microsoft/markitdown $REPOSITORY_ROOT/src/InterviewCoach.Mcp.MarkItDown
```

### Use GitHub Models

1. Get the [GitHub Personal Access Token (PAT)](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-fine-grained-personal-access-token) to access to [GitHub Models](https://github.com/marketplace?type=models).

1. Store the GitHub PAT to user secrets.

    ```bash
    dotnet user-secrets --file ./apphost.cs set GitHub:Token {{GITHUB_PAT}}
    ```

1. Make sure that `src/InterviewCoach.AppHost/appsettings.json` or `apphost.settings.json` points to use GitHub Models. You can change the default model from `openai/gpt-5-mini` to your preferred one.

    ```jsonc
    {
      "LlmProvider": "GitHubModels",

      "GitHub": {
        "Endpoint": "https://models.github.ai/inference",
        "Token": "{{GITHUB_PAT}}",
        "Model": "openai/gpt-5-mini"
      }
    }
    ```

### Use Azure OpenAI

1. Login to Azure

    ```bash
    az login
    ```

1. Assuming you've already got an active [Azure OpenAI](https://azure.microsoft.com/products/ai-foundry/models/openai) instance. Get both endpoint and API from that instance.

    ```bash
    # zsh/bash
    AZURE_OPENAI_ENDPOINT=$(az cognitiveservices account show -g rg-juyoo-aoai -n openai-6heq7tc4w2ols --query "properties.endpoint" -o tsv)
    AZURE_OPENAI_API_KEY=$(az cognitiveservices account keys list -g rg-juyoo-aoai -n openai-6heq7tc4w2ols --query "key1" -o tsv)
    ```

    ```powershell
    # PowerShell
    $AZURE_OPENAI_ENDPOINT = az cognitiveservices account show -g rg-juyoo-aoai -n openai-6heq7tc4w2ols --query "properties.endpoint" -o tsv
    $AZURE_OPENAI_API_KEY = az cognitiveservices account keys list -g rg-juyoo-aoai -n openai-6heq7tc4w2ols --query "key1" -o tsv
    ```

1. Store both endpoint and API key to user secrets.

    ```bash
    dotnet user-secrets --file ./apphost.cs set Azure:OpenAI:Endpoint $AZURE_OPENAI_ENDPOINT
    dotnet user-secrets --file ./apphost.cs set Azure:OpenAI:ApiKey $AZURE_OPENAI_API_KEY
    ```

1. Make sure that `src/InterviewCoach.AppHost/appsettings.json` or `apphost.settings.json` points to use Azure OpenAI. You can change the default model from `gpt-5-mini` to your preferred one.

    ```jsonc
    {
      "LlmProvider": "AzureOpenAI",

      "Azure": {
        "OpenAI": {
          "Endpoint": "{{AZURE_OPENAI_ENDPOINT}}",
          "ApiKey": "{{AZURE_OPENAI_API_KEY}}",
          "DeploymentName": "gpt-5-mini"
        }
      }
    }
    ```

### Run Aspire

You can run Aspire from either way - file-based `apphost.cs` or project-based `AppHost.csproj`.

1. Run Aspire from the file-based `apphost.cs`.

    ```bash
    dotnet run --file ./apphost.cs
    ```

1. Run Aspire from the project-based `AppHost.csproj`.

    ```bash
    dotnet run --project ./src/InterviewCoach.AppHost
    ```

1. Open Aspire dashboard then navigate to the `webui` instance to run the interview coach app.

### Deploy to Azure

1. Login to Azure.

    ```bash
    azd auth login
    ```

   > **NOTE**: You might have to use the option `--scope https://storage.azure.com/.default`.

1. Publish to Azure.

    ```bash
    azd up
    ```

1. Open the `webui` container app instance to run the interview coach app.

1. Once completed delete all the resources to avoid unexpected billing shock.

    ```bash
    azd down --force --purge
    ```

## Additional Resources

- [Microsoft Agent Framework](https://aka.ms/agent-framework)
- [Multi-Agent Orchestration Pattern](https://learn.microsoft.com/agent-framework/user-guide/workflows/orchestrations/overview)
- [AG-UI Protocol](https://docs.ag-ui.com/introduction)
- [MarkItDown MCP Server](https://github.com/microsoft/markitdown/tree/main/packages/markitdown-mcp)
- [Aspire](https://aspire.dev)
<!-- - [Outlook Email MCP Server](https://github.com/microsoft/mcp-dotnet-samples/tree/main/outlook-email)
- [OneDrive Download MCP Server](https://github.com/microsoft/mcp-dotnet-samples/tree/main/onedrive-download)
- [Google Drive Download MCP Server](https://github.com/microsoft/mcp-dotnet-samples/tree/main/googledrive-download) -->
