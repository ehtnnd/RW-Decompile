using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public class PatchOperationFindMod : PatchOperation
	{
		private List<string> mods;

		private PatchOperation match;

		private PatchOperation nomatch;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool flag = false;
			for (int i = 0; i < this.mods.Count; i++)
			{
				if (ModLister.GetModWithIdentifier(this.mods[i]) != null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (this.match != null)
				{
					return this.match.Apply(xml);
				}
			}
			else if (this.nomatch != null)
			{
				return this.nomatch.Apply(xml);
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0}({1})", base.ToString(), GenText.ToCommaList(this.mods, false));
		}
	}
}
