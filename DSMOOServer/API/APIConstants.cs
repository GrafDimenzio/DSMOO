using DSMOOServer.API.Enum;

namespace DSMOOServer.API;

public static class APIConstants
{
    public static Dictionary<int, PlayerAction> PlayerActions = new()
    {
        { 1, PlayerAction.Damage },
        { 14, PlayerAction.Bonk },
        { 20, PlayerAction.RainbowTwirl },
        { 24, PlayerAction.SpinJump },
        { 105, PlayerAction.Death },
        { 106, PlayerAction.Death },
        { 107, PlayerAction.Death },
        { 108, PlayerAction.Death }, // NOT TESTED JUST ASSUMED
        { 109, PlayerAction.Death }, // NOT TESTED JUST ASSUMED
        { 110, PlayerAction.Death }, // NOT TESTED JUST ASSUMED
        { 111, PlayerAction.Death }, // NOT TESTED JUST ASSUMED
        { 112, PlayerAction.Death }, // NOT TESTED JUST ASSUMED
        { 113, PlayerAction.Death },
        { 114, PlayerAction.Death },
        { 235, PlayerAction.Dive },
        { 236, PlayerAction.GroundPound },
        { 241, PlayerAction.Jump }, // Walking Jump
        { 242, PlayerAction.DoubleJump },
        { 243, PlayerAction.TripleJump },
        { 244, PlayerAction.Backflip },
        { 245, PlayerAction.LongJump },
        { 255, PlayerAction.CapTrample },
        { 263, PlayerAction.Jump }, // Stand still jump
        { 265, PlayerAction.SideFlip },
        { 346, PlayerAction.Roll },
        { 352, PlayerAction.RollBoost },
        { 392, PlayerAction.Throw },
        { 403, PlayerAction.Throw },
        { 419, PlayerAction.Spin },
        { 423, PlayerAction.SpinGroundPound },
        { 432, PlayerAction.Crouch },
        { 433, PlayerAction.Crouch },
        { 434, PlayerAction.Crouch },
        { 512, PlayerAction.LedgeUp },
        { 517, PlayerAction.LedgeGrab }
    };
}