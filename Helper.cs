using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace _301222912_abraham_mehta_Lab3
{
    public class Helper
    {
        public readonly static AmazonDynamoDBClient client;

        static Helper()
        {
            client = GetDynamoClient();
        }
        private static AmazonDynamoDBClient GetDynamoClient()
        {
            BasicAWSCredentials credentials =  new BasicAWSCredentials(
                                ConfigurationManager.AppSettings["accessId"],
                                ConfigurationManager.AppSettings["secretKey"]);
            return new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.CACentral1);
        }


    }
}
