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
    // LineItems controller with address api/lineitems
    [Route("api/[controller]")]
    [ApiController]
    public class LineItemsController : ControllerBase
    {
        private readonly PRSDBContext _context;

        public LineItemsController(PRSDBContext context)
        {
            _context = context;
        }

        // GET: api/LineItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItems()
        {
            return await _context.LineItems.ToListAsync();
        }

        // GET: api/LineItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LineItem>> GetLineItem(int id)
        {
            var lineItem = await _context.LineItems.FindAsync(id);

            if (lineItem == null)
            {
                return NotFound();
            }

            return lineItem;
        }

        [HttpGet("lineItems-for-request/{reqID}")]
        public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItemsForRequest(int reqID)
        {
            // Validate that the RequestId exists
            var requestExists = await _context.Requests.AnyAsync(r => r.Id == reqID);
            if (!requestExists)
            {
                return NotFound($"Request with ID {reqID} not found.");
            }

            // Retrieve all LineItems for the given RequestId
            var lineItems = await _context.LineItems
                .Where(li => li.RequestId == reqID)
                .ToListAsync();

            return Ok(lineItems);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLineItem(int id, [FromBody] LineItemDTO lineItemDto)
        {
            // Find the existing line item by ID
            var lineItem = await _context.LineItems.FindAsync(id);
            if (lineItem == null)
            {
                return NotFound($"Line item with ID {id} not found.");
            }

            // Update the line item with values from the DTO
            lineItem.RequestId = lineItemDto.RequestId;
            lineItem.ProductId = lineItemDto.ProductId;
            lineItem.Quantity = lineItemDto.Quantity;

            // Mark the line item as modified
            _context.Entry(lineItem).State = EntityState.Modified;

            try
            {
                // Save changes to the line item
                await _context.SaveChangesAsync();

                // Recalculate the total for the associated request
                var request = await _context.Requests.FindAsync(lineItem.RequestId);
                if (request != null)
                {
                    request.Total = CalculateTotal(request.Id);
                    _context.Entry(request).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineItemExists(id))
                {
                    return NotFound("Line item not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }






        private bool LineItemExists(int id)
        {
            return _context.LineItems.Any(e => e.Id == id);
        }

        private decimal CalculateTotal(int requestId)
        {
            // Calculate the total using a single query.
            return _context.LineItems
                .Where(li => li.RequestId == requestId)
                .Join(
                    _context.Products,
                    li => li.ProductId,
                    p => p.Id,
                    (li, p) => p.Price * li.Quantity
                )
                .Sum();
        }


        // POST: api/LineItems
        [HttpPost]
        public async Task<IActionResult> AddLineItem(LineItemDTO lineItemDto)
        {
            if (lineItemDto == null)
            {
                return BadRequest("Line item data is required.");
            }

            // Validate the product exists
            var product = await _context.Products.FindAsync(lineItemDto.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {lineItemDto.ProductId} does not exist.");
            }

            if (product.Price <= 0)
            {
                return BadRequest($"Product with ID {lineItemDto.ProductId} has an invalid price.");
            }

            // Validate the request exists
            var request = await _context.Requests.FindAsync(lineItemDto.RequestId);
            if (request == null)
            {
                return NotFound($"Request with ID {lineItemDto.RequestId} does not exist.");
            }

            // Add the line item
            var lineItem = new LineItem
            {
                ProductId = lineItemDto.ProductId,
                Quantity = lineItemDto.Quantity,
                RequestId = lineItemDto.RequestId
            };

            _context.LineItems.Add(lineItem);
            await _context.SaveChangesAsync();

            // Recalculate the total for the request
            request.Total = CalculateTotal(request.Id);
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(request); // Return the updated request
        }

        // DELETE: api/LineItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLineItem(int id)
        {
            var lineItem = await _context.LineItems.FindAsync(id);
            if (lineItem == null)
            {
                return NotFound();
            }

            // Remove the line item
            _context.LineItems.Remove(lineItem);
            await _context.SaveChangesAsync();

            // Recalculate the total for the associated request
            var request = await _context.Requests.FindAsync(lineItem.RequestId);
            if (request != null)
            {
                request.Total = CalculateTotal(request.Id);
                _context.Entry(request).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
