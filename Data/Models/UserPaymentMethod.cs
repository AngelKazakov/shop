﻿using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class UserPaymentMethod
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int PaymentTypeId { get; set; }

        public PaymentType PaymentType { get; set; }

        public string Provider { get; set; }

        public int ShopOrderId { get; set; }

        public ShopOrder ShopOrder { get; set; }

        public int AccountNumber { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsDefault { get; set; }
    }
}
