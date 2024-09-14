using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Strategy.Models
{
    public class Product
    {
        [BsonId]
        [Key]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        public int Stock { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)] //guidId 32 digit gelir, ObjectId 24 digit kaydeder.
        public string UserId { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
    }
}
