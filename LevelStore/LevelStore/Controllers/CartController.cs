﻿using System.Collections.Generic;
using System.Linq;
using LevelStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LevelStore.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository repository;
        private readonly IShareRepository shareRepository;
        private readonly Cart cart;

        public CartController(IProductRepository repo, Cart cartService, IShareRepository shareRepo)
        {
            repository = repo;
            shareRepository = shareRepo;
            cart = cartService;
        }

        public ViewResult Index()
        {
            List<TypeColor> colors = repository.TypeColors.ToList();
            TempData["colors"] = colors;
            TempData["Shares"] = shareRepository.Shares.ToList();
            return View("ListCart", cart);
        }

        public IActionResult AddToCart(int productId, int quantity, int? furniture, int? selectedColor)
        {
            if (furniture == null || selectedColor == null || selectedColor == 0)
            {
                return RedirectToAction($"ViewSingleProduct", new RouteValueDictionary(
                    new { controller = "Product", action = "ViewSingleProduct", productId = productId, wasError = true}));
            }
            if (quantity == 0)
            {
                quantity = 1;
            }
            Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product != null)
            {
                cart.AddItem(product, quantity, (int) furniture, (int) selectedColor);
            }
            return RedirectToAction("List", "Product");
        }

        public RedirectToActionResult RemoveFromCart(int productId)
        {
            Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product != null)
            {
                cart.RemoveLine(product);
            }
            return RedirectToAction("List", "Product");
        }
    }
}