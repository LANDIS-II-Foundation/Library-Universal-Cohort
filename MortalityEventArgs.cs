// Copyright:  The LANDIS-II Foundation
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// Information about a cohort's death.
    /// </summary>
    public class MortalityEventArgs
    {
        private ICohort cohort;
        private ActiveSite site;
        private ExtensionType disturbanceType;
        private double fractionBiomassReduction;

        //---------------------------------------------------------------------

        /// <summary>
        /// The cohort that died.
        /// </summary>
        public ICohort Cohort
        {
            get
            {
                return cohort;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site where the cohort died.
        /// </summary>
        public ActiveSite Site
        {
            get
            {
                return site;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The type of disturbance that killed the cohort.
        /// </summary>
        /// <remarks>
        /// null if the cohort died during the growth phase of succession.
        /// </remarks>
        public ExtensionType DisturbanceType
        {
            get
            {
                return disturbanceType;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The type of disturbance that killed the cohort.
        /// </summary>
        public double FractionBiomassReduction
        {
            get
            {
                return fractionBiomassReduction;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MortalityEventArgs(ICohort cohort,
                              ActiveSite site,
                              ExtensionType disturbanceType, double fractionBiomassReduction)
        {
            this.cohort = cohort;
            this.site = site;
            this.disturbanceType = disturbanceType;
            this.fractionBiomassReduction = fractionBiomassReduction;
        }
    }
}
