using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }

        [PersonalData]
        public string JobDescription { get; set; }

        [PersonalData]
        public DateTime? BirthDate { get; set; }



        [PersonalData]
        public string EmployeeCode { get; set; }



        [PersonalData]
        public int? SAP { get; set; }

    }
}