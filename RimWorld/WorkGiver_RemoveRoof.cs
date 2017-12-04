using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RemoveRoof : WorkGiver_Scanner
	{
		public override bool Prioritized
		{
			get
			{
				return true;
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			return pawn.Map.areaManager.NoRoof.ActiveCells;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			if (!pawn.Map.areaManager.NoRoof[c])
			{
				return false;
			}
			if (!c.Roofed(pawn.Map))
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			LocalTargetInfo target = c;
			ReservationLayerDef ceiling = ReservationLayerDefOf.Ceiling;
			return pawn.CanReserve(target, 1, -1, ceiling, false);
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			return new Job(JobDefOf.RemoveRoof, c, c);
		}

		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			IntVec3 cell = t.Cell;
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = cell + GenAdj.AdjacentCells[i];
				if (c.InBounds(t.Map))
				{
					Building edifice = c.GetEdifice(t.Map);
					if (edifice != null && edifice.def.holdsRoof)
					{
						return -60f;
					}
					if (c.Roofed(pawn.Map))
					{
						num++;
					}
				}
			}
			return (float)(-(float)Mathf.Min(num, 3));
		}
	}
}
