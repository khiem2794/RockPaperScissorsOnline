using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profile.Models
{
    public class ProfileInfo
    {
        public UserInfo User { get; set; }
        public List<GameInfo> Games { get; set; }
    }
}
