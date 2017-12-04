using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class SituationalThoughtHandler
	{
		private class CachedSocialThoughts
		{
			public List<Thought_SituationalSocial> thoughts = new List<Thought_SituationalSocial>();

			public List<Thought_SituationalSocial> activeThoughts = new List<Thought_SituationalSocial>();

			public int lastRecalculationTick = -99999;

			public int lastQueryTick = -99999;

			private const int ExpireAfterTicks = 300;

			public bool Expired
			{
				get
				{
					return Find.TickManager.TicksGame - this.lastQueryTick >= 300;
				}
			}

			public bool ShouldRecalculateState
			{
				get
				{
					return Find.TickManager.TicksGame - this.lastRecalculationTick >= 100;
				}
			}
		}

		public Pawn pawn;

		private List<Thought_Situational> cachedThoughts = new List<Thought_Situational>();

		private int lastMoodThoughtsRecalculationTick = -99999;

		private Dictionary<Pawn, SituationalThoughtHandler.CachedSocialThoughts> cachedSocialThoughts = new Dictionary<Pawn, SituationalThoughtHandler.CachedSocialThoughts>();

		private Dictionary<Pawn, SituationalThoughtHandler.CachedSocialThoughts> cachedSocialThoughtsAffectingMood = new Dictionary<Pawn, SituationalThoughtHandler.CachedSocialThoughts>();

		private const int RecalculateStateEveryTicks = 100;

		private HashSet<ThoughtDef> tmpCachedThoughts = new HashSet<ThoughtDef>();

		private HashSet<Pair<ThoughtDef, Pawn>> tmpToAdd = new HashSet<Pair<ThoughtDef, Pawn>>();

		private HashSet<ThoughtDef> tmpCachedSocialThoughts = new HashSet<ThoughtDef>();

		public SituationalThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void SituationalThoughtInterval()
		{
			this.RemoveExpiredThoughtsFromCache();
		}

		public void AppendMoodThoughts(List<Thought> outThoughts)
		{
			this.CheckRecalculateMoodThoughts();
			for (int i = 0; i < this.cachedThoughts.Count; i++)
			{
				Thought_Situational thought_Situational = this.cachedThoughts[i];
				if (thought_Situational.Active)
				{
					outThoughts.Add(thought_Situational);
				}
			}
			int ticksGame = Find.TickManager.TicksGame;
			foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> current in this.cachedSocialThoughtsAffectingMood)
			{
				current.Value.lastQueryTick = ticksGame;
				List<Thought_SituationalSocial> activeThoughts = current.Value.activeThoughts;
				for (int j = 0; j < activeThoughts.Count; j++)
				{
					outThoughts.Add(activeThoughts[j]);
				}
			}
		}

		public void AppendSocialThoughts(Pawn otherPawn, List<ISocialThought> outThoughts)
		{
			this.CheckRecalculateSocialThoughts(otherPawn);
			SituationalThoughtHandler.CachedSocialThoughts cachedSocialThoughts = this.cachedSocialThoughts[otherPawn];
			cachedSocialThoughts.lastQueryTick = Find.TickManager.TicksGame;
			List<Thought_SituationalSocial> activeThoughts = cachedSocialThoughts.activeThoughts;
			for (int i = 0; i < activeThoughts.Count; i++)
			{
				outThoughts.Add(activeThoughts[i]);
			}
		}

		private void CheckRecalculateMoodThoughts()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame - this.lastMoodThoughtsRecalculationTick < 100)
			{
				return;
			}
			this.lastMoodThoughtsRecalculationTick = ticksGame;
			try
			{
				this.tmpCachedThoughts.Clear();
				for (int i = 0; i < this.cachedThoughts.Count; i++)
				{
					this.cachedThoughts[i].RecalculateState();
					this.tmpCachedThoughts.Add(this.cachedThoughts[i].def);
				}
				List<ThoughtDef> situationalNonSocialThoughtDefs = ThoughtUtility.situationalNonSocialThoughtDefs;
				int j = 0;
				int count = situationalNonSocialThoughtDefs.Count;
				while (j < count)
				{
					if (!this.tmpCachedThoughts.Contains(situationalNonSocialThoughtDefs[j]))
					{
						Thought_Situational thought_Situational = this.TryCreateThought(situationalNonSocialThoughtDefs[j]);
						if (thought_Situational != null)
						{
							this.cachedThoughts.Add(thought_Situational);
						}
					}
					j++;
				}
				this.RecalculateSocialThoughtsAffectingMood();
			}
			finally
			{
			}
		}

		private void RecalculateSocialThoughtsAffectingMood()
		{
			try
			{
				this.tmpToAdd.Clear();
				List<ThoughtDef> situationalSocialThoughtDefs = ThoughtUtility.situationalSocialThoughtDefs;
				int i = 0;
				int count = situationalSocialThoughtDefs.Count;
				while (i < count)
				{
					if (situationalSocialThoughtDefs[i].socialThoughtAffectingMood)
					{
						foreach (Pawn current in situationalSocialThoughtDefs[i].Worker.PotentialPawnCandidates(this.pawn))
						{
							if (current != this.pawn)
							{
								this.tmpToAdd.Add(new Pair<ThoughtDef, Pawn>(situationalSocialThoughtDefs[i], current));
							}
						}
					}
					i++;
				}
				foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> current2 in this.cachedSocialThoughtsAffectingMood)
				{
					List<Thought_SituationalSocial> thoughts = current2.Value.thoughts;
					for (int j = thoughts.Count - 1; j >= 0; j--)
					{
						if (!this.tmpToAdd.Contains(new Pair<ThoughtDef, Pawn>(thoughts[j].def, current2.Key)))
						{
							thoughts.RemoveAt(j);
						}
					}
				}
				foreach (Pair<ThoughtDef, Pawn> current3 in this.tmpToAdd)
				{
					ThoughtDef first = current3.First;
					Pawn second = current3.Second;
					SituationalThoughtHandler.CachedSocialThoughts cachedSocialThoughts;
					bool flag = this.cachedSocialThoughtsAffectingMood.TryGetValue(second, out cachedSocialThoughts);
					if (flag)
					{
						bool flag2 = false;
						for (int k = 0; k < cachedSocialThoughts.thoughts.Count; k++)
						{
							if (cachedSocialThoughts.thoughts[k].def == first)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							continue;
						}
					}
					Thought_SituationalSocial thought_SituationalSocial = this.TryCreateSocialThought(first, second);
					if (thought_SituationalSocial != null)
					{
						if (!flag)
						{
							cachedSocialThoughts = new SituationalThoughtHandler.CachedSocialThoughts();
							this.cachedSocialThoughtsAffectingMood.Add(second, cachedSocialThoughts);
						}
						cachedSocialThoughts.thoughts.Add(thought_SituationalSocial);
					}
				}
				this.cachedSocialThoughtsAffectingMood.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> x) => x.Value.thoughts.Count == 0);
				int ticksGame = Find.TickManager.TicksGame;
				foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> current4 in this.cachedSocialThoughtsAffectingMood)
				{
					SituationalThoughtHandler.CachedSocialThoughts value = current4.Value;
					List<Thought_SituationalSocial> thoughts2 = value.thoughts;
					value.activeThoughts.Clear();
					for (int l = 0; l < thoughts2.Count; l++)
					{
						thoughts2[l].RecalculateState();
						value.lastRecalculationTick = ticksGame;
						if (thoughts2[l].Active)
						{
							value.activeThoughts.Add(thoughts2[l]);
						}
					}
				}
			}
			finally
			{
			}
		}

		private void CheckRecalculateSocialThoughts(Pawn otherPawn)
		{
			try
			{
				SituationalThoughtHandler.CachedSocialThoughts cachedSocialThoughts;
				if (!this.cachedSocialThoughts.TryGetValue(otherPawn, out cachedSocialThoughts))
				{
					cachedSocialThoughts = new SituationalThoughtHandler.CachedSocialThoughts();
					this.cachedSocialThoughts.Add(otherPawn, cachedSocialThoughts);
				}
				if (cachedSocialThoughts.ShouldRecalculateState)
				{
					cachedSocialThoughts.lastRecalculationTick = Find.TickManager.TicksGame;
					this.tmpCachedSocialThoughts.Clear();
					for (int i = 0; i < cachedSocialThoughts.thoughts.Count; i++)
					{
						Thought_SituationalSocial thought_SituationalSocial = cachedSocialThoughts.thoughts[i];
						thought_SituationalSocial.RecalculateState();
						this.tmpCachedSocialThoughts.Add(thought_SituationalSocial.def);
					}
					List<ThoughtDef> situationalSocialThoughtDefs = ThoughtUtility.situationalSocialThoughtDefs;
					int j = 0;
					int count = situationalSocialThoughtDefs.Count;
					while (j < count)
					{
						if (!this.tmpCachedSocialThoughts.Contains(situationalSocialThoughtDefs[j]))
						{
							Thought_SituationalSocial thought_SituationalSocial2 = this.TryCreateSocialThought(situationalSocialThoughtDefs[j], otherPawn);
							if (thought_SituationalSocial2 != null)
							{
								cachedSocialThoughts.thoughts.Add(thought_SituationalSocial2);
							}
						}
						j++;
					}
					cachedSocialThoughts.activeThoughts.Clear();
					for (int k = 0; k < cachedSocialThoughts.thoughts.Count; k++)
					{
						Thought_SituationalSocial thought_SituationalSocial3 = cachedSocialThoughts.thoughts[k];
						if (thought_SituationalSocial3.Active)
						{
							cachedSocialThoughts.activeThoughts.Add(thought_SituationalSocial3);
						}
					}
				}
			}
			finally
			{
			}
		}

		private Thought_Situational TryCreateThought(ThoughtDef def)
		{
			Thought_Situational thought_Situational = null;
			try
			{
				if (!ThoughtUtility.CanGetThought(this.pawn, def))
				{
					Thought_Situational result = null;
					return result;
				}
				if (!def.Worker.CurrentState(this.pawn).Active)
				{
					Thought_Situational result = null;
					return result;
				}
				thought_Situational = (Thought_Situational)ThoughtMaker.MakeThought(def);
				thought_Situational.pawn = this.pawn;
				thought_Situational.RecalculateState();
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception while recalculating ",
					def,
					" thought state for pawn ",
					this.pawn,
					": ",
					ex
				}));
			}
			return thought_Situational;
		}

		private Thought_SituationalSocial TryCreateSocialThought(ThoughtDef def, Pawn otherPawn)
		{
			Thought_SituationalSocial thought_SituationalSocial = null;
			try
			{
				if (!ThoughtUtility.CanGetThought(this.pawn, def))
				{
					Thought_SituationalSocial result = null;
					return result;
				}
				if (!def.Worker.CurrentSocialState(this.pawn, otherPawn).Active)
				{
					Thought_SituationalSocial result = null;
					return result;
				}
				thought_SituationalSocial = (Thought_SituationalSocial)ThoughtMaker.MakeThought(def);
				thought_SituationalSocial.pawn = this.pawn;
				thought_SituationalSocial.otherPawn = otherPawn;
				thought_SituationalSocial.RecalculateState();
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception while recalculating ",
					def,
					" thought state for pawn ",
					this.pawn,
					": ",
					ex
				}));
			}
			return thought_SituationalSocial;
		}

		public void Notify_SituationalThoughtsDirty()
		{
			this.cachedThoughts.Clear();
			this.cachedSocialThoughts.Clear();
			this.cachedSocialThoughtsAffectingMood.Clear();
		}

		private void RemoveExpiredThoughtsFromCache()
		{
			this.cachedSocialThoughts.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
			this.cachedSocialThoughtsAffectingMood.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
		}
	}
}
