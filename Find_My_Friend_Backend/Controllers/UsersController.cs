using Find_My_Friend_Backend.Dtos;
using Find_My_Friend_Backend.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;
using System.Web.UI.WebControls;

namespace Find_My_Friend_Backend.Controllers
{
    public class UsersController : ApiController
    {
        FindMyFriendsEntities db = new FindMyFriendsEntities();
        [HttpPost]
        [Route("api/Users/Signup")]
        public IHttpActionResult Signup(User user)
        {
            try
            {
             string checkQuery = "SELECT COUNT(*) FROM [Users] WHERE Email = @p0";
             int exists = db.Database.SqlQuery<int>(checkQuery, user.Email).FirstOrDefault();

             if (exists > 0)
             {
                    return Content(HttpStatusCode.Conflict, new
                    {
                        status = HttpStatusCode.Conflict,
                        message = "Email already exists"
                    });
                }
            string query = @"
            INSERT INTO [Users]
            (FullName, Email, Password, PhoneNo)
            VALUES
            (@p0, @p1, @p2, @p3)";

            int rowAffected = db.Database.ExecuteSqlCommand(query, user.FullName, user.Email, user.Password, user.PhoneNo);
            if(rowAffected > 0)
                {
                    return Content(HttpStatusCode.OK, new
                    {
                        status = HttpStatusCode.OK,
                        message = "User registered successfully"
                    });
                       
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = HttpStatusCode.BadRequest,
                        message = "Failed to register user"
                    });
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = HttpStatusCode.InternalServerError,
                    message = "An error occurred while signing up",
                    error = ex.Message
                });
            }


        }
        ///                             Login API     ///////////////
        [HttpPost]
        [Route("api/Users/Login")]
        public IHttpActionResult Login(LoginDto model)
        {
            try
            {
                string query = "SELECT UserId FROM [Users] WHERE Email = @p0 AND Password = @p1";
                int userid = db.Database.SqlQuery<int>(query, model.Email, model.Password).FirstOrDefault();

                if (userid>0)
                {
               
                    return Ok(new
                    {
                        UserId = userid,
                        status = HttpStatusCode.OK,
                        message = "Login successful"
                    });
                }
                else
                {
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        status = HttpStatusCode.Unauthorized,
                        message = "Invalid email or password"
                    });
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = HttpStatusCode.InternalServerError,
                    message = "An error occurred during login",
                    error = ex.Message
                });
            }
        }

        /// Maintain Users  Location  History API ////
        [HttpPost]
        [Route("api/Users/UserLocationHistory")]
        public IHttpActionResult UserLocationHistory(UserLocationDto userLocation)
        {
            try
            {
                // current location update
                string historyQuery = @"INSERT INTO [LocationHistory] (UserId, Latitude, Longitude) VALUES (@p0, @p1, @p2)";
                int rowsAffected = db.Database.ExecuteSqlCommand(historyQuery,userLocation.UserId, userLocation.Latitude, userLocation.Longitude);

                if (rowsAffected > 0)
                {

                    return Ok(new
                    {
                        status = HttpStatusCode.OK,
                        message = "User location updated successfully"
                    });
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = HttpStatusCode.BadRequest,
                        message = "Failed to update user location"
                    });
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = HttpStatusCode.InternalServerError,
                    message = "An error occurred while updating user location",
                    error = ex.Message
                });
            }
        }


        ////////    Matched Conatct Api //////////
        [HttpPost]
        [Route("api/Users/MatchContacts")]
        public IHttpActionResult MatchContacts(ContactSyncDto model)
        {
            try
            {
                if (model == null || model.Contacts == null)
                {
                    return BadRequest("No contacts provided");
                }


                // .Select(x => $"'{x}'") Har number ke around single quotes add karti hai
                //  string.Join  Har conatctnumber   ko comma se join kar deti hai

                string phones = string.Join(",", model.Contacts.Select(x => $"'{x}'"));

                //IN operator use hota hai jab  ek column ko multiple values se compare karna ho.
                string query = $@"
                SELECT UserId, FullName, PhoneNo FROM [Users] WHERE PhoneNo IN ({phones})";

                var matchedUsers = db.Database.SqlQuery<UserDto>(query).ToList();


                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    message = "Matched contacts found",
                    data = matchedUsers
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = HttpStatusCode.InternalServerError,
                    message = "Error occurred",
                    error = ex.Message
                });
            }
        }

        ////             Send Location Request API //////////
        [HttpPost]
        [Route("api/Users/SendRequest")]
        public IHttpActionResult SendRequest(LocationRequestDto model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Invalid data");
                }

                string query = @"
            INSERT INTO [LocationRequests] (SenderId, ReceiverId,Status)
            VALUES (@p0, @p1,'Pending')";

                db.Database.ExecuteSqlCommand(query, model.SenderId, model.ReceiverId);

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    message = "Friend request sent successfully"
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = HttpStatusCode.InternalServerError,
                    message = "Error occurred",
                    error = ex.Message
                });
            }
        }

        // Show location Request API //
        [HttpGet]
        [Route("api/Users/GetLocationRequests")]
        public IHttpActionResult GetLocationRequests(int receiverId)
        {
            try
            {
                string query = @"
       SELECT
            LR.RequestId,
            U.UserId,
            U.FullName,
            U.PhoneNo,
            LR.Status
        FROM LocationRequests LR
         JOIN Users U
            ON LR.SenderId = U.UserId
        WHERE LR.ReceiverId = @p0";

                var requests = db.Database.SqlQuery<LocationRequestNotificationDto>(
                    query,
                    receiverId
                ).ToList();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }


        // Accept Reject  location Request API //
        [HttpPost]
        [Route("api/Users/UpdateLocationRequestStatus")]
        public IHttpActionResult UpdateLocationRequestStatus(RequestStatusDto model)
        {
            try
            {
                string query = @"
        UPDATE  [LocationRequests]
        SET Status = @p0
        WHERE RequestId = @p1";

                int rows = db.Database.ExecuteSqlCommand(
                    query,
                    model.Status,
                    model.RequestId
                );

                if (rows == 0)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    status = HttpStatusCode.OK,
                    Message = "Request Accepted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // location Share with Friends API //

        [HttpGet]
        [Route("api/Users/GetSharedLocationHistory")]
        public IHttpActionResult GetSharedLocationHistory(int senderId)
        {
            try
            {
                // Sender ke sab accepted receivers
                string requestQuery = @"
SELECT
    LR.ReceiverId,
    U.FullName
FROM LocationRequests LR
INNER JOIN Users U
    ON LR.ReceiverId = U.UserId
WHERE LR.SenderId = @p0
AND LR.Status = 'Accepted'";

                var users = db.Database.SqlQuery<LocationRequestDto>(
                    requestQuery,
                    senderId
                ).ToList();

                if (!users.Any())
                {
                    return BadRequest("No accepted requests found.");
                }

                var result = new List<object>();

                foreach (var user in users)
                {
                    string locationQuery = @"
SELECT
    UserId,
    Latitude,
    Longitude,
    RecordedAt
FROM LocationHistory
WHERE UserId = @p0
ORDER BY RecordedAt DESC";

                    var locations = db.Database.SqlQuery<UserLocationDto>(
                        locationQuery,
                        user.ReceiverId
                    ).ToList();

                    result.Add(new
                    {
                    
                        FullName = user.FullName,
                        Locations = locations
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        ////          Fetch Friends ////
        [HttpGet]
        [Route("api/Users/FetchFriends")]
        public IHttpActionResult FetchFriends()
        {
            try
            {
                string query = @"SELECT UserId,FullName,PhoneNo FROM [Users] ";

                var requests = db.Database.SqlQuery<FetchFriendDto>(query).ToList();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }
        ///Get Sent Requests API////

        [HttpGet]
        [Route("api/Users/GetSentFriendsRequests")]
        public IHttpActionResult GetSentFriendsRequests(int userid)
        {
            try
            {
                string query = @"
      SELECT
    U.UserId,
    U.FullName,
    U.PhoneNo,
    LR.Status,
    LR.RequestId

FROM LocationRequests LR
 JOIN Users U
ON LR.ReceiverId = U.UserId
WHERE LR.SenderId = @p0";

                var requests = db.Database.SqlQuery<LocationRequestNotificationDto>(
                    query,
                    userid
                ).ToList();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }


        ////// Get Received Requests API////
        [HttpGet]
        [Route("api/Users/GetReceivedFriendsRequests")]
        public IHttpActionResult GetReceivedRequests(int userid)
        {
            try
            {
                string query = @"
      SELECT
    U.UserId,
    U.FullName,
    U.PhoneNo,
    LR.Status,
    LR.RequestId

FROM LocationRequests LR
 JOIN Users U
ON LR.SenderId = U.UserId
WHERE LR.ReceiverId = @p0";

                var requests = db.Database.SqlQuery<LocationRequestNotificationDto>(
                    query,
                    userid
                ).ToList();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

    }
}
