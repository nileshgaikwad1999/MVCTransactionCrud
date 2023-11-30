using MLCS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLCS.ViewModel
{
    public class CustomerViewModel
    {
        public Customer Customer { get; set; } = new Customer();

        public ProductBill ProductBill { get; set; } = new ProductBill();

        public List<ProductillDetails> ProductillDetailList { get; set; } = new List<ProductillDetails>();

        public ProductillDetails ProductillDetails { get; set; } = new ProductillDetails();

        public Bill Bill { get; set; } = new Bill();
    }
}