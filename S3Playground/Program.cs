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

      if (_options.LifecycleConfig)
      {
        var bucketLifecycle = await s3Client.GetLifecycleConfigurationAsync(_options.BucketName);
        Console.WriteLine($"Bucket Lifecycle: {_options.BucketName}");
        foreach (var rule in bucketLifecycle.Configuration.Rules)
        {
          Console.WriteLine($"Rule Id: {rule.Id}");
          Console.WriteLine($"Expiration: {rule.Expiration.Days} Days, ExpireDeleteMarkers {rule.Expiration.ExpiredObjectDeleteMarker}");
          Console.WriteLine($"Noncurrent: {rule?.NoncurrentVersionExpiration?.NewerNoncurrentVersions} NewerNonCurrentVersions, {rule?.NoncurrentVersionExpiration?.NoncurrentDays} NonCurrentDays");
        }
      }

      Console.WriteLine($"Getting list of object versions for {_options.ObjectKey}");
      var listResponse = await GetObjectVersions(s3Client);
      foreach(S3ObjectVersion v in listResponse.Versions)
      {
        Console.WriteLine($"Version Key: {v.Key}, VersionId: {v.VersionId}, DeleteMarker: {v.IsDeleteMarker}");
        // Console.WriteLine($"Deleting object {_options.ObjectKey} from s3");
        // var deleteResponse = await DeleteAttachment(s3Client, v.Key);
        //
        // Console.WriteLine($"Deleted: {v.Key}-{deleteResponse.VersionId}-{deleteResponse.DeleteMarker}");
      }
    }

    private static async Task<ListVersionsResponse> GetObjectVersions(IAmazonS3 s3Client)
    {
      return await s3Client.ListVersionsAsync(_options.BucketName, _options.ObjectKey);
    }

    private static async Task<DeleteObjectResponse> DeleteAttachment(IAmazonS3 s3Client,
      string key)
    {
      return await s3Client.DeleteObjectAsync(_options.BucketName, key);
    }
  }
}
