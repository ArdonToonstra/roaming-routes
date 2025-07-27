using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace RoamingRoutes.Services;

public interface IGoogleDriveService
{
    Task SynchronizeContentAsync();
}

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly string _contentCachePath;
    private readonly string _imagesRootPath;

    private class UrlKeyHelper
    {
        [YamlMember(Alias = "url_key")]
        public string? UrlKey { get; set; }
    }

    public GoogleDriveService(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<GoogleDriveService> logger
    )
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
        _contentCachePath = Path.Combine(_env.ContentRootPath, "_contentCache");
        _imagesRootPath = Path.Combine(_env.WebRootPath, "images");
    }

    public async Task SynchronizeContentAsync()
    {
        _logger.LogInformation("--- Google Drive Smart Sync Started ---");
        try
        {
            var service = Authenticate();
            if (service == null)
                return;

            var rootFolderId = _configuration["GoogleApi:RootFolderId"];
            if (string.IsNullOrEmpty(rootFolderId))
            {
                _logger.LogError("GoogleApi:RootFolderId is not set in your configuration.");
                return;
            }

            var expectedRemoteFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await ListAllRemoteFilesRecursive(service, rootFolderId, "", expectedRemoteFiles);

            CleanUpOrphanedFiles(expectedRemoteFiles);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred during Google Drive synchronization.");
        }
        _logger.LogInformation("--- Google Drive Smart Sync Finished ---");
    }

    private async Task ListAllRemoteFilesRecursive(
        DriveService service,
        string folderId,
        string relativePath,
        HashSet<string> expectedFiles
    )
    {
        var request = service.Files.List();
        request.Q = $"'{folderId}' in parents and trashed=false";
        request.Fields = "files(id, name, mimeType, modifiedTime)";
        var result = await request.ExecuteAsync();

        foreach (var file in result.Files)
        {
            var newRelativePath = Path.Combine(relativePath, file.Name);
            if (file.MimeType == "application/vnd.google-apps.folder")
            {
                await ListAllRemoteFilesRecursive(service, file.Id, newRelativePath, expectedFiles);
            }
            else
            {
                var localPath = await GetCorrectLocalPath(service, file, newRelativePath);
                if (localPath != null)
                {
                    expectedFiles.Add(localPath);
                    await DownloadFileIfNeeded(service, file, localPath);
                }
            }
        }
    }

    private async Task<string?> GetCorrectLocalPath(
        DriveService service,
        Google.Apis.Drive.v3.Data.File file,
        string relativePath
    )
    {
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
        if (pathParts.Length < 2)
            return null;

        var category = pathParts[0];

        if (file.Name.EndsWith(".yaml") || file.Name.EndsWith(".yml"))
        {
            return Path.GetFullPath(Path.Combine(_contentCachePath, category, file.Name));
        }

        if (
            pathParts.Length > 2
            && pathParts[2].Equals("images", StringComparison.OrdinalIgnoreCase)
        )
        {
            var fileGetRequest = service.Files.Get(file.Id);
            fileGetRequest.Fields = "parents";
            var fileMeta = await fileGetRequest.ExecuteAsync();
            var parentFolderId = fileMeta.Parents?.FirstOrDefault();
            if (string.IsNullOrEmpty(parentFolderId))
                return null;

            var parentGetRequest = service.Files.Get(parentFolderId);
            parentGetRequest.Fields = "parents";
            var parentMeta = await parentGetRequest.ExecuteAsync();
            var itemFolderId = parentMeta.Parents?.FirstOrDefault();
            if (string.IsNullOrEmpty(itemFolderId))
                return null;

            var listRequest = service.Files.List();
            listRequest.Q =
                $"'{itemFolderId}' in parents and (fileExtension = 'yaml' or fileExtension = 'yml') and trashed=false";
            listRequest.Fields = "files(id, name)";
            var yamlFileInDrive = (await listRequest.ExecuteAsync()).Files.FirstOrDefault();

            if (yamlFileInDrive != null)
            {
                var yamlRequest = service.Files.Get(yamlFileInDrive.Id);
                using var tempMemoryStream = new MemoryStream();
                await yamlRequest.DownloadAsync(tempMemoryStream);
                tempMemoryStream.Position = 0;

                using var reader = new StreamReader(tempMemoryStream);
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                var urlKeyHelper = deserializer.Deserialize<UrlKeyHelper>(reader);
                var urlKey = urlKeyHelper?.UrlKey;

                if (!string.IsNullOrEmpty(urlKey))
                {
                    return Path.GetFullPath(
                        Path.Combine(_imagesRootPath, category.ToLower(), urlKey, file.Name)
                    );
                }
            }
        }

        _logger.LogWarning(
            "Could not determine correct local path for file: {FileName}",
            file.Name
        );
        return null;
    }

    private void CleanUpOrphanedFiles(HashSet<string> expectedRemoteFiles)
    {
        _logger.LogInformation("Cleaning up orphaned files...");

        var pathsToClean = new[] { _contentCachePath, _imagesRootPath };
        foreach (var path in pathsToClean)
        {
            if (Directory.Exists(path))
            {
                var localFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var localFile in localFiles)
                {
                    if (!expectedRemoteFiles.Contains(Path.GetFullPath(localFile)))
                    {
                        _logger.LogInformation("Deleting orphaned file: {LocalFile}", localFile);
                        File.Delete(localFile);
                    }
                }
            }
        }
    }

    private async Task DownloadFileIfNeeded(
        DriveService service,
        Google.Apis.Drive.v3.Data.File driveFile,
        string destinationPath
    )
    {
        var shouldDownload = true;
        if (File.Exists(destinationPath))
        {
            var localFileLastModified = File.GetLastWriteTimeUtc(destinationPath);
            var driveFileLastModified = driveFile.ModifiedTimeDateTimeOffset?.UtcDateTime;

            if (
                driveFileLastModified.HasValue
                && localFileLastModified >= driveFileLastModified.Value
            )
            {
                shouldDownload = false;
            }
        }

        if (shouldDownload)
        {
            _logger.LogInformation(
                "Downloading updated file to: {DestinationPath}",
                destinationPath
            );
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (destinationDirectory != null)
                Directory.CreateDirectory(destinationDirectory);

            var request = service.Files.Get(driveFile.Id);
            using var memoryStream = new MemoryStream();
            await request.DownloadAsync(memoryStream);
            await File.WriteAllBytesAsync(destinationPath, memoryStream.ToArray());
        }
    }

    private DriveService? Authenticate()
    {
        var credentialSection = _configuration.GetSection("GoogleApi:Credentials");
        if (!credentialSection.Exists())
        {
            _logger.LogError("GoogleApi:Credentials section in user secrets is missing.");
            return null;
        }

        var clientEmail = credentialSection["client_email"];
        var privateKey = credentialSection["private_key"]?.Replace("\\n", "\n");

        if (string.IsNullOrEmpty(clientEmail) || string.IsNullOrEmpty(privateKey))
        {
            _logger.LogError(
                "GoogleApi:Credentials section in user secrets is incomplete. It must contain 'client_email' and 'private_key'."
            );
            return null;
        }

        var initializer = new ServiceAccountCredential.Initializer(clientEmail)
        {
            Scopes = new[] { DriveService.Scope.DriveReadonly },
        }.FromPrivateKey(privateKey);

        var credential = new ServiceAccountCredential(initializer);

        return new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Roaming Routes Content Fetcher",
            }
        );
    }
}
