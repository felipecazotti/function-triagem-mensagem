using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.Models;

namespace TriagemMensagem.Domain.IServices;

public interface ITriagemMensagemService
{
    Task SalvarRegistroAsync(string descricao = null);
    Task<List<Registro>> ResumirPeriodoAsync(IdentificadorPeriodoResumoEnum tipoPeriodoResumo);
    Task<List<Registro>> FiltrarPeriodoAsync(IdentificadorPeriodoFiltroEnum tipoPeriodoFiltro, int quantidade);
    Task<bool> ExcluirRegistroAsync(string id);
}