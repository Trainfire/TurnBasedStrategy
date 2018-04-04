public interface IStateHandler
{
    void SaveStateBeforeMove();
    void RestoreStateBeforeMove();
    void CommitStateAfterAttack();
}