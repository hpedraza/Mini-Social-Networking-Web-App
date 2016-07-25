﻿using Microsoft.AspNet.Identity;
using Mini_Social_Networking_Web_App.Models;
using Mini_Social_Networking_Web_App.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using System;

namespace Mini_Social_Networking_Web_App.Controllers
{
    public class GigsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GigsController()
        {
            // worrt about Dependency Injection, repository pattern later 
            
            _context = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Mine()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Gigs
                .Where(g => g.ArtistId == userId && 
                       g.IsCanceled != true &&
                       g.DateTime > DateTime.Now)
                .Include(g => g.Genre)
                .ToList();

            return View(gigs);
        }


        [Authorize]
        public ActionResult Attending()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Attendances
                .Where(a => a.AttendeeId == userId)
                .Select(a => a.Gig)
                .Include(g => g.Artist)
                .Include(g => g.Genre)
                .ToList();

            var attendances = _context.Attendances.Where(x => x.AttendeeId == userId && x.Gig.DateTime > DateTime.Now).ToList().ToLookup(a => a.GigId);
            var followings = _context.Followings.Where(x => x.FollowerId == userId).ToList().ToLookup(x => x.FolloweeId);
            var vm = new GigsViewModel
            {
                UpcomingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading = "Gigs I'm Attending",
                Attendances = attendances,
                Followings = followings
            };

            return View("Gigs",vm);
        }


        [HttpPost]
        public ActionResult Search(GigsViewModel vm)
        {
            return RedirectToAction("Index", "Home", new { query = vm.SearchTerm });
        }

        [Authorize]
        public ActionResult Create()
        {
            var viewModel = new GigFormViewModel(_context.Genres.ToList() , "Add a Gig");
            return View("GigForm" , viewModel);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var gig = _context.Gigs.Single(g => g.Id == id && g.ArtistId == userId);

            var viewModel = new GigFormViewModel(gig.Id , 
                _context.Genres.ToList() , 
                gig.DateTime.ToString("d MMM yyyy"), 
                gig.DateTime.ToString("HH:MM"),
                gig.GenreId, 
                gig.Venue,
                "Edit Gig");

            return View("GigForm",viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Details(int gigId)
        {
            var gig = _context.Gigs
                .Include(x => x.Artist)
                .Single(g => g.Id == gigId);

            var Details = new GigDetailsViewModel(gig.Artist.Name,gig.Venue, gig.DateTime , gig.ArtistId);

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();

                Details.IsAuthenticated = true;

                Details.IsAttendding = _context.Attendances.Any(a => a.GigId == gig.Id && a.AttendeeId == userId);
               
                Details.IsFollowing = _context.Followings.Any(F => F.FolloweeId == gig.Artist.Id && F.FollowerId == userId);
            }

            return View(Details);


        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GigFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SetGenresList(_context.Genres.ToList());
                return View("GigForm", vm);
            }

            // create constructor for create
            string userId = User.Identity.GetUserId();
            var Followers = (from user in _context.Users
                             where user.Id == userId
                             from follower in user.Followers
                             from nu in _context.Users
                             where nu.Id == follower.FollowerId
                             select nu).ToList(); 




                var gig = new Gig(userId, vm.Genre , vm.GetDateTime() , vm.Venue, Followers);
                            _context.Gigs.Add(gig);
                            _context.SaveChanges();

            return RedirectToAction("Mine","Gigs");
}

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(GigFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SetGenresList(_context.Genres.ToList());
                return View("GigForm", vm);
            }

            var userId = User.Identity.GetUserId();

            var gig = _context.Gigs
                .Include(g=>g.Attendances.Select(a => a.Attendee))
                .Single(g => g.Id == vm.Id && g.ArtistId == userId);

            gig.UpdateGig(vm.Venue, vm.GetDateTime() , vm.Genre);

            _context.SaveChanges();

            return RedirectToAction("Mine", "Gigs");

        }

    }
}