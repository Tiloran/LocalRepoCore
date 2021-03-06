﻿using System;
using LevelStore.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LevelStore.Models
{
    public class SessionCart : Cart
    {
        public static Cart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionCart cart = session?.GetJson<SessionCart>("Cart") ?? new SessionCart();
            cart.Session = session;
            return cart;
        }

        [JsonIgnore]
        public ISession Session { get; set; }

        public override void SetPromoCode(int prmCodeDiscount, string prmCodeName)
        {
            base.SetPromoCode(prmCodeDiscount, prmCodeName);
            Session.SetJson("Cart", this);
        }

        public override void DeletePromoCode()
        {
            base.DeletePromoCode();
            Session.SetJson("Cart", this);
        }

        public override void AddItem(Product product, int quantity, int furniture, int selectedColor)
        {
            base.AddItem(product, quantity, furniture, selectedColor);            
            Session.SetJson("Cart", this);
        }

        public override void RemoveLine(Product product)
        {
            base.RemoveLine(product);
            Session.SetJson("Cart", this);
        }

        public override void Clear()
        {
            base.Clear();
            Session.Remove("Cart");
        }
    }
}
