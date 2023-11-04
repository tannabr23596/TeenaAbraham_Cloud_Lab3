using Amazon.S3.Transfer;
using Amazon.S3;

namespace _301222912_abraham_mehta_Lab3.Services
{
    public class MovieS3Service
    {
        public  async Task<string> saveFiles(IFormFile file)
        {
            return await UploadObjectToS3AndGetUrl(file, "movievideobucket");
        }

        public  async Task<string> UploadObjectToS3AndGetUrl(IFormFile file, string bucketName)
        {
            // Create a TransferUtility instance with the configured S3 client
            var transferUtility = new TransferUtility(Helper.s3Client);

            // Upload the file to S3
            await transferUtility.UploadAsync(file.OpenReadStream(), bucketName, file.FileName);

            // Construct and return the S3 URL
            string s3Url = $"https://{bucketName}.s3.amazonaws.com/{file.FileName}";

            return s3Url;
        }
    }
}
