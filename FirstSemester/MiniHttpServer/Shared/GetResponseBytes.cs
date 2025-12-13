using System;
using System.IO;
using System.Linq;

namespace MiniHttpServer.Shared
{
    public class GetResponseBytes 
    {
        private const string PUBLIC_FOLDER = "Public";
        
        public static byte[]? Invoke(string path) 
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Empty path, serving index.html");
                return TryGetFile("index.html");
            }

            if (Path.HasExtension(path)) 
            {
                Console.WriteLine(path);
                return TryGetFile(path);
            }
    
            string directoryPath = Path.Combine(PUBLIC_FOLDER, path); 
            if (Directory.Exists(directoryPath))
            {
                string indexPath = Path.Combine(path, "index.html"); 
                Console.WriteLine($"Directory {path} found, trying {indexPath}");
                return TryGetFile(indexPath);
            }
    
            Console.WriteLine($"{path} has no extension and is not a directory");
            return TryGetFile(path); 
        }
        
        private static byte[]? TryGetFile(string path)
        {
            try
            {
                string normalizedPath = path.Replace('/', Path.DirectorySeparatorChar) 
                                            .Replace('\\', Path.DirectorySeparatorChar);
                
                normalizedPath = normalizedPath.TrimStart(Path.DirectorySeparatorChar); 

                if (normalizedPath.Contains(".."))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Security: Directory traversal attempt blocked: {path}");
                    Console.ResetColor();
                    return null;
                }

                string directPath = Path.Combine(PUBLIC_FOLDER, normalizedPath);
                if (File.Exists(directPath)) 
                {
                    return File.ReadAllBytes(directPath);
                }
                
                string fileName = Path.GetFileName(normalizedPath);
                string? targetPath = null;

                string[] pathParts = normalizedPath.Split(Path.DirectorySeparatorChar);
                
                if (!Directory.Exists(PUBLIC_FOLDER)) 
                {
                    return null;
                }
                
                var matchingFiles = Directory.EnumerateFiles(
                    PUBLIC_FOLDER, 
                    fileName, 
                    SearchOption.AllDirectories 
                ); 

                foreach (var file in matchingFiles) 
                {
                    if (file.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase)) 
                    {
                        targetPath = file; 
                        break;
                    }
                }

                if (targetPath == null)
                {
                    targetPath = matchingFiles.FirstOrDefault(); 
                }

                if (targetPath == null)
                {
                    return null;
                }

                byte[] fileBytes = File.ReadAllBytes(targetPath);
                
                return fileBytes;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Directory not found: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (FileNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"File not found: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Access denied: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"IO Error reading file: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected error reading file: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }
        
        
    }
}