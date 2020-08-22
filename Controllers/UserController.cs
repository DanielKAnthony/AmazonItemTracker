﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmazonTrackingApp.Models;

namespace AmazonTrackerApp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    public class UserController : ControllerBase
    {
    	private readonly TrackerContext _context;

    	public UserController(TrackerContext context)
    	{
    		_context = context;
    	}

    	[HttpGet]
    	public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    	{
    		return await _context.Users.ToListAsync();
    	}

        [HttpGet("logauth")]
        public bool AuthUser([FromQuery] string namefield,[FromQuery] string pass)
        {

            var tmpUser =  _context.Users.SingleOrDefault(user => 
            (namefield.Contains('@') ? user.Email:user.Username) == namefield);


            if(tmpUser == null)
            {
                return false;
            }

            return tmpUser.Password == pass;
        }

    	[HttpGet("{Id}")]
    	public async Task<ActionResult<User>> GetUser(int id)
    	{
    		var user = await _context.Users.FindAsync(id);

            if(user == null)
            {
                return NotFound();
            }

    		return user;
    	}

    	[HttpPut("{Id}")]
    	public async Task<IActionResult> PustUser(int id, User user)
    	{
    		user.Id = id;

            _context.Entry(user).State = EntityState.Modified;

    		try{
    			await _context.SaveChangesAsync();
    		}
    		catch(DbUpdateConcurrencyException)
    		{
    			if(!UserExists(id))
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

    	[HttpPost]
    	public async Task<ActionResult<User>> PostUser([FromBody] User user)
    	{
            User errUser = new User
            {
                Email = "temp",
                Username = "temp",
                Password = null
            };

            bool duplicate = false;

            if (EmailExists(user.Email))
            {
                errUser.Email = null;
                duplicate = true;
            }
            if (UsernameExists(user.Username))
            {
                errUser.Username = null;
                duplicate = true;
            }

            if (duplicate) return errUser;

            _context.Users.Add(user);
    		await _context.SaveChangesAsync();

    		return CreatedAtAction("GetUser", new { id = user.Id}, user);
    	}

    	[HttpDelete("{Id}")]
    	public async Task<ActionResult<User>> DeleteUser(int id)
    	{
    		var user = await _context.Users.FindAsync(id);



    		if(user == null)
    		{
    			return NotFound();
    		}

    		_context.Users.Remove(user);
    		await _context.SaveChangesAsync();

    		return user;
    	}

    	private bool UserExists(int id)
    	{
    		return _context.Users.Any(e => e.Id == id);
    	}

        private bool EmailExists(string Email)
        {
            return _context.Users.Any(e => e.Email == Email);
        }

        private bool UsernameExists(string Username)
        {
            return _context.Users.Any(e => e.Username == Username);
        }
    }
}
