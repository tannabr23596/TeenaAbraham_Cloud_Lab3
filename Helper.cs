using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace _301222912_abraham_mehta_Lab3
{
    public class Helper
    {
        public readonly static AmazonDynamoDBClient dynamoClient;
        public readonly static Amazon.S3.IAmazonS3 s3Client;


        static Helper()
        {
            dynamoClient = GetDynamoClient();
            s3Client = GetS3Client();

        }
        private static AmazonDynamoDBClient GetDynamoClient()
        {
            BasicAWSCredentials credentials =  new BasicAWSCredentials(
                                ConfigurationManager.AppSettings["accessId"],
                                ConfigurationManager.AppSettings["secretKey"]);
            return new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.CACentral1);
        }
        private static IAmazonS3 GetS3Client()
        {
            string awsAccessKey = ConfigurationManager.AppSettings.Get("accesskey");
            string awsSecretKey = ConfigurationManager.AppSettings.Get("secretkey");
            return new Amazon.S3.AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.CACentral1);
        }

    }
}
