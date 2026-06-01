// Models/UserRoleViewModel.cs
using System.Collections.Generic;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}