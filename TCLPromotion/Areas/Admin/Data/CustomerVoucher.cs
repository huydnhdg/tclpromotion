using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCLPromotion.Models;

namespace TCLPromotion.Areas.Admin.Data
{
    public class CustomerVoucher:Customer
    {
        public string Voucher { get; set; }
    }
}