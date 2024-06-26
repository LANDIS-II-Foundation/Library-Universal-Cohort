//  Authors:  Robert M. Scheller, James B. Domingo

using System;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// Methods for biomass cohorts.
    /// </summary>
    public static class Cohorts
    {
        private static int successionTimeStep;
        private static ICalculator biomassCalculator;

        //---------------------------------------------------------------------

        /// <summary>
        /// The succession time step used by biomass cohorts.
        /// </summary>
        public static int SuccessionTimeStep
        {
            get {
                return successionTimeStep;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The calculator for computing how a cohort's biomass changes.
        /// </summary>
        public static ICalculator BiomassCalculator
        {
            get {
                return biomassCalculator;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the biomass-cohorts module.
        /// </summary>
        /// <param name="successionTimeStep">
        /// The time step for the succession extension.  Unit: years
        /// </param>
        /// <param name="biomassCalculator">
        /// The calculator for computing the change in a cohort's biomass due
        /// to growth and mortality.
        /// </param>
        public static void Initialize(int         successionTimeStep,
                                      ICalculator biomassCalculator)
        {
            Cohorts.successionTimeStep = successionTimeStep;
            Cohorts.biomassCalculator  = biomassCalculator;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total biomass for all the cohorts at a site.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts)
        {
            int youngBiomass;
            return ComputeBiomass(siteCohorts, out youngBiomass);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total biomass for all the cohorts at a site, and the
        /// total biomass for all the young cohorts.
        /// </summary>
        public static int ComputeBiomass(ISiteCohorts siteCohorts,
                                         out int      youngBiomass)
        {
            youngBiomass = 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in siteCohorts) {
                foreach (ICohort cohort in speciesCohorts) {
                    totalBiomass += cohort.Data.Biomass;
                    if (cohort.Data.Age < successionTimeStep)
                        youngBiomass += cohort.Data.Biomass;
                }
            }
            return totalBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the total biomass for all the cohorts, not including young cohorts.
        /// </summary>
        public static int ComputeNonYoungBiomass(ISiteCohorts siteCohorts)
        {
            if (siteCohorts == null)
                return 0;
            int totalBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in siteCohorts) {
                foreach (ICohort cohort in speciesCohorts) {
                    if (cohort.Data.Age >= successionTimeStep)
                        totalBiomass += cohort.Data.Biomass;
                }
            }
            return totalBiomass;
        }


    }
}
