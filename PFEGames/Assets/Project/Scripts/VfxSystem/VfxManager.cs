using System;
using UnityEngine;

public class VfxManager : MonoBehaviour
{
    public IVfx CurrentStateVfx { get; private set; }
    public event Action<IVfx> VfxStateChanged; 


    public void GotoVfx(IVfx newStateVfx)
    {
        CurrentStateVfx?.Exit(this);
        CurrentStateVfx = newStateVfx;
        CurrentStateVfx?.Enter(this);
        VfxStateChanged?.Invoke(CurrentStateVfx);

    }
    
    
}
