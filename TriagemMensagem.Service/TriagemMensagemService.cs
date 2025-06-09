using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.IRepositories;
using TriagemMensagem.Domain.IServices;
using TriagemMensagem.Domain.Models;

namespace TriagemMensagem.Service;

public class TriagemMensagemService(IRegistroRepository registroRepository) : ITriagemMensagemService
{
    public Task SalvarRegistroAsync(string numeroTelefoneOrigem, string descricao = null)
    {
        var registro = new Registro
        {
            NumeroTelefoneOrigem = numeroTelefoneOrigem,
            Descricao = descricao,
            DataHoraRegistro = DateTime.Now
        };
        return registroRepository.SalvarAsync(registro);
    }

    public Task<List<Registro>> ResumirPeriodoAsync(string numeroTelefoneOrigem, IdentificadorPeriodoResumoEnum tipoPeriodoResumo)
    {
        var dataHora = DateTime.Today;
        if (tipoPeriodoResumo == IdentificadorPeriodoResumoEnum.Semanal)
        {
            var diasDesdeDomingo = (int)dataHora.DayOfWeek;
            var dataDomingoDestaSemana = dataHora.AddDays(-diasDesdeDomingo);
            var dataDomingoSemanaPassada = dataDomingoDestaSemana.AddDays(-7);

            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: dataDomingoSemanaPassada, dataAte: dataDomingoDestaSemana);
        }

        if (tipoPeriodoResumo == IdentificadorPeriodoResumoEnum.Mensal)
        {
            var primeiroDiaDesseMes = new DateTime(dataHora.Year, dataHora.Month, 1);
            int mesPassado = dataHora.Month == 1 ? 12 : dataHora.Month - 1;
            int anoPassado = dataHora.Month == 1 ? dataHora.Year - 1 : dataHora.Year;
            var primeiroDiaMesPassado = new DateTime(anoPassado, mesPassado, 1);

            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: primeiroDiaMesPassado, dataAte: primeiroDiaDesseMes);
        }

        if (tipoPeriodoResumo == IdentificadorPeriodoResumoEnum.Anual)
        {
            var primeiroDiaDesseAno = new DateTime(dataHora.Year, 1, 1);
            var primeiroDiaAnoPassado = new DateTime(dataHora.Year - 1, 1, 1);

            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: primeiroDiaAnoPassado, dataAte: primeiroDiaDesseAno);
        }

        throw new ArgumentException("Tipo de período do resumo inválido.", nameof(tipoPeriodoResumo));
    }

    public Task<List<Registro>> FiltrarPeriodoAsync(string numeroTelefoneOrigem, IdentificadorPeriodoFiltroEnum tipoPeriodoFiltro, int quantidade)
    {
        var dataHora = DateTime.Today;

        if (tipoPeriodoFiltro == IdentificadorPeriodoFiltroEnum.Dia)
        {
            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: dataHora.AddDays(-quantidade));
        }

        if (tipoPeriodoFiltro == IdentificadorPeriodoFiltroEnum.Semana)
        {
            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: dataHora.AddDays(-quantidade * 7));
        }

        if (tipoPeriodoFiltro == IdentificadorPeriodoFiltroEnum.Mes)
        {
            return registroRepository.ListarAsync(numeroTelefoneOrigem, dataDe: dataHora.AddMonths(-quantidade));
        }

        throw new ArgumentException("Tipo de período do filtro inválido.", nameof(tipoPeriodoFiltro));
    }

    public async Task<bool> ExcluirRegistroAsync(string id)
    {
        return await registroRepository.ExcluirAsync(id);
    }
}