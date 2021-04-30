    using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<ShoppingCart> ListCart { get; set; }
     
    }
}
