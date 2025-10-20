using kmbf.Blueprint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kmbf
{
    static class FixBuffRefs
    {
        // GUIDs taken from CotW. Should not run together
        public static readonly BlueprintBuffGuid ShatterDefensesHit = new("843741b85d8249b9acdcffb042015f06");
        public static readonly BlueprintBuffGuid ShatterDefensesAppliedThisRound = new("cf3e721e93044a21b87692526b3c45e3");
    }

    static class FixFeatureRefs
    {
        public static readonly BlueprintFeatureGuid HalflingWeaponFamiliarity = new("02a08242dd3b488bb21f6b93a42f1f66");
    }
}
