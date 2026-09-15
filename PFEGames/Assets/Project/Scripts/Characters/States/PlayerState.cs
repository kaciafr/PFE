using UnityEngine.TextCore.Text;

namespace Characters
{
    public abstract class PlayerState
    {
        protected PlayerStateMachine ctx ;

        public PlayerState(PlayerStateMachine ctx)
        {
            this.ctx = ctx;
        }
        
        public virtual void Enter() { }

        public virtual void Tick() { }
    }
}