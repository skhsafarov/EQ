using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EQ_Server.Models
{
    public class Queue
    {
        public Queue(int userId)
        {
            UserId=userId;
        }
        public int? Number { get; set; }
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}