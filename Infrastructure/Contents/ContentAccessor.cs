using System.Net;
using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Content;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Contents;

public class ContentAccessor : IContentAccessor
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _client;

    public ContentAccessor(IOptions<DigitalOceanSettings> config)
    {
        _client = new AmazonS3Client(config.Value.DO_SPACES_ACCESS_KEY, config.Value.DO_SPACES_SECRET_KEY,
            new AmazonS3Config
            {
                ServiceURL = config.Value.ServiceUrl
            });

        _bucketName = config.Value.SpacesName;
    }

    public async Task<ContentActionResult> AddContent(IFormFile file)
    {
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = file.OpenReadStream(),
                Key = file.FileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.PublicRead // Make the file public
            };

            var fileTransferUtility = new TransferUtility(_client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            var builder = new UriBuilder(_client.Config.ServiceURL)
            {
                Path = Path.Combine(_bucketName, file.FileName)
            };


            return new ContentActionResult
            {
                Success = true,
                Result = new ContentUploadResult
                {
                    PublicId = file.FileName,
                    Url = builder.Uri.ToString()
                }
            };
        }
        catch (Exception e)
        {
            return new ContentActionResult
            {
                Success = false,
                Message = $"An error occurred when uploading the file: {e.Message}"
            };
        }
    }

    public async Task<ContentActionResult> DeleteContent(string publicId)
    {
        try
        {
            var deleteResponse = await _client.DeleteObjectAsync(_bucketName, publicId);

            if (deleteResponse.HttpStatusCode == HttpStatusCode.NoContent)
                return new ContentActionResult
                {
                    Success = true,
                    Message = "File deleted successfully"
                };
            return new ContentActionResult
            {
                Success = false,
                Message = "File could not be deleted"
            };
        }
        catch (Exception ex)
        {
            return new ContentActionResult
            {
                Success = false,
                Message = $"An error occurred when deleting the file: {ex.Message}"
            };
        }
    }
}