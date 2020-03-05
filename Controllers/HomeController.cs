using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bank_Account.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Bank_Account.Controllers
{
    public class HomeController : Controller
    {
        private int? UserSession
        {
            get { return HttpContext.Session.GetInt32("UserId"); }
            set { HttpContext.Session.SetInt32("UserId", (int)value); }
        }
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext=context;
        }[HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email==user.Email))
                {
                    ModelState.AddModelError("Email","Email is already in use choose another or go to login");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    dbContext.Add(user);
                    dbContext.SaveChanges();
                    return RedirectToAction("Account");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("userlogin")]
        public IActionResult UserLogin(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                User userInDb =dbContext.Users.FirstOrDefault(u => u.Email==userSubmission.Email);
                if(userInDb ==null)
                {
                    ModelState.AddModelError("Email","Invalid Email/Password");
                    return View("Login");
                }
                else
                {    
                    PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
                    var result= hasher.VerifyHashedPassword(userSubmission,userInDb.Password,userSubmission.Password);
                    if(result==0)
                    {
                        ModelState.AddModelError("Password","Invalid Password");
                        return View("Login");
                    }
                    else
                    {
                        UserSession = userInDb.UserId;
                        return RedirectToAction("Account");
                    }
                }
            }
            else
            {
                return View("Login");
            }
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("account")]
        public IActionResult Account()
        {
            if (UserSession == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
            // Grab user with included transactions
                User thisUser = dbContext.Users.Include(u => u.UserTransactions).FirstOrDefault(u => u.UserId == UserSession);
                ViewBag.User = thisUser;
                ViewBag.Transactions = dbContext.Transactions
                .OrderBy(t => t.created_at)
                .Where(t => t.UserId == thisUser.UserId);
                return View("account");
            }        
        }

        [HttpPost("account/{userId}")]
        public IActionResult Money(Transaction transaction)
        {
            User thisUser = dbContext.Users.Include(u => u.UserTransactions).FirstOrDefault(u => u.UserId == UserSession);
            if (ModelState.IsValid)
            {
                 if (transaction.Amount < (0 - thisUser.Balance))
                {
                    ModelState.AddModelError("Amount", "Who you Foolin you aint no baller!");
                    // Set viewbag again to display correct info while allowing errors to show
                    ViewBag.User = thisUser;
                    ViewBag.Transactions = dbContext.Transactions
                        .OrderBy(t => t.created_at)
                        .Where(t => t.UserId == thisUser.UserId);
                    return View("Account");
                }
                else
                {
                    dbContext.Transactions.Add(transaction);
                    dbContext.SaveChanges();
                    return RedirectToAction("Account");
                }
            }
            else
            {
                ViewBag.User = thisUser;
                ViewBag.Transactions = dbContext.Transactions
                .OrderBy(t => t.created_at)
                .Where(t => t.UserId == thisUser.UserId);
                return View("Account");
            }
        }
        
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }







    }
}