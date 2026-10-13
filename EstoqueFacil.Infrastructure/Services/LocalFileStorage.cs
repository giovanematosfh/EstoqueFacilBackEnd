using EstoqueFacil.Application.Interfaces.Services;

namespace EstoqueFacil.Infrastructure.Services
{
    // Grava em disco, em {RootPath}/{folder}/{guid}{ext}. Nomes aleatórios evitam colisão e adivinhação.
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _rootPath;

        public LocalFileStorage(string rootPath)
        {
            _rootPath = rootPath;
        }

        public async Task<string> SaveAsync(string folder, Stream content, string extension)
        {
            var directory = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            await using var file = File.Create(Path.Combine(directory, fileName));
            await content.CopyToAsync(file);

            return fileName;
        }

        public void Delete(string folder, string fileName)
        {
            var path = Path.Combine(_rootPath, folder, Path.GetFileName(fileName));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
