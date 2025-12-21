using System.Collections.Generic;

namespace Mankura.Models.ViewModels
{
    public class ChapterReadViewModel
    {
        public int ChapterId { get; set; }

        public List<ChapterPage> Pages { get; set; } = new();

        public int? PrevChapterId { get; set; }
        public int? NextChapterId { get; set; }
    }
}