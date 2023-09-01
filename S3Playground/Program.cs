using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CommandLine;

namespace S3Playground
{
  class Program
  {
    private static Options _options;

    static async Task Main(string[] args)
    {
      // Before running this app:
      // - Credentials must be specified in an AWS profile. If you use a profile other than
      //   the [default] profile, also set the AWS_PROFILE environment variable.
      // - An AWS Region must be specified either in the [default] profile
      //   or by setting the AWS_REGION environment variable.
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed(options => _options = options)
        .WithNotParsed(err => throw new ArgumentException($"Could not parse arguments, {args}"));

      var s3Client = new AmazonS3Client();

      Console.WriteLine($"Getting list of object versions for {_options.ObjectKey}");
      var listResponse = await GetObjectVersions(s3Client);
      foreach(S3ObjectVersion v in listResponse.Versions)
      {
        Console.WriteLine($"{v.Key}-{v.VersionId}");
      }

      Console.WriteLine($"Deleting object {_options.ObjectKey} from s3");
      var deleteResponse = await DeleteAttachment(s3Client, listResponse);
      foreach (DeletedObject deletedObject in deleteResponse.DeletedObjects)
      {
        Console.WriteLine($"Deleted: {deletedObject.Key}-{deletedObject.VersionId}-{deletedObject.DeleteMarker}");
      }

      if (deleteResponse.DeleteErrors.Any())
      {
        foreach (var error in deleteResponse.DeleteErrors)
        {
          Console.WriteLine($"Error deleting {error.Key}-{error.VersionId}: {error.Message}");
        }
      }
    }

    private static async Task<ListVersionsResponse> GetObjectVersions(IAmazonS3 s3Client)
    {
      return await s3Client.ListVersionsAsync(_options.BucketName, _options.ObjectKey);
    }

    private static async Task<DeleteObjectsResponse> DeleteAttachment(IAmazonS3 s3Client,
      ListVersionsResponse versionList)
    {
      var deleteRequest = new DeleteObjectsRequest
      {
        BucketName = _options.BucketName,
        Objects = versionList.Versions.Select(v => new KeyVersion { Key = v.Key, VersionId = v.VersionId }).ToList()
      };

      return await s3Client.DeleteObjectsAsync(deleteRequest);
    }
  }
}
