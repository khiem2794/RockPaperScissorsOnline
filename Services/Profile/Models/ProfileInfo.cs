using System.Collections.Generic;

namespace Profile.Models
{
    public class ProfileInfo
    {
        public UserInfo User { get; set; }
        public List<GameInfo> Games { get; set; }
    }
}
