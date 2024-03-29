using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EQ_Server.Models
{
    public class Signin
    {
        public Signin(int userId, string phone, string code)
        {
            UserId=userId;
            Phone=phone;
            Code=code;
        }
        [Required]
        public string Phone { get; set; }
        public string Code { get; set; }
        [DefaultValue(10)]
        public int Attempts { get; set; }
        [DefaultValue("2021-01-01T00:00:00Z")]
        public DateTime Expiration { get; set; }
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

    }
}