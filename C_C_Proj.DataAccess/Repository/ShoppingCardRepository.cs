﻿using C_C_Proj_WebStore.DataAccess.Data;
using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace C_C_Proj_WebStore.DataAccess.Repository
{
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCard shoppingCard)
        {
            _db.ShoppingCards.Update(shoppingCard);
        }
    }
}
