using UnityEngine;

namespace Goblfin.PnjSystem
{
    public static class Communication
    {
	    private static BrainPnj inAction;
	    public static bool SomeoneIsIn(BrainPnj brainPnj)
	    {
		    if (inAction != null) return true;
		    inAction = brainPnj;
		    return false;
	    }

	    public static void EndAction(BrainPnj brainPnj)
	    {
		    inAction = null;
	    }
	    
	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	    private static void Reset() => inAction = null;
    }
}
 