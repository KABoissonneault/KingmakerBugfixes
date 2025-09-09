//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace kmbf.Component
{
    // Unit Part base for parts that add buffs to a unit, then removes itself from the unit if all the buffs have been removed
    // This allows adding a feature from multiple sources, while making sure the feature is only removed once all sources have been removed
    // Taken from Call of the Wild
    public class AdditiveUnitPart<PartT> : UnitPart
        where PartT : AdditiveUnitPart<PartT>
    {
        [JsonProperty]
        protected List<Fact> storedSources = new List<Fact>();

        public virtual void AddSourceFact(Fact fact)
        {
            if (!storedSources.Contains(fact))
                storedSources.Add(fact);
        }

        public virtual void RemoveSourceFact(Fact fact)
        {
            storedSources.Remove(fact);
            if(storedSources.Empty())
            {
                Owner.Remove<PartT>();
            }
        }
    }
}
