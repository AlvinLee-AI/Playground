﻿using CommandLine;

namespace S3Playground
{
    public class Options
    {
        [Option("BucketName", Required = true, HelpText = "AWS S3 bucket name")]
        public string BucketName { get; set; }

        [Option("ObjectKey", Required = true, HelpText = "AWS S3 object key name")]
        public string ObjectKey { get; set; }
    }
}
