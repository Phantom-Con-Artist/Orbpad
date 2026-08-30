using System.IO;

namespace Orbpad.Services;

public class FileService
{
    public string ReadFile(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    public void WriteFile(string filePath, string content)
    {
        File.WriteAllText(filePath, content);
    }
}