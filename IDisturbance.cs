// Copyright:  The LANDIS-II Foundation
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// A disturbance that damages cohorts thereby reducing their biomass.
    /// </summary>
    public interface IDisturbance
    {
        /// <summary>
        /// The disturbance's type.
        /// </summary>
        ExtensionType Type
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The current site that the disturbance is damaging.
        /// </summary>
        ActiveSite CurrentSite
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes how a cohort is damaged by the disturbance.
        /// </summary>
        /// <returns>
        /// The FRACTION of biomass that the cohort's biomass is to be reduced
        /// by.
        /// </returns>
        double ReduceOrKillMarkedCohort(ICohort cohort);
        

    }
}
