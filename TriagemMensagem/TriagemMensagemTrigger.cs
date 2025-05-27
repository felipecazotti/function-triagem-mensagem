using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.Helpers;
using TriagemMensagem.Domain.IServices;

namespace TriagemMensagem;

public class TriagemMensagemTrigger(ITriagemMensagemService triagemMensagemService, ILogger<TriagemMensagemTrigger> logger)
{
    [Function("TriagemMensagemTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {
            using var reader = new StreamReader(req.Body);
            var jsonBody = await reader.ReadToEndAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var requestModel = JsonSerializer.Deserialize<RequestModel>(jsonBody, jsonOptions); //TODO: validar formato que o Twilio envia;

            var mensagem = requestModel.Mensagem;

            var entradaSplit = mensagem?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

            var tipoAcao = ProcessadorMensagemEstatico.ObterAcao(entradaSplit);

            if (tipoAcao.IsError)
            {
                logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, tipoAcao.FirstError.Code, tipoAcao.FirstError.Code);
                return new BadRequestObjectResult(new { Mensagem = tipoAcao.FirstError.Description });
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Registrar)
            {
                var descricao = ProcessadorMensagemEstatico.ObterDescricaoRegistro(entradaSplit);
                await triagemMensagemService.SalvarRegistroAsync(descricao);
                return new NoContentResult();
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Resumo)
            {
                var periodoResumo = ProcessadorMensagemEstatico.ObterPeriodoResumo(entradaSplit);
                if (periodoResumo.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, periodoResumo.FirstError.Code, periodoResumo.FirstError.Description);
                    return new BadRequestObjectResult(new { Mensagem = periodoResumo.FirstError.Description });
                }
                var registros = await triagemMensagemService.ResumirPeriodoAsync(periodoResumo.Value);
                return new OkObjectResult(registros);
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Filtro)
            {
                var periodoFiltro = ProcessadorMensagemEstatico.ObterPeriodoFiltro(entradaSplit);
                if (periodoFiltro.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, periodoFiltro.FirstError.Code, periodoFiltro.FirstError.Description);
                    return new BadRequestObjectResult(new { Mensagem = periodoFiltro.FirstError.Description });
                }
                var registros = await triagemMensagemService.FiltrarPeriodoAsync(periodoFiltro.Value.Item1, periodoFiltro.Value.Item2);
                return new OkObjectResult(registros);
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Excluir)
            {
                var idExclusao = ProcessadorMensagemEstatico.ObterIdExclusao(entradaSplit);
                if(idExclusao.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, idExclusao.FirstError.Code, idExclusao.FirstError.Description);
                    return new BadRequestObjectResult(new { Mensagem = idExclusao.FirstError.Description });
                }
                var resultado = await triagemMensagemService.ExcluirRegistroAsync(idExclusao.Value);
                if (!resultado)
                {
                    logger.LogWarning("Falha ao excluir registro com id: {Id}", idExclusao.Value);
                    return new NotFoundObjectResult(new { Mensagem = $"Registro com id {idExclusao} não encontrado." });
                }
                return new NoContentResult();
            }

            return new BadRequestObjectResult(new { Mensagem = "Mensagem Inválida"});
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar a mensagem.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}


public class RequestModel
{
    public string Mensagem { get; set; }
}