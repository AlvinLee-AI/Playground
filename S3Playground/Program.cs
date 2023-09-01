using System;
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

      var listResponse = await GetObjectVersions(s3Client);
      foreach(S3ObjectVersion v in listResponse.Versions)
      {
        Console.WriteLine($"{v.Key}-{v.VersionId}");
      }
    }

    private static async Task<ListVersionsResponse> GetObjectVersions(IAmazonS3 s3Client)
    {
      return await s3Client.ListVersionsAsync(_options.BucketName, _options.ObjectKey);
    }
  }
}
