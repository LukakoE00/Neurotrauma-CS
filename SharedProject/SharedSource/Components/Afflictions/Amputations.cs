using Barotrauma;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Neurotrauma;

public static class Amputations
{
    // Each extremity has additional limbs (a leg has the thigh + leg + foot) that get removed when the entire extremity is severed.
    // We'll need to restore all sub-parts of the extremity too, so we borrow basegame's traversal to find what's attached to what.
    private static readonly MethodInfo GetConnectedLimbs = typeof(Ragdoll).GetMethod("GetConnectedLimbs", BindingFlags.Instance | BindingFlags.NonPublic)!;

    // Effectively the inverse of basegame's 'Ragdoll.SeverLimbJoint' method. We run it on the server via items then pass the info along to clients so it syncs.
    // Otherwise, one person sees a nugget and the other an actual human being. Not good!
    public static bool RegenerateLimb(Character C, LimbType TargetLimb, bool ReceivedFromServer = false)
    {
        // A client shouldn't be able to do this unless it's used to sync up with the server.
        if (!ReceivedFromServer && !HF.IsMain())
        {
            return false;
        }

        Ragdoll Ragdoll = C.AnimController;

        // Which limbs are attached to the given limb through non-severed joints? Fresh sets every time, never basegame's shared ones.
        HashSet<Limb> GetConnected(Limb StartLimb)
        {
            var ConnectedLimbs = new HashSet<Limb>();
            var CheckedJoints = new HashSet<LimbJoint>();
            GetConnectedLimbs.Invoke(Ragdoll, new object[] { ConnectedLimbs, CheckedJoints, StartLimb, false });
            return ConnectedLimbs;
        }

        // Now we see if the limbs that are part of the selected extremity are actually severed.
        var SelectedExtremity = HF.NormalizeLimbType(TargetLimb);
        var LimbsThatShouldBeRegenerated = Ragdoll.Limbs.Where(SubLimb => SubLimb.IsSevered && HF.NormalizeLimbType(SubLimb.type) == SelectedExtremity).ToHashSet();

        // Nothing severed? Hit the road dude
        if (LimbsThatShouldBeRegenerated.Count == 0)
        {
            return false;
        }

        // Where originally it severs the joint, we instead bring it back.
        var JointsThatShouldBeRegenerated = Ragdoll.LimbJoints.Where(Joint => Joint.IsSevered && (LimbsThatShouldBeRegenerated.Contains(Joint.LimbA) || LimbsThatShouldBeRegenerated.Contains(Joint.LimbB))).ToList();
        foreach (var Joint in JointsThatShouldBeRegenerated)
        {
            Joint.IsSevered = false;
        }

        // Check from the main limb onwards now.
        var ConnectedToMainLimb = GetConnected(Ragdoll.MainLimb);

        // Limbs that started off severed but are attached again.
        var RestoredLimbs = Ragdoll.Limbs.Where(SubLimb => SubLimb.IsSevered && ConnectedToMainLimb.Contains(SubLimb)).ToHashSet();
        bool AnyRegenerated = JointsThatShouldBeRegenerated.Count > 0 || RestoredLimbs.Count > 0;

        // Snap the newly restored limbs back to reality (attach them to the body again).
        var LimbsToPlace = new HashSet<Limb>(RestoredLimbs);
        bool PlacedAny = true;

        while (LimbsToPlace.Count > 0 && PlacedAny)
        {
            PlacedAny = false;
            foreach (var Joint in Ragdoll.LimbJoints)
            {
                if (Joint.IsSevered) continue;

                bool LimbAWaiting = LimbsToPlace.Contains(Joint.LimbA);
                if (LimbAWaiting == LimbsToPlace.Contains(Joint.LimbB))
                {
                    continue; 
                }

                Limb ChildLimb = LimbAWaiting ? Joint.LimbA : Joint.LimbB;

                Vector2 AnchorA = Joint.BodyA.GetWorldPoint(Joint.LocalAnchorA);
                Vector2 AnchorB = Joint.BodyB.GetWorldPoint(Joint.LocalAnchorB);
                Vector2 Offset = LimbAWaiting ? AnchorB - AnchorA : AnchorA - AnchorB;

                // Don't make it tweak out.
                ChildLimb.body.ResetDynamics(); 
                ChildLimb.body.SetTransform(ChildLimb.body.SimPosition + Offset, ChildLimb.body.Rotation);

                LimbsToPlace.Remove(ChildLimb);
                PlacedAny = true;
            }
        }

        // Remove the IsSevered flags!
        foreach (var RestoredLimb in RestoredLimbs)
        {
            RestoredLimb.IsSevered = false;
        }

        // There is no syncing for un-severing a limb so we gotta do that ourselves. Pass along the character + limb to fix and have the client run this function too!
#if SERVER
        if (AnyRegenerated && !ReceivedFromServer)
        {
            foreach (Client client in GameMain.Server.ConnectedClients)
            {
                if (client.Connection == null) continue;

                IWriteMessage msg = LuaCsSetup.Instance.Networking.Start("NT.RegenLimb");
                msg.WriteUInt16(C.ID);
                msg.WriteInt32((int)TargetLimb);
                LuaCsSetup.Instance.Networking.Send(msg, client.Connection);
            }
        }
#endif

        return AnyRegenerated;
    }
}