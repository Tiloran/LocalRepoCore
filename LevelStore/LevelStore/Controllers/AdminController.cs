﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LevelStore.Infrastructure;
using LevelStore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LevelStore.Controllers
{
    public class AdminController : Controller
    {
        private IProductRepository repository;
        private IHostingEnvironment _appEnvironment;

        public AdminController(IProductRepository repo, IHostingEnvironment appEnvironment)
        {
            repository = repo;
            _appEnvironment = appEnvironment;
        }

        public ViewResult Create()
        {
            TempData["OtherStuffForProduct"] = new OtherStuffForProduct();
            TempData["Colors"] = repository.TypeColors.ToList();
            TempData["BoundColors"] = new List<Color>();
            TempData["Accesories"] = repository.Accessories.ToList();
            return View("Edit", new Product());
        }

        
        public IActionResult Edit(int productid)
        {
            Product product = repository.Products.FirstOrDefault(i => i.ProductID == productid);
            TempData["OtherStuffForProduct"] = new OtherStuffForProduct();
            TempData["Colors"] = repository.TypeColors.ToList();
            TempData["BoundColors"] = repository.BoundColors.Where(i=> i.ProductID == productid).ToList();
            TempData["Accesories"] = repository.Accessories.ToList();
            return View("Edit", product);
        }

        [HttpPost]
        public IActionResult Edit(Product product, List<int> colors)
        {
            if (ModelState.IsValid)
            {
                int? id = repository.SaveProduct(product, colors);
                if (id == null)
                {
                    return View();
                }
                TempData["id"] = id;
                //TempData["ImageList"] = repository.Images.ToList();
                List<Color> bindedColors = repository.BoundColors.Where(i => i.ProductID == id).ToList();
                List<TypeColor> ourTypeColors = repository.TypeColors
                    .Where(i1 => bindedColors.Any(i2 => i2.TypeColorID == i1.TypeColorID)).ToList();
                List<Image> imageList = repository.Images.Where(i => i.ProductID == id).ToList();
                if (imageList.Count > 1)
                {
                    bool NoFirst = imageList.FirstOrDefault(f => f.FirstOnScreen && f.SecondOnScreen == false) == null;
                    if (NoFirst)
                    {
                        imageList.FirstOrDefault(f => f.FirstOnScreen == false && f.SecondOnScreen == false)
                            .FirstOnScreen = true;
                    }
                    bool NoSecond = imageList.FirstOrDefault(s => s.FirstOnScreen == false && s.SecondOnScreen) == null;
                    if (NoSecond)
                    {
                        imageList.FirstOrDefault(s => s.FirstOnScreen == false && s.SecondOnScreen == false)
                            .SecondOnScreen = true;
                    }
                    bool bug = imageList.FirstOrDefault(s => s.FirstOnScreen && s.SecondOnScreen) != null;
                    if (bug)
                    {
                        foreach (var image in imageList)
                        {
                            if (image.FirstOnScreen && image.SecondOnScreen)
                            {
                                image.FirstOnScreen = false;
                                image.SecondOnScreen = false;
                            }
                        }
                    }
                }
                List<TypeColor> boundedColors = repository.GetColorThatBindedWithImages(imageList);
                TempData["Colors"] = ourTypeColors;
                TempData["ImageList"] = imageList;
                TempData["BindedColors"] = boundedColors;
                //TempData["Colors"] = repository.TypeColors.ToList();
                return View("UploadFiles");
            }
            //Some error
            return View();
        }

        public IActionResult AddColors()
        {
            TempData["ColorList"] = repository.TypeColors.ToList();
            return View(new TypeColor());
        }

        [HttpPost]
        public IActionResult AddColors(TypeColor newTypeColor)
        {
            repository.SaveTypeColor(newTypeColor);
            TempData["ColorList"] = repository.TypeColors.ToList();
            return View(new TypeColor());
        }

        public IActionResult BindPhotoAndColor(int?[] imageID, int?[] ColorID, string[] Alternative, int? ValFirstOnScreen, int? ValSecondOnScreen)
        {
            if (imageID.Length == ColorID.Length)
            {
                for (int i = 0; i < imageID.Length; i++)
                {
                    if (imageID != null)
                    {
                        Image currentImage = repository.Images.FirstOrDefault(iid => iid.ImageID == imageID[i]);
                        currentImage.Alternative = Alternative[i];
                        if (imageID[i] == ValFirstOnScreen)
                        {
                            currentImage.FirstOnScreen = true;
                            currentImage.SecondOnScreen = false;
                        }
                        else if (imageID[i] == ValSecondOnScreen)
                        {
                            currentImage.FirstOnScreen = false;
                            currentImage.SecondOnScreen = true;
                        }
                        else
                        {
                            currentImage.FirstOnScreen = false;
                            currentImage.SecondOnScreen = false;
                        }
                        
                        TypeColor selectedColor = repository.TypeColors.FirstOrDefault(tcid => tcid.TypeColorID == ColorID[i]);
                        if (currentImage != null && selectedColor != null)
                        {
                            if (selectedColor.Images == null)
                            {
                                selectedColor.Images = new List<Image>();
                            }
                            selectedColor.Images.Add(currentImage);
                            repository.SaveTypeColor(selectedColor);
                        }
                    }
                }
            }

            int? id = TempData["id"] as int?;
            if (id != null)
            {
                TempData["id"] = id;
                List<Image> imageList = repository.Images.Where(i => i.ProductID == id).ToList();
                List<TypeColor> boundedColors = repository.GetColorThatBindedWithImages(imageList);
                List<Color> bindedColors = repository.BoundColors.Where(i => i.ProductID == id).ToList();
                List<TypeColor> ourTypeColors = repository.TypeColors
                    .Where(i1 => bindedColors.Any(i2 => i2.TypeColorID == i1.TypeColorID)).ToList();
                TempData["Colors"] = ourTypeColors;
                TempData["ImageList"] = imageList;
                TempData["BindedColors"] = boundedColors;
                return View("UploadFiles");
            }
            else
            {
                return RedirectToActionPermanent(actionName : "ListAdmin", controllerName: "Product");
            }
            
        }

        public IActionResult RemoveColor(int typeColorId)
        {
            repository.DeleteTypeColor(typeColorId);
            List<TypeColor> typeColors = repository.TypeColors.ToList();
            TempData["ColorList"] = typeColors;
            return View("AddColors",new TypeColor());
        }



        public ViewResult Test() => View();

        [HttpPost]
        public ViewResult Test(IList<Image> list)
        {
            return View();
        }

        public IActionResult UploadFiles()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFiles(IList<IFormFile> files)
        {
            long size = 0;
            List<string> imageNameList = new List<string>();
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                    .Parse(file.ContentDisposition)
                    .FileName
                    .ToString()
                    .Trim('"');
                imageNameList.Add(filename);
                filename = _appEnvironment.WebRootPath + $@"\images\{filename}";
                size += file.Length;
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }
            int? id = TempData["id"] as int?;
            //ViewBag.Message = $"{files.Count} file(s) / {size} bytes uploaded successfully!";
            if (id != null)
            {
                repository.AddImages(imageNameList, id);
                
            }

            //return RedirectToAction(actionName: "ListAdmin", controllerName: "Product");
            TempData["id"] = id;
            List<Image> imageList = repository.Images.Where(i => i.ProductID == id).ToList();
            List<TypeColor> boundedColors = repository.GetColorThatBindedWithImages(imageList);
            List<Color> bindedColors = repository.BoundColors.Where(i => i.ProductID == id).ToList();
            List<TypeColor> ourTypeColors = repository.TypeColors
                .Where(i1 => bindedColors.Any(i2 => i2.TypeColorID == i1.TypeColorID)).ToList();
            TempData["Colors"] = ourTypeColors;
            TempData["ImageList"] = imageList;
            TempData["BindedColors"] = boundedColors;
            return View("UploadFiles");
        }

        [HttpPost]
        public IActionResult UploadFilesAjax()
        {
            long size = 0;
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                    .Parse(file.ContentDisposition)
                    .FileName
                    .ToString()
                    .Trim('"');
                filename = _appEnvironment.WebRootPath + $@"\images\{filename}";
                size += file.Length;
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }
            string message = $"{files.Count} file(s) / {size} bytes uploaded successfully!";
            return Json(message);
        }
    }
}