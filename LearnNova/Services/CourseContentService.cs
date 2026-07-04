using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using LearnNova.Models.Enums;

namespace LearnNova.Services;

public class CourseContentService : ICourseContentService
{
    private readonly ICourseContentRepository _contentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IWebHostEnvironment _env;

    public CourseContentService(ICourseContentRepository contentRepository, ICourseRepository courseRepository, IWebHostEnvironment env)
    {
        _contentRepository = contentRepository;
        _courseRepository = courseRepository;
        _env = env;
    }

    private async Task<bool> IsCourseOwnerAsync(int courseId, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        return course?.TeacherId == teacherId;
    }

    public async Task<IEnumerable<CourseContent>> GetContentsByCourseAsync(int courseId, string teacherId)
    {
        if (!await IsCourseOwnerAsync(courseId, teacherId))
            return Enumerable.Empty<CourseContent>();

        return await _contentRepository.GetByCourseIdAsync(courseId);
    }

    public async Task<CourseContent?> GetContentByIdAsync(int id, string teacherId)
    {
        var content = await _contentRepository.GetByIdAsync(id);
        if (content == null) return null;

        if (!await IsCourseOwnerAsync(content.CourseId, teacherId))
            return null;

        return content;
    }

    private async Task<(bool Succeeded, string Url, ContentType Type, string Error)> ProcessFileUploadAsync(CourseContentFormViewModel model)
    {
        if (model.SourceType == "UPLOAD")
        {
            if (model.UploadFile == null || model.UploadFile.Length == 0)
                return (false, "", ContentType.Document, "يرجى اختيار ملف لرفعه.");

            // Validate size (max 50MB)
            if (model.UploadFile.Length > 50 * 1024 * 1024)
                return (false, "", ContentType.Document, "حجم الملف يتجاوز الحد المسموح به (50 ميجابايت).");

            // Validate extension
            var ext = Path.GetExtension(model.UploadFile.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".rar", ".jpg", ".png", ".jpeg", ".webp", ".mp4", ".txt" };
            if (!allowedExts.Contains(ext))
                return (false, "", ContentType.Document, "نوع الملف غير مدعوم.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "course_content");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.UploadFile.CopyToAsync(stream);
            }

            var type = DetectTypeFromExtension(ext);
            return (true, $"/uploads/course_content/{uniqueFileName}", type, "");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.FileUrl))
                return (false, "", ContentType.Document, "رابط المحتوى مطلوب.");
            
            // Format YouTube URL
            string finalUrl = model.FileUrl.Trim();
            if (finalUrl.Contains("youtube.com") || finalUrl.Contains("youtu.be"))
            {
                string videoId = "";
                if (finalUrl.Contains("watch?v="))
                    videoId = finalUrl.Split("watch?v=")[1].Split('&')[0];
                else if (finalUrl.Contains("youtu.be/"))
                    videoId = finalUrl.Split("youtu.be/")[1].Split('?')[0];
                else if (finalUrl.Contains("embed/"))
                    videoId = finalUrl.Split("embed/")[1].Split('?')[0];

                if (!string.IsNullOrEmpty(videoId))
                    finalUrl = $"https://www.youtube.com/embed/{videoId}";
                
                return (true, finalUrl, ContentType.Video, "");
            }
            else if (finalUrl.Contains("drive.google.com") || finalUrl.Contains("onedrive") || finalUrl.Contains("dropbox.com"))
            {
                return (true, finalUrl, ContentType.ExternalLink, "");
            }

            var ext = Path.GetExtension(finalUrl).ToLowerInvariant();
            var type = DetectTypeFromExtension(ext);
            
            if (type == ContentType.Document) // fallback
            {
                if (finalUrl.StartsWith("http")) type = ContentType.ExternalLink;
            }

            return (true, finalUrl, type, "");
        }
    }

    private ContentType DetectTypeFromExtension(string ext)
    {
        if (ext == ".pdf") return ContentType.Pdf;
        if (ext == ".doc" || ext == ".docx") return ContentType.Word;
        if (ext == ".ppt" || ext == ".pptx") return ContentType.PowerPoint;
        if (ext == ".xls" || ext == ".xlsx") return ContentType.Excel;
        if (ext == ".zip" || ext == ".rar") return ContentType.Archive;
        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp") return ContentType.Image;
        if (ext == ".mp4") return ContentType.Video;
        return ContentType.OtherDocument;
    }

    public async Task<ServiceResult> AddContentAsync(int courseId, CourseContentFormViewModel model, string teacherId)
    {
        if (!await IsCourseOwnerAsync(courseId, teacherId))
            return ServiceResult.Fail("غير مصرح لك بإضافة محتوى لهذا الكورس.");

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان المحتوى مطلوب.");

        var uploadResult = await ProcessFileUploadAsync(model);
        if (!uploadResult.Succeeded)
            return ServiceResult.Fail(uploadResult.Error);

        var nextOrder = await _contentRepository.GetMaxOrderIndexAsync(courseId) + 1;

        var content = new CourseContent
        {
            CourseId = courseId,
            Title = model.Title.Trim(),
            Type = uploadResult.Type,
            FileUrl = uploadResult.Url,
            DurationSec = model.DurationSec,
            OrderIndex = nextOrder
        };

        await _contentRepository.AddAsync(content);
        await _contentRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateContentAsync(int id, CourseContentFormViewModel model, string teacherId)
    {
        var content = await _contentRepository.GetByIdAsync(id);
        if (content == null) return ServiceResult.Fail("المحتوى غير موجود.");

        if (!await IsCourseOwnerAsync(content.CourseId, teacherId))
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا المحتوى.");

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان المحتوى مطلوب.");

        string finalUrl = content.FileUrl;
        ContentType? newType = null;
        
        // If teacher changed the source, or if it's UPLOAD and they uploaded a new file
        if (model.SourceType == "UPLOAD" && model.UploadFile != null)
        {
            var uploadResult = await ProcessFileUploadAsync(model);
            if (!uploadResult.Succeeded)
                return ServiceResult.Fail(uploadResult.Error);
            finalUrl = uploadResult.Url;
            newType = uploadResult.Type;
        }
        else if (model.SourceType == "URL")
        {
            var uploadResult = await ProcessFileUploadAsync(model);
            if (!uploadResult.Succeeded)
                return ServiceResult.Fail(uploadResult.Error);
            finalUrl = uploadResult.Url;
            newType = uploadResult.Type;
        }

        content.Title = model.Title.Trim();
        if (newType.HasValue)
        {
            content.Type = newType.Value;
        }
        content.FileUrl = finalUrl;
        content.DurationSec = model.DurationSec;

        _contentRepository.Update(content);
        await _contentRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteContentAsync(int id, string teacherId)
    {
        var content = await _contentRepository.GetByIdAsync(id);
        if (content == null) return ServiceResult.Fail("المحتوى غير موجود.");

        if (!await IsCourseOwnerAsync(content.CourseId, teacherId))
            return ServiceResult.Fail("غير مصرح لك بحذف هذا المحتوى.");

        _contentRepository.Remove(content);
        await _contentRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ReorderContentAsync(int courseId, List<int> orderedContentIds, string teacherId)
    {
        if (!await IsCourseOwnerAsync(courseId, teacherId))
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا الكورس.");

        var contents = (await _contentRepository.GetByCourseIdAsync(courseId)).ToList();

        for (int i = 0; i < orderedContentIds.Count; i++)
        {
            var content = contents.FirstOrDefault(c => c.Id == orderedContentIds[i]);
            if (content != null)
            {
                content.OrderIndex = i;
                _contentRepository.Update(content);
            }
        }

        await _contentRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }
}
