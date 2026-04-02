using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Model.DTO.Settings
{
    public class WorkingDirectoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BasePath { get; set; }
        public string ArchivePath { get; set; }
        public string NotImportedPath { get; set; }
        public List<SubPathElementDto> SubPathElements { get; set; } = new();


        public string GenerateFullPathPreview(bool fillWithExampleData, List<PathPlaceholderDto>? placeholders)
        {
            if (fillWithExampleData == true && placeholders == null) return string.Empty;


            string basePath = this.BasePath ?? string.Empty;
            basePath = basePath.TrimEnd(Path.DirectorySeparatorChar);
            basePath = basePath.TrimEnd(Path.AltDirectorySeparatorChar);

            var pathBuilder = new StringBuilder();
            pathBuilder.Append(basePath).Append(Path.DirectorySeparatorChar);

            if (this.SubPathElements != null)
            {
                foreach (var element in this.SubPathElements.OrderBy(e => e.Order))
                {
                    string segmentToAppend = element.PathSegment;
                    if (fillWithExampleData)
                    {
                        var placeholder = placeholders.FirstOrDefault(p => p.UniqueName == element.PathSegment);
                        if (placeholder != null)
                        {
                            segmentToAppend = placeholder.ExampleValue;
                        }
                        // If it's a placeholder not in DB or not a placeholder, PathSegment is used as is.
                    }
                    pathBuilder.Append(segmentToAppend);
                }
            }

            string tempPreview = pathBuilder.ToString();

            char correctSeparator = Path.DirectorySeparatorChar;
            char incorrectSeparator = correctSeparator == '/' ? '\\' : '/';
            tempPreview = tempPreview.Replace(incorrectSeparator, correctSeparator);

            string doubleSeparatorPattern = Regex.Escape(correctSeparator.ToString()) + "{2,}";
            string finalPreview = Regex.Replace(tempPreview, doubleSeparatorPattern, correctSeparator.ToString());

            return finalPreview;
        }
    }
}
