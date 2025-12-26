using Mankura.Data;

namespace Mankura.Services
{
    public class ChapterImportService
    {
        private readonly ChapterRepository _repo;

        public ChapterImportService(ChapterRepository repo)
        {
            _repo = repo;
        }

        public async Task ImportFromFolder(int mangaId, string folderPath)
        {
            var files = Directory.GetFiles(folderPath)
                .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg"))
                .ToList();

            var groups = files.GroupBy(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                return int.Parse(name.Split('_')[0]);   
            });

            foreach (var group in groups)
            {
                int chapterNumber = group.Key;

                int chapterId = _repo.CreateChapter(mangaId, chapterNumber);

                foreach (var file in group.OrderBy(f => f))
                {
                    var parts = Path.GetFileNameWithoutExtension(file).Split('_');
                    int page = int.Parse(parts[1]);  

                    _repo.AddPage(chapterId, page, file);
                }
            }
        }
    }
}