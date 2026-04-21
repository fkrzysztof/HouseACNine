using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Data.HouseRentalData
{
    [Index(nameof(Token), IsUnique = true)]
    public class CommentAccessToken
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } 
    }
}
