using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.IServices;

namespace TriagemMensagem;

public partial class TriagemMensagemTrigger(ITriagemMensagemService triagemMensagemService, ILogger<TriagemMensagemTrigger> logger)
{

    [GeneratedRegex(@"^(Registrar|Resumo|Filtro|Excluir)(?: (Semanal|Mensal|Anual)| -([1-9][0-9]*)(Dia|Semana|Mes)| (id) | ([a-zA-Z0-9 ]+))$")]
    private static partial Regex RegexTriagemMensagem();


    [Function("TriagemMensagemTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var jsonBody = await reader.ReadToEndAsync();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var requestModel = JsonSerializer.Deserialize<RequestModel>(jsonBody, jsonOptions); //TODO: validar formato que o Twilio envia;

        var mensagem = requestModel.Mensagem;

        var match = RegexTriagemMensagem().Match(mensagem);

        if (!match.Success)
        {
            logger.LogWarning("Mensagem inválida: {Mensagem}", mensagem);
            return new BadRequestObjectResult(new { Mensagem = "Mensagem inválida." });
        }

        // match.Groups[1] = (Registrar, Resumo, Filtro, Excluir)
        if (!Enum.TryParse(match.Groups[1].Value, out IdentificadorAcaoEnum tipoAcaoEnum))
        {
            logger.LogWarning("Mensagem inválida: {Mensagem}", mensagem);
            return new BadRequestObjectResult(new { Mensagem = $"Tipo acao {match.Groups[1].Value} nao reconhecido" });
        }

        if (tipoAcaoEnum == IdentificadorAcaoEnum.Registrar)
        {
            var descricao = mensagem.Replace(IdentificadorAcaoEnum.Registrar.ToString(), string.Empty).Trim();
            await triagemMensagemService.SalvarRegistroAsync(string.IsNullOrWhiteSpace(descricao) ? null : descricao);
            return new NoContentResult();
        }

        if (tipoAcaoEnum == IdentificadorAcaoEnum.Resumo)
        {
            // match.Groups[2] = (Semanal, Mensal, Anual)
            if (!Enum.TryParse(match.Groups[2].Value, out IdentificadorPeriodoResumoEnum tipoPeriodoResumo))
            {
                logger.LogWarning("Mensagem inválida: {Mensagem}", mensagem);
                return new BadRequestObjectResult(new { Mensagem = $"Tipo periodo resumo {match.Groups[2].Value} nao reconhecido" });
            }

            var registros = await triagemMensagemService.ResumirPeriodoAsync(tipoPeriodoResumo);
            return new OkObjectResult(registros);
        }

        if (tipoAcaoEnum == IdentificadorAcaoEnum.Filtro)
        {
            // match.Groups[3] = quantidade
            if (!int.TryParse(match.Groups[3].Value, out int quantidade))
            {
                logger.LogWarning("Quantidade inválida: {Quantidade}", match.Groups[3].Value);
                return new BadRequestObjectResult(new { Mensagem = $"Quantidade {match.Groups[3].Value} invalida" });
            }

            // match.Groups[4] = tipo(Dia, Semana, Mes)
            if (!Enum.TryParse(match.Groups[4].Value, true, out IdentificadorPeriodoFiltroEnum tipoPeriodoFiltro))
            {
                logger.LogWarning("Tipo de filtro inválido: {Tipo}", match.Groups[4].Value);
                return new BadRequestObjectResult(new { Mensagem = $"Tipo de periodo filtro {match.Groups[4].Value} nao reconhecido" });
            }

            var registros = await triagemMensagemService.FiltrarPeriodoAsync(tipoPeriodoFiltro, quantidade);
            return new OkObjectResult(registros);
        }

        if (tipoAcaoEnum == IdentificadorAcaoEnum.Excluir)
        {
            // match.Groups[5] = id
            var id = match.Groups[5].Value;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogWarning("Id para exclusão não informado.");
                return new BadRequestObjectResult(new { Mensagem = "Id para exclusão não informado." });
            }

            var resultado = await triagemMensagemService.ExcluirRegistroAsync(id);

            if (!resultado)
            {
                logger.LogWarning("Falha ao excluir registro com id: {Id}", id);
                return new NotFoundObjectResult(new { Mensagem = $"Registro com id {id} nao encontrado." });
            }
            return new NoContentResult();
        }


        logger.LogWarning("Nenhuma condição encontrada para essa mensagem : {Mensagem}", mensagem);
        return new BadRequestObjectResult(new { Mensagem = "Mensagem inválida." });
    }
}


public class RequestModel
{
    public string Mensagem { get; set; }
}