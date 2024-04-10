using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace C_C_Proj_WebStore.Models
{
    public class UserCreditCard
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public byte[] EncryptedCardNumber { get; set; }

        [Required]
        public int ExpiryMonth { get; set; }

        [Required]
        public int ExpiryYear { get; set; }

        [Required]
        public byte[] EncryptedCVV { get; set; }


        [NotMapped]
        [StringLength(12, ErrorMessage = "Invalid Input")]
        public string CardNumber { get; set; }

        [NotMapped]
        [StringLength(3, ErrorMessage = "Invalid Input")]
        public string CVV { get; set; }

        [Required]
        public byte[] key { get; set; }
        [Required]
        public byte[] iv { get; set; }


    }


}
