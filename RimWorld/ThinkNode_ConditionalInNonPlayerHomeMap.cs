using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalInNonPlayerHomeMap : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.MapHeld != null && !pawn.MapHeld.IsPlayerHome;
		}
	}
}
