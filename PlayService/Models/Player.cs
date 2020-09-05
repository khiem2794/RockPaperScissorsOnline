using Play.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Models
{
    interface IPlayer
    {
    }
    public class Player : IPlayer
    {
        public int Point { get; set; }
        public User User { get; set; }
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
