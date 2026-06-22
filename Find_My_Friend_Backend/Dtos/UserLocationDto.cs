using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Find_My_Friend_Backend.Dtos
{
    public class UserLocationDto
    {
        public int UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

      
        public DateTime RecordedAt { get; set; }
    }
}