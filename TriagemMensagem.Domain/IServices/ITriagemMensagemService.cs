using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.Models;

namespace TriagemMensagem.Domain.IServices;

public interface ITriagemMensagemService
{
    Task SalvarRegistroAsync(string numeroTelefoneOrigem, string descricao = null);
    Task<List<Registro>> ResumirPeriodoAsync(string numeroTelefoneOrigem, IdentificadorPeriodoResumoEnum tipoPeriodoResumo);
    Task<List<Registro>> FiltrarPeriodoAsync(string nuneroTelefoneOrigem, IdentificadorPeriodoFiltroEnum tipoPeriodoFiltro, int quantidade);
    Task<bool> ExcluirRegistroAsync(string id);
}