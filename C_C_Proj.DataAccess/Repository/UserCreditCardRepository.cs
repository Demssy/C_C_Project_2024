using C_C_Proj_WebStore.DataAccess.Data;
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
    public class UserCreditCardRepository : Repository<UserCreditCard>, IUserCreditCardRepository
    {
        private ApplicationDbContext _db;
        public UserCreditCardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(UserCreditCard userCreditCard)
        {
            throw new NotImplementedException();
        }
    }
}
