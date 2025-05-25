using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using TriagemMensagem.Domain.IRepositories;
using TriagemMensagem.Domain.Models;

namespace TriagemMensagem.Repository;

public class RegistroRepository(IMongoDatabase database, IConfiguration configuration) : IRegistroRepository
{
    private readonly IMongoCollection<Registro> registroCollection = database.GetCollection<Registro>(configuration["MongoDbConfiguration:RegistroCollectionName"]);

    public Task SalvarAsync(Registro registro)
    {
        return registroCollection.InsertOneAsync(registro);
    }

    public async Task<bool> ExcluirAsync(string id)
    {
        var resultado = await registroCollection.DeleteOneAsync(r => r.Id == id);
        return resultado.DeletedCount > 0;
    }

    public Task<List<Registro>> ListarAsync(DateTime dataDe, DateTime? dataAte = null)
    {
        var filtroBuilder = Builders<Registro>.Filter;
        var filtro = filtroBuilder.Gte(r => r.DataHoraRegistro, dataDe);

        if (dataAte.HasValue)
            filtro &= filtroBuilder.Lt(r => r.DataHoraRegistro, dataAte.Value);

        return registroCollection.Find(filtro).ToListAsync();
    }
}
