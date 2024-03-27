using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C_Proj_WebStore.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCard> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
