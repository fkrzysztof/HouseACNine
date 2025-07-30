using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

public class FileUploadService
{
    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        //return "/uploads/" + uniqueFileName;
        return uniqueFileName;
    }

    public bool DeleteFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // Dodaj folder 'uploads' ręcznie
        var fullPath = Path.Combine(_env.WebRootPath, "uploads", fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }

        return false;
    }

    public async Task<string?> EditFileAsync(IFormFile newFile, string? oldFileName)
    {
        if (newFile == null || newFile.Length == 0)
            return oldFileName; // Zwraca starą nazwę jeśli nowy plik jest pusty

        // Usuń stary plik, jeśli istnieje
        if (!string.IsNullOrWhiteSpace(oldFileName))
        {
            DeleteFile(oldFileName);
        }

        // Zapisz nowy plik
        return await UploadFileAsync(newFile);
    }

}
