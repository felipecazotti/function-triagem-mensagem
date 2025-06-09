using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TriagemMensagem.Domain.Enums;
using TriagemMensagem.Domain.Helpers;
using TriagemMensagem.Domain.IServices;
using TriagemMensagem.Domain.Models;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;

namespace TriagemMensagem;

public class TriagemMensagemTrigger(ITriagemMensagemService triagemMensagemService, ILogger<TriagemMensagemTrigger> logger)
{
    [Function("TriagemMensagemTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var data = DateTime.Now;
            logger.LogInformation($"Iniciando processamento na data: {data:o}. Tipo{data.Kind}. Data Local:{data.ToLocalTime():o}. Data Utc: {data.ToUniversalTime():o}");
            var formularioRequest = await req.ReadFormAsync();

            foreach(var key in formularioRequest.Keys)
            {
                logger.LogInformation("Chave: {Key}, Valor: {Value}", key, formularioRequest[key]);
            }

            var mensagem = formularioRequest["body"].ToString();
            logger.LogInformation("Recebendo mensagem: {Mensagem}", mensagem);

            if(mensagem == "Ping")
            {
                logger.LogInformation("Mensagem de ping recebida. Respondendo com pong.");
                return ToTwiML("Pong");
            }

            var entradaSplit = mensagem?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

            var tipoAcao = ProcessadorMensagemEstatico.ObterAcao(entradaSplit);

            if (tipoAcao.IsError)
            {
                logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, tipoAcao.FirstError.Code, tipoAcao.FirstError.Code);
                return ToTwiML(tipoAcao.FirstError.Description);
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Registrar)
            {
                var descricao = ProcessadorMensagemEstatico.ObterDescricaoRegistro(entradaSplit);
                await triagemMensagemService.SalvarRegistroAsync(descricao);
                return ToTwiML("Registro salvo com sucesso.");
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Resumo)
            {
                var periodoResumo = ProcessadorMensagemEstatico.ObterPeriodoResumo(entradaSplit);
                if (periodoResumo.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, periodoResumo.FirstError.Code, periodoResumo.FirstError.Description);
                    return ToTwiML(periodoResumo.FirstError.Description);
                }
                var registros = await triagemMensagemService.ResumirPeriodoAsync(periodoResumo.Value);
                return ToTwiML(registros);
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Filtro)
            {
                var periodoFiltro = ProcessadorMensagemEstatico.ObterPeriodoFiltro(entradaSplit);
                if (periodoFiltro.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, periodoFiltro.FirstError.Code, periodoFiltro.FirstError.Description);
                    return ToTwiML(periodoFiltro.FirstError.Description);
                }
                var registros = await triagemMensagemService.FiltrarPeriodoAsync(periodoFiltro.Value.Item1, periodoFiltro.Value.Item2);
                return ToTwiML(registros);
            }

            if(tipoAcao.Value == IdentificadorAcaoEnum.Excluir)
            {
                var idExclusao = ProcessadorMensagemEstatico.ObterIdExclusao(entradaSplit);
                if(idExclusao.IsError)
                {
                    logger.LogWarning("Mensagem inválida: {Mensagem}. Código: {Codigo}. Descricão: {Descricao}", mensagem, idExclusao.FirstError.Code, idExclusao.FirstError.Description);
                    return ToTwiML(idExclusao.FirstError.Description);
                }
                logger.LogDebug("Antes de excluir registro com id: {Id}", idExclusao.Value);
                var resultado = await triagemMensagemService.ExcluirRegistroAsync(idExclusao.Value);
                logger.LogDebug("Após tentar excluir registro com id: {Id}. Resultado: {Resultado}", idExclusao.Value, resultado);
                if (!resultado)
                {
                    logger.LogWarning("Falha ao excluir registro com id: {Id}", idExclusao.Value);
                    return ToTwiML($"Falha ao excluir registro com id: {idExclusao.Value}.");
                }
                return ToTwiML($"Registro com id: {idExclusao.Value} excluído com sucesso.");
            }

            return ToTwiML("Ação não reconhecida. Por favor, envie uma mensagem válida.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar a mensagem. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            return ToTwiML("Ocorreu um erro ao processar a mensagem");
        }
    }

    private static TwiMLResult ToTwiML(string mensagem)
    {
        var responseTwiMl = new MessagingResponse();
        responseTwiMl.Message(mensagem);
        return responseTwiMl.ToTwiMLResult();
    }

    private static TwiMLResult ToTwiML(List<Registro> registros)
    {
        if (registros == null || registros.Count == 0)
            return ToTwiML("Nenhum registro encontrado.");

        var response = new MessagingResponse();

        foreach (var registro in registros)
        {
            response.Append(new Message($"ID: {registro.Id}\nData: {registro.DataHoraRegistro:dd/MM/yyyy HH:mm:ss}. Tipo: {registro.DataHoraRegistro.Kind}. ToTocal: {registro.DataHoraRegistro.ToLocalTime():o}. Original: {registro.DataHoraRegistro:o}" + (string.IsNullOrWhiteSpace(registro.Descricao) ? "\n" : $"\nDescricao: {registro.Descricao}\n")));
        }   
        return new TwiMLResult(response);
    }



}