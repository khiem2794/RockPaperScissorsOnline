namespace Play.Enum
{
    public enum Hand
    {
        Default = 0,
        Rock,
        Paper,
        Scissors
    }

    public enum GameState
    {
        Waiting,
        Start,
        Compare,
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
        ConnectionEstablished,
        GameUpdate
    }
}
