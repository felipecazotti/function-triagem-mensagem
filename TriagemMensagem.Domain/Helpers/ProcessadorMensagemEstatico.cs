using ErrorOr;
using MongoDB.Bson;
using TriagemMensagem.Domain.Enums;

namespace TriagemMensagem.Domain.Helpers;

public static class ProcessadorMensagemEstatico
{
    private const int QUANTIDADE_MINIMA_ARGUMENTOS = 1;
    private const int QUANTIDADE_ARGUMENTOS_RESUMO = 2;
    private const int QUANTIDADE_ARGUMENTOS_FILTRO = 3;
    private const int QUANTIDADE_ARGUMENTOS_EXCLUSAO = 2;
    public static ErrorOr<IdentificadorAcaoEnum> ObterAcao(string[] splitEntrada)
    {
        if(splitEntrada == null || splitEntrada.Length < QUANTIDADE_MINIMA_ARGUMENTOS)
            return Error.Validation("MensagemInvalida.QuantidadeArgumentosInvalida", "Tipo de ação não especificado.");

        var acaoString = splitEntrada[0];

        if (Enum.TryParse(acaoString, out IdentificadorAcaoEnum acaoEnum))
            return acaoEnum;

        return Error.Validation("MensagemInvalida.TipoAcaoInvalido", $"Tipo de ação '{acaoString}' não reconhecido.");
    }

    public static string ObterDescricaoRegistro(string[] splitEntrada)
    {
        var descricao = string.Join(" ", splitEntrada.Skip(1)).Trim();
        return string.IsNullOrWhiteSpace(descricao) ? null : descricao;
    }

    public static ErrorOr<IdentificadorPeriodoResumoEnum> ObterPeriodoResumo(string[] splitEntrada)
    {
        if (splitEntrada.Length != QUANTIDADE_ARGUMENTOS_RESUMO)
            return Error.Validation("MensagemInvalida.QuantidadeArgumentosInvalida", "Quantidade de argumentos inválida para ação Resumo");

        var periodoString = splitEntrada[1];

        if (Enum.TryParse(periodoString, out IdentificadorPeriodoResumoEnum periodoEnum))
            return periodoEnum;

        return Error.Validation("MensagemInvalida.PeriodoResumoInvalido", $"Período de resumo '{periodoString}' não reconhecido.");
    }

    public static ErrorOr<(IdentificadorPeriodoFiltroEnum, int)> ObterPeriodoFiltro(string[] splitEntrada)
    {
        if (splitEntrada.Length != QUANTIDADE_ARGUMENTOS_FILTRO)
            return Error.Validation("MensagemInvalida.QuantidadeArgumentosInvalida", "Quantidade de argumentos inválida para ação Filtro");

        var quantidadeString = splitEntrada[1];
        var periodoString= splitEntrada[2];

        if(!int.TryParse(quantidadeString, out int quantidade) || quantidade <= 0)
            return Error.Validation("MensagemInvalida.NumeroQuantidadeFiltroInvalido", "Quantidade deve ser um número positivo.");

        if (!Enum.TryParse(periodoString, out IdentificadorPeriodoFiltroEnum periodoEnum))
            return Error.Validation("MensagemInvalida.PeriodoFiltroInvalido", $"Período de filtro '{periodoString}' não reconhecido.");
            
        return (periodoEnum, quantidade);
    }

    public static ErrorOr<string> ObterIdExclusao(string[] splitEntrada)
    {
        if (splitEntrada.Length < QUANTIDADE_ARGUMENTOS_EXCLUSAO || !ObjectId.TryParse(splitEntrada[1], out _))
            return Error.Validation("MensagemInvalida.IdExclusaoInvalido", "ID para exclusão não informado ou inválido.");
        return splitEntrada[1];
    }
}
