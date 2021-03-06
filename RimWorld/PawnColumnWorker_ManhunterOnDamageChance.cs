using System;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_ManhunterOnDamageChance : PawnColumnWorker_Text
	{
		protected override string GetTextFor(Pawn pawn)
		{
			return PawnUtility.GetManhunterOnDamageChance(pawn, null).ToStringPercent();
		}

		protected override string GetTip(Pawn pawn)
		{
			return "HarmedRevengeChanceExplanation".Translate();
		}
	}
}
