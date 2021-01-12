# sdk-csharp
A C# sdk that faciliates interacting with telerobotic services

# Setup
```shell
$ GITHUB_TOKEN=<github_token> bash download_libsdk
```

# Test
```shell
$ dotnet test
```

## Updating libsdk asset
```shell
$ curl -s -u dopl-builder:<githubtoken> https://api.github.com/repos/dopl-technologies/libsdk/releases/tags/v1.0.0

# Update download_libsdk
```