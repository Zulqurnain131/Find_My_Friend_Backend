using Find_My_Friend_Backend.Dtos;
using Find_My_Friend_Backend.Models;
using System;
using System.Collections.Generic;
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

        //           Saved User Location API ////
        [HttpPut]
        [Route("api/Users/SavedUserLocation")]
        public IHttpActionResult SavedUserLocation(UserLocationDto userLocation)
        {
            try
            {
                string query = "UPDATE [Users] SET Latitude = @p0, Longitude = @p1 WHERE UserId = @p2";
                int rowsAffected = db.Database.ExecuteSqlCommand(query,userLocation.Latitude, userLocation.Longitude, userLocation.UserId);
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


    }
}
