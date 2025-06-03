using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TriagemMensagem.Domain.Models;

public class Registro
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public DateTime DataHoraRegistro { get; set; }
    public string Descricao { get; set; }
}
