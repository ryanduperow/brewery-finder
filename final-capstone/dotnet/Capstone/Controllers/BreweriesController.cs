﻿using Microsoft.AspNetCore.Mvc;
using Capstone.DAO;
using Capstone.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Capstone.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BreweriesController : ControllerBase
    {
        private readonly IBreweryDao breweryDao;
        private readonly IUserDao userDao;
        private User CurrentUser
        {
            get
            {
                int currentUserId = int.Parse(User.FindFirst("sub")?.Value);
                return userDao.GetUser(currentUserId);
            }
        }

        public BreweriesController(IBreweryDao _breweryDao, IUserDao _userDao)
        {
            breweryDao = _breweryDao;
            userDao = _userDao;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllBreweries()
        {
            List<Brewery> listOfBreweries = breweryDao.GetAllBreweries();
            if (listOfBreweries != null)
            {
                return Ok(listOfBreweries);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{breweryId}")]
        [AllowAnonymous]
        public IActionResult GetBreweryById(int breweryId)
        {
            Brewery brewery = breweryDao.GetBrewery(breweryId);
            if (brewery != null)
            {
                return Ok(brewery);
            }
            else
            {
                return new NotFoundObjectResult("Couldn't find a brewery by that id.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "brewer, admin")]
        public IActionResult AddBrewery(Brewery brewery)
        {
            //LOGIC
            //You may only create a brewery with your own id as owner unless you're an administrator
            if (!(brewery.BreweryOwnerId == CurrentUser.UserId || CurrentUser.Role == "admin"))
            {
                return new UnauthorizedObjectResult("You may only create a brewery with your own id as owner.");
            }

            //The owner of a brewery must be an existing user
            if (userDao.GetUser(brewery.BreweryOwnerID) == null) new BadRequestObjectResult("The owner of a brewery must be an existing user.");

            Brewery result = breweryDao.AddBrewery(brewery);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{breweryId}")]
        [Authorize(Roles = "brewer, admin")]
        public IActionResult UpdateBrewery(int breweryId, Brewery brewery)
        {
            Brewery breweryToUpdate = breweryDao.GetBrewery(breweryId);

            if (breweryToUpdate != null)
            {
                //LOGIC
                //You may only update a brewery if you're the owner or if you are an administrator
                if (!(breweryToUpdate.BreweryOwnerID == CurrentUser.UserId || CurrentUser.Role == "admin"))
                {
                    return new UnauthorizedObjectResult("You may only update a brewery you own.");
                }

                //The owner of a brewery must be an existing user
                if (userDao.GetUser(brewery.BreweryOwnerID) == null) return new BadRequestObjectResult("The owner of a brewery must be an existing user.");

                breweryDao.UpdateBrewery(brewery);
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{breweryId}")]
        [Authorize(Roles = "brewer, admin")]
        public IActionResult DeleteBrewery(int breweryId)
        {
            Brewery breweryToDelete = breweryDao.GetBrewery(breweryId);

            if (breweryToDelete != null)
            {
                //LOGIC
                //You may only delete a brewery if you're the owner or if you are an administrator
                if (!(breweryToDelete.BreweryOwnerID == CurrentUser.UserId || CurrentUser.Role == "admin"))
                {
                    return new UnauthorizedObjectResult("You may only delete a brewery you own.");
                }

                breweryDao.DeleteBrewery(breweryId);
                return Ok();
            }
            else
            {
                return new NotFoundObjectResult("Couldn't find a brewery by that id.");
            }
        }

        
    }
}
