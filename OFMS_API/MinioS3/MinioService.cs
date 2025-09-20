using Minio;
using Minio.DataModel.Args;

namespace OFMS_API.MinioS3
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName = "mybucket";

        public MinioService()
        {
            // Assign to the class field, not create a local variable
            _minioClient = new MinioClient()
                .WithEndpoint("http://192.168.28.197:9000")
                .WithCredentials("minioadmin", "minioadmin")
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            bool found = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName)
            );

            if (!found)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucketName)
                );
            }

            var objectName = $"uploads/{Guid.NewGuid()}_{file.FileName}";

            using (var stream = file.OpenReadStream())
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType ?? "application/octet-stream")
                );
            }

            return $"http://localhost:9000/{_bucketName}/{objectName}";
        }
    }
}
