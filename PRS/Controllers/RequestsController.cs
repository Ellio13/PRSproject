using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Models;

namespace PRS.Controllers
{
    // Requests controller with address api/requests
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly PRSDBContext _context;

        public RequestsController(PRSDBContext context)
        {
            _context = context;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
        {
            return await _context.Requests.ToListAsync();
        }

        // GET: api/Requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Request>> GetRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            return request;
        }

        [HttpGet("submit-review/{userId}")]
        public async Task<IActionResult> SubmitReview(int userId)
        {
            // Retrieve all requests for the specified user.
            var requests = await _context.Requests
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (requests == null || !requests.Any())
            {
                return NotFound($"No requests found for user with ID {userId}.");
            }

            // Loop through each request and update the status based on its total.
            foreach (var request in requests)
            {
                // Using GetValueOrDefault in case Total is null.
                if (request.Total.GetValueOrDefault() <= 50m)
                {
                    request.Status = "Approved";
                }
                else
                {
                    request.Status = "Review";
                }
                _context.Entry(request).State = EntityState.Modified;
            }

            // Save the updates to the database.
            await _context.SaveChangesAsync();

            // Return the updated requests.
            return Ok(requests);
        }

        [HttpGet("list-review/{id}")]
        public async Task<ActionResult<IEnumerable<Request>>> ListReview(int id)
        {
            // Retrieve all requests with "Review" status, excluding those with the same UserId as the provided id.
            var reviewRequests = await _context.Requests
                .Where(r => r.Status == "Review" && r.UserId != id)
                .ToListAsync();

            if (!reviewRequests.Any())
            {
                return NotFound("No review requests available for this user.");
            }

            return Ok(reviewRequests);
        }

        
        //this approves any request without checking to see if the user is an 
        //admin.  Trying it out for review testing, but prefer following version.
        //Neither one works right now.  

        [HttpPut("approve/{requestId}")]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            // Find the request by ID
            var request = await _context.Requests.FindAsync(requestId);
            if (request == null)
            {
                return NotFound($"Request with ID {requestId} not found.");
            }

