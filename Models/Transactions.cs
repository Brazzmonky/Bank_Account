using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank_Account
{    
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        public int TransactionId {get;set;}

        [Required]
        [Display(Name="Amount")]
        public decimal Amount {get;set;}
        
        [Required]
        public DateTime created_at{get;set;} =DateTime.Now;
        
        [Required]
        [Display(Name="User")]
        public int UserId{get;set;}
        public User User {get;set;}
    }


}        