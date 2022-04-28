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
            Debug.WriteLine("REQUEST BODY HEREEEEEEEEEE: " + myobj.FirstName);



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
                    return BadRequest(ModelState);
                }


                if (user.Amount - myobj.value < 0)
                {
                    return BadRequest(ModelState);
                }

                user.Amount = user.Amount - myobj.value;
                Debug.WriteLine("AMOUNTTTTT!!!!!!!!! " + user.Amount);
                aspNetUser.ApplyTo(user, ModelState); // apply changes to aspNetUser

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _context.SaveChangesAsync(); // save changes to db

                return Ok(user);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

    
    }
}
