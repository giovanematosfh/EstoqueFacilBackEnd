namespace EstoqueFacil.Application.Interfaces.Services
{
    // Armazena arquivos enviados (ex.: fotos de produtos) e devolve o nome gerado.
    public interface IFileStorage
    {
        Task<string> SaveAsync(string folder, Stream content, string extension);
        void Delete(string folder, string fileName);
    }
}
