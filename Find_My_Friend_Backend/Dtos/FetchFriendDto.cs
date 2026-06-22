using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Find_My_Friend_Backend.Dtos
{
    public class FetchFriendDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNo { get; set; }
    }
}