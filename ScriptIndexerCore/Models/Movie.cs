using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScriptIndexerCore.Models
{
    public class Movie
    {
        [BsonId] public ObjectId Id { get; set; }

        [BsonElement] [MaxLength(250)]public string filename { get; set; }

        [BsonElement] public DateTime filedate { get; set; }

        [BsonElement] public string filetype { get; set; }

        [BsonElement] public string filecontents { get; set; }

        [BsonElement] public string filecontentsparsed { get; set; }

        public string IdStripped
        {
            get
            {
                return Id.ToString();
            }
            
        }

    }
}

