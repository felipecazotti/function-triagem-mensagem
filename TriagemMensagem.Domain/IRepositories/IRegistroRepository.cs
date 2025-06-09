using TriagemMensagem.Domain.Models;

namespace TriagemMensagem.Domain.IRepositories;
public interface IRegistroRepository
{
    Task SalvarAsync(Registro registro);
    Task<bool> ExcluirAsync(string id);
    Task<List<Registro>> ListarAsync(string numeroTelefoneOrigem, DateTime dataDe, DateTime? dataAte = null);
}
