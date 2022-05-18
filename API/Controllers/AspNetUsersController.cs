#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.JsonPatch;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AspNetUsersController : ControllerBase
    {
        private readonly CIS4290Context _context;

        public AspNetUsersController(CIS4290Context context)
        {
            _context = context;
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: api/AspNetUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AspNetUser>>> GetAspNetUsers()
        {
            //Debug.WriteLine("CONTEXT HERE!!!!!!!!!!!!!!!!!!!: " + _context.AspNetUsers.FirstOrDefault(user => user.Id == id);

            return await _context.AspNetUsers.ToListAsync();
        }

        // GET: api/AspNetUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AspNetUser>> GetAspNetUser(string id)
        {
            var aspNetUser = await _context.AspNetUsers.FindAsync(id);

            if (aspNetUser == null)
            {
                return NotFound();
            }

            return aspNetUser;
        }

        /*
[
    {
        "value": 30,
        "path": "/Amount",
        "op": "replace",
        "CardNumber": "1111 1111 1111 1111",
        "ExpDate": "11/30",
        "Csv": "111",
        "FirstName": "Anedrew",
        "LastName": "ngngng",
        "UserName": "sdfsdf000@0.com"
    }
]
        */


        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, JsonPatchDocument<AspNetUser> aspNetUser)
        {

            Request.EnableBuffering();

            Request.Body.Position = 0;

            var rawRequestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            Auth myobj = JsonConvert.DeserializeObject<Auth>(rawRequestBody.Substring(1, rawRequestBody.Length - 2));
            Debug.WriteLine("REQUEST BODY HEREEEEEEEEEE: " + id);



            if (aspNetUser != null)
            {
                var user = _context.AspNetUsers.FirstOrDefault(
                    user => user.Id == id 
                    && 
                    user.UserName == myobj.UserName //loop and find user with matching id
                    &&
                    user.FirstName == myobj.FirstName
                    &&
                    user.LastName == myobj.LastName
                    &&
                    user.CardNumber == myobj.CardNumber
                    &&
                    user.Csv == myobj.Csv
                    &&
                    user.ExpDate == myobj.ExpDate
                );

                if (user == null)
                {
                    return StatusCode(404);
                }


                if (user.Amount - myobj.value < 0)
                {
                    return Ok("Insufficient funds. Card on hold");
                }

                myobj.value = (float)(user.Amount - myobj.value);
                var json = JsonConvert.SerializeObject(myobj);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
                var stream = await requestContent.ReadAsStreamAsync();
                Request.Body = stream;

                Debug.WriteLine("AMOUNTTTTT!!!!!!!!! " + myobj.value);
                aspNetUser.ApplyTo(user, ModelState); // apply changes to aspNetUser

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _context.SaveChangesAsync(); // save changes to db

                Response response = new Response();
                response.message = "Success! New balance is updated";
                response.authToken = RandomString(40);
                string jsonRes = JsonConvert.SerializeObject(response);
                Update(myobj.value, myobj, id);
                return Ok(jsonRes);
            }
            else
            {
                return StatusCode(404); 
            }
        }

























        [HttpPost]
        public string Update(float value, Auth myobj, string id)
        {
            var user = _context.AspNetUsers.FirstOrDefault(
                    user => user.Id == id
                    &&
                    user.UserName == myobj.UserName //loop and find user with matching id
                    &&
                    user.FirstName == myobj.FirstName
                    &&
                    user.LastName == myobj.LastName
                    &&
                    user.CardNumber == myobj.CardNumber
                    &&
                    user.Csv == myobj.Csv
                    &&
                    user.ExpDate == myobj.ExpDate
            );
            user.Amount = value;
            Debug.WriteLine("sdfjkfhasdkljfhasldkjfhalskdjfhasdS");
            _context.SaveChangesAsync();
            return "";
        }

    
    }
}
