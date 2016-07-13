﻿using Microsoft.AspNet.Identity;
using Mini_Social_Networking_Web_App.DTO;
using Mini_Social_Networking_Web_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Mini_Social_Networking_Web_App.Controllers
{
    public class FollowingController : ApiController
    {
        private ApplicationDbContext _context;
        public FollowingController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpPost]
        public IHttpActionResult AddToFollowings(FolloweeDTO dto)
        {
            var userId = User.Identity.GetUserId();

            if (_context.Followings.Any(a => a.FollowerId == userId && a.FolloweeId == dto.ArtistId))
            {
                return BadRequest("Following already exists.");
            }
            var following = new Followings
            {
                FollowerId = userId,
                FolloweeId = dto.ArtistId
            };
            _context.Followings.Add(following);
            _context.SaveChanges();

            return Ok();

        }

    }
}
