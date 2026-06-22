using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Find_My_Friend_Backend.Dtos
{
    public class RequestStatusDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; }
    }
}