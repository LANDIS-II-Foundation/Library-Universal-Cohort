// Copyright:  The LANDIS-II Foundation
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;
using System;
using System.Dynamic;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// A species cohort with biomass information.
    /// </summary>
    public class Cohort
        : ICohort
    {
        public static class Delegates
        {
            public delegate CohortData ComputeCohortData(ushort age, int biomass, double anpp, ExpandoObject parametersToAdd);
        }

        private ISpecies species;
        private CohortData data;
        private static Delegates.ComputeCohortData computeCohortData = null;

        //---------------------------------------------------------------------

        /// <summary>
        /// The method that this class uses to add new young cohort for a
        /// particular species at a site.
        /// </summary>
        public static Delegates.ComputeCohortData ComputeCohortData
        {
            get
            {
                return computeCohortData;
            }
            set
            {
                Require.ArgumentNotNull(value);
                computeCohortData = value;
            }
        }

        //---------------------------------------------------------------------

        public virtual ISpecies Species
        {
            get 
            {
                return species;
            }
        }

        //---------------------------------------------------------------------

        public ExpandoObject AdditionalParameters
        {
            get
            {
                return data.AdditionalParameters;
            }
            set
            {
                data.AdditionalParameters = value;
            }
        }

        //---------------------------------------------------------------------

        public virtual ushort Age
        {
            get
            {
                return data.Age;
            }
            set
            {
                data.Age = value;
            }
        }

        //---------------------------------------------------------------------

        public virtual int Biomass
        {
            get 
            {
                return data.Biomass;
            }
            set
            {
                data.Biomass = value;
            }
        }

        //---------------------------------------------------------------------
        public virtual double ANPP
        {
            get
            {
                return data.ANPP;
            }
            set
            {
                data.ANPP = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The cohort's age and biomass data.
        /// </summary>
        public CohortData Data
        {
            get {
                return data;
            }
        }


        //---------------------------------------------------------------------

        public Cohort(ISpecies species,
                      ushort   age,
                      int   biomass,
                      ExpandoObject parametersToAdd)
        {
            this.species = species;

            if (computeCohortData != null)
            {
                this.data = computeCohortData(age, biomass, biomass, parametersToAdd);
            }
            else
            {
                this.data = new CohortData(age, biomass, parametersToAdd);
            }
        }
        //---------------------------------------------------------------------

        public Cohort()
        {

        }

        //---------------------------------------------------------------------

        public Cohort(ISpecies species,
                      ushort age,
                      int biomass,
                      int anpp,
                      ExpandoObject parametersToAdd)
        {
            this.species = species;

            if (computeCohortData != null)
            {
                this.data = computeCohortData(age, biomass, anpp, parametersToAdd);
            }
            else
            {
                this.data = new CohortData(age, biomass, anpp, parametersToAdd);
            }
        }

        //---------------------------------------------------------------------

        public Cohort(ISpecies   species,
                      CohortData cohortData,
                      ExpandoObject parametersToAdd)
        {
            this.species = species;

            if (computeCohortData != null)
            {
                this.data = computeCohortData(cohortData.Age, cohortData.Biomass, cohortData.ANPP, parametersToAdd);
            }
            else
            {
                this.data = cohortData;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Increments the cohort's age by one year.
        /// </summary>
        public virtual void IncrementAge()
        {
            data.Age += 1;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Changes the cohort's biomass.
        /// </summary>
        public virtual void ChangeBiomass(int delta)
        {
            int newBiomass = data.Biomass + delta;
            data.Biomass = System.Math.Max(0, newBiomass);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Sets the cohort's ANPP.
        /// </summary>
        public virtual void ChangeANPP(double anpp)
        {
            data.ANPP = Math.Max(0, anpp);
        }
        //---------------------------------------------------------------------

        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            Percentage nonWoodyPercentage = Cohorts.BiomassCalculator.ComputeNonWoodyPercentage(this, site);
            return (int) (data.Biomass * nonWoodyPercentage);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Occurs when a cohort dies either due to senescence or biomass
        /// disturbances.
        /// </summary>
        public static event MortalityEventHandler<MortalityEventArgs> MortalityEvent;
        //---------------------------------------------------------------------

        /// <summary>
        /// Raises a Cohort.DeathEvent if partial mortality.
        /// </summary>
        public static void CohortMortality(object sender,
                                ICohort cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType,
                                double fractionBiomassReduction)
        {
            if (MortalityEvent != null)
                MortalityEvent(sender, new MortalityEventArgs(cohort, site, disturbanceType, fractionBiomassReduction));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Occurs when a cohort is killed by an age-only disturbance.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> AgeOnlyDeathEvent;

        //---------------------------------------------------------------------

        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void KilledByAgeOnlyDisturbance(object     sender,
                                                      ICohort    cohort,
                                                      ActiveSite site,
                                                      ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
                AgeOnlyDeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        public void ChangeParameters(ExpandoObject additionalParams)
        {
            this.Data.ChangeParameters(additionalParams);
        }
    }
}
