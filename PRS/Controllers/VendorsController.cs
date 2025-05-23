﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Models;


namespace PRS.Controllers
{
    //vendor controller with address api/vendors
    [Route("api/[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private readonly PRSDBContext _context;

        public VendorsController(PRSDBContext context)
        {
            _context = context;
        }

        // GET: api/Vendors  get all vendors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors()
        {
            return await _context.Vendors.ToListAsync();
        }

        // GET: api/Vendors/{id}  get vendors by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return NotFound();
            }

            return vendor;
        }

        // PUT: api/Vendors/{id}  edit vendor by id
        //this try catch concurrency exception does not consistently appear
        //in my code.  It would be better to have consistency and it's not necessary
        //for the scale of this project, but it's here for now as an example

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendor(int id, Vendor vendor)
        {
            if (id != vendor.Id)
            {
                return BadRequest();
            }

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
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

        // POST: api/Vendor  add vendor by id
        [HttpPost]
        public async Task<ActionResult<Vendor>> PostVendor(Vendor vendor)
        {
            // Validate the input
            if (vendor == null ||
                string.IsNullOrWhiteSpace(vendor.Name) ||
                string.IsNullOrWhiteSpace(vendor.Address) ||
                string.IsNullOrWhiteSpace(vendor.City) ||
                string.IsNullOrWhiteSpace(vendor.State) ||
                string.IsNullOrWhiteSpace(vendor.Zip) ||
                string.IsNullOrWhiteSpace(vendor.Code))
            {
                return BadRequest("Name, address, city, state, zip, and code are required.");
            }

            // Add the vendor to the database
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            // Return the created vendor
            return CreatedAtAction("GetVendor", new { id = vendor.Id }, vendor);
        }



        // DELETE: api/Vendors/{id}  delete vendor by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
}
