namespace Carcassonne.Controllers
{
    public interface IGameControllerInterface
    {
        public void NewGame();
        public void EndTurn();
        public void Reset();

    }
}