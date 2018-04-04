public interface IStateHandler
{
    void Record();
    void Undo();
    void Commit();
}