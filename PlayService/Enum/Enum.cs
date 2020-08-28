using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Enum
{
    public enum Hand
    {
        Default,
        Rock,
        Paper,
        Scissors
    }

    public enum GameState
    {
        WaitingPlayer,
        Start,
        CompareHand,
        End
    }

    public enum Result
    {
        Draw,
        Win,
        Lose
    }

    public enum MessageType
    {
        UserInfo,
        GameUpdate
    }
}
