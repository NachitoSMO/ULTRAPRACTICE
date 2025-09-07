using UnityEngine;

namespace ULTRAPRACTICE.Classes;

/// <summary>
/// coins use the old implementation with a hacky solution to the Invoke() issue because the way I had to get and spawn coins was really strange
/// and hard to make it work otherwise
/// </summary>
public sealed class CoinTimers : MonoBehaviour
{
    public float deleteTimer;

    public float checkSpeedTimer;

    public float tripleTimer;

    public float tripleEndTimer;

    public float doubleTimer;

}