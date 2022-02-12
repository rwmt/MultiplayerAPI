using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Multiplayer.API
{
    /// <summary>
    /// A record storing an object which holds a reference to <see cref="ThingFilter"/>
    /// </summary>
    public abstract record ThingFilterContext : ISyncSimple
    {
        /// <summary>
        /// Current state of the filter
        /// </summary>
        public abstract ThingFilter Filter { get; }
        /// <summary>
        /// Global/parent/default value of the filter
        /// </summary>
        public abstract ThingFilter ParentFilter { get; }
        /// <summary>
        /// Collection of all filters which are not visible to the user
        /// </summary>
        public virtual IEnumerable<SpecialThingFilterDef> HiddenFilters { get => null; }

        public void AllowStuffCat_Helper(StuffCategoryDef cat, bool allow)
        {
            Filter.SetAllow(cat, allow);
        }

        public void AllowSpecial_Helper(SpecialThingFilterDef sfDef, bool allow)
        {
            Filter.SetAllow(sfDef, allow);
        }

        public void AllowThing_Helper(ThingDef thingDef, bool allow)
        {
            Filter.SetAllow(thingDef, allow);
        }

        public void DisallowAll_Helper()
        {
            Filter.SetDisallowAll(null, HiddenFilters);
        }

        public void AllowAll_Helper()
        {
            Filter.SetAllowAll(ParentFilter);
        }

        public void AllowCategory_Helper(ThingCategoryDef categoryDef, bool allow)
        {
            var node = new TreeNode_ThingCategory(categoryDef);

            Filter.SetAllow(
                categoryDef,
                allow,
                null,
                Listing_TreeThingFilter
                    .CalculateHiddenSpecialFilters(node, ParentFilter)
                    .ConcatIfNotNull(HiddenFilters)
            );
        }
    }
}