            // Update the request status to "Approved"
            request.Status = "Approved";
            _context.Entry(request).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated request
            return Ok(request);
        }


        //This section checks to see if user is an admin before they can approve requests
        //need to check with Admin to see what they want here
        //
        //[HttpPut("approve/{requestId}")]
        //public async Task<IActionResult> ApproveRequest(int requestId, [FromQuery] int userId)
        //{
        //    // Validate the user exists and has admin privileges
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound($"User with ID {userId} not found.");
        //    }

        //    if (!user.Admin)
        //    {
        //        return Unauthorized("Only administrators can approve requests.");
        //    }

        //    // Find the request by ID
        //    var request = await _context.Requests.FindAsync(requestId);
        //    if (request == null)
        //    {
        //        return NotFound($"Request with ID {requestId} not found.");
        //    }

        //    // Update the request status to "Approved"
        //    request.Status = "Approved";
        //    _context.Entry(request).State = EntityState.Modified;

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return Ok(request); // Return the updated request
        //}




        // PUT: api/Requests/{id}
      
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectRequest(int id, [FromQuery] int userId, [FromBody] RejectRequestDTO rejectRequestDto)
        {
            // Validate the user exists and has admin privileges
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            if (!user.Admin)
            {
                return Unauthorized("Only administrators can reject requests.");
            }

            // Validate the reason for rejection
            if (string.IsNullOrWhiteSpace(rejectRequestDto.ReasonForRejection))
            {
                return BadRequest("Reason for rejection is required.");
            }

            // Find the request by ID
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound($"Request with ID {id} not found.");
            }

            // Update the request status to "Rejected" and set the reason for rejection
            request.Status = "Rejected";
            request.ReasonForRejection = rejectRequestDto.ReasonForRejection;
            _context.Entry(request).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(request); // Return the updated request
        }



        // POST: api/Requests
        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(RequestDTO requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request data is required.");
            }

            // Create a new Request entity from the provided RequestDTO
            var request = new Request
            {
                UserId = requestDto.UserId,
                Description = requestDto.Description,
                Justification = requestDto.Justification,
                DateNeeded = requestDto.DateNeeded,
                DeliveryMode = requestDto.DeliveryMode,
                Status = requestDto.Status ?? "NEW", // if nothing here, then defaults to "NEW"
                Total = 0, // Default total (will be updated later)
                SubmittedDate = DateTime.Now, // Default submitted date
                RequestNumber = GetNextRequestNumber() // Helper method to generate request number
            };

            // Add the Request to the database
            _context.Requests.Add(request);
            await _context.SaveChangesAsync(); // Save to generate the RequestId

            // Process LineItems if provided
            if (requestDto.LineItems != null)
            {
                foreach (var lineItemDto in requestDto.LineItems)
                {
                    var product = await _context.Products.FindAsync(lineItemDto.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {lineItemDto.ProductId} does not exist.");
                    }

                    if (product.Price <= 0)
                    {
                        return BadRequest($"Product with ID {lineItemDto.ProductId} has an invalid price.");
                    }

                    var lineItem = new LineItem
                    {
                        ProductId = lineItemDto.ProductId,
                        Quantity = lineItemDto.Quantity,
                        RequestId = request.Id // Use the generated RequestId
                    };

                    _context.LineItems.Add(lineItem);
                }
            }

            // Save all LineItems to the database
            await _context.SaveChangesAsync();

            // Calculate and update the total for the request
            request.Total = CalculateTotal(request.Id); // Use the helper method to calculate the total
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Return the created request
            return CreatedAtAction("GetRequest", new { id = request.Id }, request);
        }



        private string GetNextRequestNumber()
        {
            // Request number format: RYYMMDDXXXX
            string requestNbr = "R" + DateOnly.FromDateTime(DateTime.Now).ToString("yyMMdd");

            // Get the maximum request number that starts with today's date
            string? maxReqNbr = _context.Requests
                .Where(r => r.RequestNumber != null && r.RequestNumber.StartsWith(requestNbr))
                .Max(r => r.RequestNumber);

            string reqNbr;
            if (maxReqNbr != null && maxReqNbr.Length == 11)
            {
                // Extract the last 4 digits, increment, and pad with leading zeros
                string tempNbr = maxReqNbr.Substring(7);
                int nbr = int.Parse(tempNbr) + 1;
                reqNbr = nbr.ToString().PadLeft(4, '0'); // Ensures 4-digit padding
            }
            else
            {
                // Start with "0001" if no existing request numbers
                reqNbr = "0001";
            }

        requestNbr += reqNbr;
            return requestNbr;
        }

        // Calculate total for request - helper method
        private decimal CalculateTotal(int requestId)
        {
            // Calculate the total using a single query
            return _context.LineItems
                .Where(li => li.RequestId == requestId)
                .Join(
                    _context.Products,
                    lineItem => lineItem.ProductId,
                    product => product.Id,
                    (lineItem, product) => product.Price * lineItem.Quantity
                )
                .Sum();
        }



        // DELETE: api/Requests/
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }


        //Post api/requests/login checks username and password
        //[HttpPost("login")]
        //public ActionResult<User> GetPassword([FromBody] UserLoginDTO userlogin)
        //{
        //    // Use FirstOrDefault to retrieve a single user or null
        //    var user = _context.Users.FirstOrDefault(u => u.UserName == userlogin.Username && u.Password == userlogin.Password);

        //    if (user == null)
        //    {
        //        // Return 404 Not Found if no user matches the credentials
        //        return NotFound("Username and password not found");
        //    }

        //    // Return 200 OK with the user if found
        //    return Ok(user);
        //}

    }
}
   
// use post body to get the username and password.  From body only accepts a single object
//define the object (class UserLoginDTO - file DTOlogin) with the properties Username and Password