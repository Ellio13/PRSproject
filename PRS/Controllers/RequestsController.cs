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

        // GET: api/Requests  generic get
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
        {
            return await _context.Requests.ToListAsync();
        }

        // GET: api/Requests/{id}  get requests by id
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


        //submit requests for review by requestID
        [HttpPut("submit-review/{requestId}")] 
        public async Task<IActionResult> SubmitReview(int requestId)
        {
            // Retrieve all requests for the specified user.
            var requests = await _context.Requests
                .Where(r => r.Id == requestId)
                .ToListAsync();

            if (requests == null || !requests.Any())
            {
                return NotFound($"No requests found for user with ID {requestId}.");
            }

            // Loop through each request and update the status based on its total.
            foreach (var request in requests)
            {
                // Ensure Total is calculated if it's null
                if (request.Total == null)
                {
                    request.Total = CalculateTotal(request.Id);
                }

                // Set status based on Total
                if (request.Total.GetValueOrDefault() <= 50m)
                {
                    request.Status = "APPROVED";
                }
                else
                {
                    request.Status = "REVIEW";
                }

                // Update the submitted date to today's date
                request.SubmittedDate = DateTime.Now;
                _context.Entry(request).State = EntityState.Modified;
            }

            // Save the updates to the database.
            await _context.SaveChangesAsync();

            // Return the updated requests.
            return Ok(requests);
        }


        //get a list of requests in review status
        [HttpGet("list-review/{id}")]
        public async Task<ActionResult<IEnumerable<Request>>> ListReview(int id)
        {
            // Retrieve all requests with "Review" status, excluding those with the same UserId as the provided id.
            var reviewRequests = await _context.Requests
                .Where(r => r.Status == "REVIEW" && r.UserId != id)
                .ToListAsync();

            if (!reviewRequests.Any())
            {
                return NotFound("No review requests available for this user.");
            }

            return Ok(reviewRequests);
        }


        
        //approve requests by id
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
            request.Status = "APPROVED";
            _context.Entry(request).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated request
            return Ok(request);
        }


        //This section checks to see if user is an admin before they can approve requests
        //not in specs, but would be good to add
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





        // PUT: api/Requests/{id}  update requests by ID
        // with concurrency exception that isn't necessary for specs

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


        //  api/Requests/reject/{id}  reject requests by ID
        [HttpPut("reject/{id}")]
        public async Task<ActionResult> RejectRequest(int id, [FromBody] string reason)
        {
            // Validate the reason for rejection
            if (string.IsNullOrWhiteSpace(reason))
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
            request.Status = "REJECTED";
            request.ReasonForRejection = reason;
            _context.Entry(request).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated request
            return Ok(request);
        }


      


        // POST: api/Requests  submit new purchase requests
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


        //helper method to create the request number with R + date + 0001 ++
        private string GetNextRequestNumber()
        {
            // Request number format: RYYMMDDXXXX
            string requestNbr = "R" + DateOnly.FromDateTime(DateTime.Now).ToString("yyMMdd");

            // Get the maximum request number so far for the current date
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
            // Calculate the total using one query
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

    }
}
