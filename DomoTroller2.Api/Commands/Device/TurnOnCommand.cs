namespace DomoTroller2.Api.Commands.Device
{
    public class TurnOnCommand
    {
        public TurnOnCommand()
        {
            Level = 100;
        }

        public int Level { get; set; }
    }
}
