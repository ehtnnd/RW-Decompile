using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Eating : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyPartTagDef tag = BodyPartTagDefOf.EatingSource;
			float arg_4E_0 = PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
			tag = BodyPartTagDefOf.EatingPathway;
			float maximum = 1f;
			return arg_4E_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, maximum, default(FloatRange), impactors, -1f) * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.EatingSource);
		}
	}
}
