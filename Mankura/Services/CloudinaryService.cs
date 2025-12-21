using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Mankura.Data;


namespace Mankura.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder, string publicId)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = publicId,
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl.ToString();
        }
    }

    public class CloudinaryImportService
    {
        private readonly ReaderRepository _readerRepo;

        public CloudinaryImportService(ReaderRepository readerRepo)
        {
            _readerRepo = readerRepo;
        }

        public void ImportImage(
            int mangaId,
            string publicId,
            string imageUrl
        )
        {
            var name = Path.GetFileNameWithoutExtension(publicId);
            var parts = name.Split('_');

            if (parts.Length < 2)
                return;

            if (!int.TryParse(parts[0], out int chapterNumber))
                return;

            if (!int.TryParse(parts[1], out int pageNumber))
                return;

            var chapterId = _readerRepo.GetOrCreateChapter(mangaId, chapterNumber);

            _readerRepo.AddPage(
                mangaId,
                chapterId,
                pageNumber,
                imageUrl
            );
        }
    }
}