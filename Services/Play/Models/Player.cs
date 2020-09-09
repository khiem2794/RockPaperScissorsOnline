using Play.Enum;

namespace Play.Models
{
    public class Player
    {
        public int Point { get; set; }
        public User User { get; }
        public Hand PlayerHand { get; set; }
        public bool LeftGame { get; set; }
        public Player(User user)
        {
            User = user;
            Point = 0;
            PlayerHand = Hand.Default;
            LeftGame = false;
        }
        public void ResetHand()
        {
            PlayerHand = Hand.Default;
        }
    }
}
