using Microsoft.AspNetCore.Authorization;
﻿using System.Diagnostics;
using Bookstore.Web.Models;
using Bookstore.Domain.Books;
using System.Threading.Tasks;
using Bookstore.Web.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IBookService bookService;

        public HomeController(IBookService bookService)
        {
            this.bookService = bookService;
        }

        public async Task<IActionResult> Index()
        {
            var books = await bookService.ListBestSellingBooksAsync(4);

            return View(new HomeIndexViewModel(books));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Search()
        {
            return RedirectToAction("Index", "Search");
        }

        public IActionResult Cart()
        {
            return RedirectToAction("Index", "ShoppingCart");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id });
        }
    }
}
