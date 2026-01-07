//  Copyright The LANDIS-II Foundation
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;

using log4net;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// The cohorts for a particular species at a site.
    /// </summary>
    public class SpeciesCohorts
        : ISpeciesCohorts
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        //---------------------------------------------------------------------

        private ISpecies species;
        private bool isMaturePresent;

        //  Cohort data is in oldest to youngest order.
        private List<CohortData> cohortData;

        //---------------------------------------------------------------------

        public int Count
        {
            get {
                return cohortData.Count;
            }
        }

        //---------------------------------------------------------------------

        public ISpecies Species
        {
            get {
                return species;
            }
        }

        //---------------------------------------------------------------------

        public bool IsMaturePresent
        {
            get {
                return isMaturePresent;
            }
        }

        //---------------------------------------------------------------------

        public ICohort this[int index]
        {
            get {
                return new Cohort(species, cohortData[index], cohortData[index].AdditionalParameters);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// An iterator from the oldest cohort to the youngest.
        /// </summary>
        public OldToYoungIterator OldToYoung
        {
            get {
                return new OldToYoungIterator(this);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance with one young cohort (age = 1).
        /// </summary>
        public SpeciesCohorts(ISpecies species,
                              ushort initialAge,
                              int   initialBiomass,
                              ExpandoObject parametersToAdd)
        {
            this.species = species;
            this.cohortData = new List<CohortData>();
            this.isMaturePresent = false;
            AddNewCohort(initialAge, initialBiomass, parametersToAdd);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance with one young cohort (age = 1).
        /// </summary>
        public SpeciesCohorts(ISpecies species,
                              ushort initialAge,
                              int initialBiomass, double initialANPP,
                              ExpandoObject parametersToAdd)
        {
            this.species = species;
            this.cohortData = new List<CohortData>();
            this.isMaturePresent = false;
            AddNewCohort(initialAge, initialBiomass, initialANPP, parametersToAdd);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a copy of a species' cohorts.
        /// </summary>
        public SpeciesCohorts Clone()
        {
            SpeciesCohorts clone = new SpeciesCohorts(this.species);
            clone.cohortData = new List<CohortData>(this.cohortData);
            clone.isMaturePresent = this.isMaturePresent;
            return clone;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance with no cohorts.
        /// </summary>
        /// <remarks>
        /// Private constructor used by Clone method.
        /// </remarks>
        private SpeciesCohorts(ISpecies species)
        {
            this.species = species;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Adds a new cohort.
        /// </summary>
        public void AddNewCohort(ushort age, int initialBiomass, ExpandoObject parametersToAdd)
        {
            this.cohortData.Add(new CohortData(age, initialBiomass, parametersToAdd));
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Adds a new cohort.
        /// </summary>
        public void AddNewCohort(ushort age, int initialBiomass, double initialANPP, ExpandoObject parametersToAdd)
        {
            this.cohortData.Add(new CohortData(age, initialBiomass, initialANPP, parametersToAdd));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Gets the age of a cohort at a specific index.
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException">
        /// </exception>
        public int GetAge(int index)
        {
            return cohortData[index].Age;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Combines all young cohorts into a single cohort whose age is the
        /// succession timestep - 1 and whose biomass is the sum of all the
        /// biomasses of the young cohorts.
        /// </summary>
        /// <remarks>
        /// The age of the combined cohort is set to the succession timestep -
        /// 1 so that when the combined cohort undergoes annual growth, its
        /// age will end up at the succession timestep.
        /// <p>
        /// For this method, young cohorts are those whose age is less than or
        /// equal to the succession timestep.  We include the cohort whose age
        /// is equal to the timestep because such a cohort is generated when
        /// reproduction occurs during a succession timestep.
        /// </remarks>
        public void CombineYoungCohorts()
        {
            //  Work from the end of cohort data since the array is in old-to-
            //  young order.
            int youngCount = 0;
            int totalBiomass = 0;
            double totalANPP = 0;
            ExpandoObject extraParms = new ExpandoObject();
            for (int i = cohortData.Count - 1; i >= 0; i--) {
                CohortData data = cohortData[i];
                if (data.Age <= Cohorts.SuccessionTimeStep) {
                    youngCount++;
                    totalBiomass += data.Biomass;
                    totalANPP += data.ANPP;
                    CombineAdditionalParameters(extraParms, data.AdditionalParameters);
                }
                else
                    break;
            }

            if (youngCount > 0) {
                cohortData.RemoveRange(cohortData.Count - youngCount, youngCount);
                cohortData.Add(new CohortData((ushort) (Cohorts.SuccessionTimeStep - 1),totalBiomass, totalANPP, extraParms));
            }
        }
        //---------------------------------------------------------------------

        private void CombineAdditionalParameters(ExpandoObject extraParms, ExpandoObject additionalParameters)
        {
            IDictionary<string, object> tempObject = extraParms;

            foreach (var parameter in additionalParameters)
            {
                if (!tempObject.ContainsKey(parameter.Key))
                {
                    tempObject.Add(parameter.Key, parameter.Value);
                }
                else
                {
                    if (parameter.Value is sbyte)
                    {
                        tempObject[parameter.Key] = (sbyte)(tempObject[parameter.Key]) + (sbyte)(parameter.Value);
                    }
                    else if (parameter.Value is byte)
                    {
                        tempObject[parameter.Key] = (byte)(tempObject[parameter.Key]) + (byte)(parameter.Value);
                    }
                    else if (parameter.Value is short)
                    {
                        tempObject[parameter.Key] = (short)(tempObject[parameter.Key]) + (short)(parameter.Value);
                    }
                    else if (parameter.Value is ushort)
                    {
                        tempObject[parameter.Key] = (ushort)(tempObject[parameter.Key]) + (ushort)(parameter.Value);
                    }
                    else if (parameter.Value is int)
                    {
                        tempObject[parameter.Key] = (int)(tempObject[parameter.Key]) + (int)(parameter.Value);
                    }
                    else if (parameter.Value is uint)
                    {
                        tempObject[parameter.Key] = (uint)(tempObject[parameter.Key]) + (uint)(parameter.Value);
                    }
                    else if (parameter.Value is long)
                    {
                        tempObject[parameter.Key] = (long)(tempObject[parameter.Key]) + (long)(parameter.Value);
                    }
                    else if (parameter.Value is ulong)
                    {
                        tempObject[parameter.Key] = (ulong)(tempObject[parameter.Key]) + (ulong)(parameter.Value);
                    }
                    else if (parameter.Value is float)
                    {
                        tempObject[parameter.Key] = (float)(tempObject[parameter.Key]) + (float)(parameter.Value);
                    }
                    else if (parameter.Value is double)
                    {
                        tempObject[parameter.Key] = (double)(tempObject[parameter.Key]) + (double)(parameter.Value);
                    }
                    else if (parameter.Value is decimal)
                    {
                        tempObject[parameter.Key] = (decimal)(tempObject[parameter.Key]) + (decimal)(parameter.Value);
                    }
                    else if (parameter.Value is bool)
                    {
                        tempObject[parameter.Key] = (bool)(tempObject[parameter.Key]) || (bool)(parameter.Value);
                    }
                    else if (parameter.Value is char || parameter.Value is string)
                    {
                        tempObject[parameter.Key] = parameter.Value;
                    }
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Grows an individual cohort for a year, incrementing its age by 1
        /// and updating its biomass for annual growth and mortality.
        /// </summary>
        /// <param name="index">
        /// The index of the cohort to grow; it must be between 0 and Count - 1.
        /// </param>
        /// <param name="site">
        /// The site where the species' cohorts are located.
        /// </param>
        /// <param name="annualTimestep">
        /// Whether or not this timestep is a new year
        /// </param>
        /// </param>
        /// <returns>
        /// The index of the next younger cohort.  Note this may be the same
        /// as the index passed in if that cohort dies due to senescence.
        /// </returns>
        public int GrowCohort(int index,
                              ActiveSite site,
                              bool annualTimestep = false)
        {
            Debug.Assert(0 <= index && index <= cohortData.Count);
            Debug.Assert(site != null);

            Cohort cohort = new Cohort(species, cohortData[index], cohortData[index].AdditionalParameters);
            //Debug.Assert(cohort.Biomass <= siteBiomass);

            if (isDebugEnabled)
                log.DebugFormat("  grow cohort: {0}, {1} yrs, {2} Mg/ha",
                                cohort.Species.Name, cohort.Age, cohort.Biomass);

            //  Check for senescence
            if (cohort.Age >= species.Longevity) {
                RemoveCohort(index, cohort, site, null);
                return index;
            }

            if (annualTimestep)
                cohort.IncrementAge();

            ExpandoObject otherChanges = new ExpandoObject();

            int biomassChange = (int)Cohorts.BiomassCalculator.ComputeChange(cohort, site, out double ANPP, out otherChanges); //, siteBiomass, prevYearSiteMortality);

            Debug.Assert(-(cohort.Biomass) <= biomassChange);  // Cohort can't loss more biomass than it has

            cohort.ChangeBiomass(biomassChange);
            cohort.ChangeParameters(otherChanges);
            cohort.ChangeANPP(ANPP);

            if (cohort.Biomass > 0) {
                cohortData[index] = cohort.Data;
                return index + 1;
            }
            else {
                RemoveCohort(index, cohort, site, null);
                return index;
            }
        }

        //---------------------------------------------------------------------

        public void RemoveCohort(int        index,
                                  ICohort    cohort,
                                  ActiveSite site,
                                  ExtensionType disturbanceType)
        {
            if (isDebugEnabled)
                log.DebugFormat("  cohort removed: {0}, {1} yrs, {2} Mg/ha ({3})",
                                cohort.Species.Name, cohort.Data.Age, cohort.Data.Biomass,
                                disturbanceType != null
                                    ? disturbanceType.Name
                                    : cohort.Data.Age >= species.Longevity
                                        ? "senescence"
                                        : cohort.Data.Biomass == 0
                                            ? "attrition"
                                            : "UNKNOWN");

            cohortData.RemoveAt(index);
            Cohort.CohortMortality(this, cohort, site, disturbanceType, 1.0);
        }

        //---------------------------------------------------------------------
        private void ReduceCohort(
                          ICohort cohort,
                          ActiveSite site,
                          ExtensionType disturbanceType, double reduction)
        {
            Cohort.CohortMortality(this, cohort, site, disturbanceType, reduction);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Updates the IsMaturePresent property.
        /// </summary>
        /// <remarks>
        /// Should be called after all the species' cohorts have grown.
        /// </remarks>
        public void UpdateMaturePresent()
        {
            isMaturePresent = false;
            for (int i = 0; i < cohortData.Count; i++) {
                if (cohortData[i].Age >= species.Maturity) {
                    isMaturePresent = true;
                    break;
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes how much a disturbance damages the cohorts by reducing
        /// their biomass.
        /// </summary>
        /// <returns>
        /// The total of all the cohorts' biomass reductions.
        /// </returns>
        public double MarkCohorts(IDisturbance disturbance)
        {
            //  Go backwards through list of cohort data, so the removal of an
            //  item doesn't mess up the loop.
            isMaturePresent = false;
            int totalReductionBiomass = 0;
            for (int i = cohortData.Count - 1; i >= 0; i--) {
                Cohort cohort = new Cohort(species, cohortData[i], cohortData[i].AdditionalParameters);
                double fractionReduction = disturbance.ReduceOrKillMarkedCohort(cohort);
                //Console.WriteLine("  Reduction: {0}, {1} yrs, {2} Mg/ha, reduction={3}", cohort.Species.Name, cohort.Age, cohort.Biomass, reduction);
                if (fractionReduction > 0.0) {
                    int cohortBiomassReduction = (int) fractionReduction * cohort.Biomass;
                    totalReductionBiomass += cohortBiomassReduction;
                    if (cohortBiomassReduction < cohort.Biomass) {
                        ReduceCohort(cohort, disturbance.CurrentSite, disturbance.Type, fractionReduction);
                        cohort.ChangeBiomass(-cohortBiomassReduction);
                        cohortData[i] = cohort.Data;
                        //Console.WriteLine("  Partial Reduction: {0}, {1} yrs, {2} Mg/ha", cohort.Species.Name, cohort.Age, cohort.Biomass);
                    }
                    else {
                        RemoveCohort(i, cohort, disturbance.CurrentSite,
                                     disturbance.Type);
                        cohort = null;
                    }
                }
                if (cohort != null && cohort.Age >= species.Maturity)
                    isMaturePresent = true;
            }
            return totalReductionBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes current forage for a cohort
        /// </summary>
        /// <returns>
        /// </returns>
        /*public int UpdateForage(IDisturbance disturbance)
        {
            //  Go backwards through list of cohort data, so the removal of an
            //  item doesn't mess up the loop.
            int totalForage = 0;
            for (int i = cohortData.Count - 1; i >= 0; i--)
            {
                Cohort cohort = new Cohort(species, cohortData[i]);
                int forage = disturbance.ChangeForage(cohort);
                cohort.ChangeForage(forage);
                cohortData[i] = cohort.Data;
                totalForage += forage;
            }
            return totalForage;
        }*/
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes current forage in reach for a cohort
        /// </summary>
        /// <returns>
        /// </returns>
        /*public int UpdateForageInReach(IDisturbance disturbance)
        {
            //  Go backwards through list of cohort data, so the removal of an
            //  item doesn't mess up the loop.
            int totalForageInReach = 0;
            for (int i = cohortData.Count - 1; i >= 0; i--)
            {
                Cohort cohort = new Cohort(species, cohortData[i]);
                int forageInReach = disturbance.ChangeForageInReach(cohort);
                cohort.ChangeForageInReach(forageInReach);
                cohortData[i] = cohort.Data;
                totalForageInReach += forageInReach;
            }
            return totalForageInReach;
        }*/
        //---------------------------------------------------------------------
        /// <summary>
        /// Computes last browse prop for a cohort
        /// </summary>
        /// <returns>
        /// </returns>
        /*public double UpdateLastBrowseProp(IDisturbance disturbance)
        {
            //  Go backwards through list of cohort data, so the removal of an
            //  item doesn't mess up the loop.
            double totalBrowseProp = 0;
            for (int i = cohortData.Count - 1; i >= 0; i--)
            {
                Cohort cohort = new Cohort(species, cohortData[i]);
                double lastBrowseProp = disturbance.ChangeLastBrowseProp(cohort);
                cohort.ChangeLastBrowseProp(lastBrowseProp);
                cohortData[i] = cohort.Data;
                totalBrowseProp += lastBrowseProp;
            }
            return totalBrowseProp;
        }*/
        //---------------------------------------------------------------------

        private static SpeciesCohortBoolArray isSpeciesCohortDamaged;

        //---------------------------------------------------------------------

        static SpeciesCohorts()
        {
            isSpeciesCohortDamaged = new SpeciesCohortBoolArray();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Removes the cohorts that are completed removed by disturbance.
        /// </summary>
        /// <returns>
        /// The total biomass of all the cohorts damaged by the disturbance.
        /// </returns>
        public int MarkCohorts(ISpeciesCohortsDisturbance disturbance)
        {
            isSpeciesCohortDamaged.SetAllFalse(Count);
            disturbance.MarkCohortsForDeath(this, isSpeciesCohortDamaged);

            //  Go backwards through list of cohort data, so the removal of an
            //  item doesn't mess up the loop.
            isMaturePresent = false;
            int totalReduction = 0;
            for (int i = cohortData.Count - 1; i >= 0; i--) {
                if (isSpeciesCohortDamaged[i]) {
                    Cohort cohort = new Cohort(species, cohortData[i], cohortData[i].AdditionalParameters);
                    totalReduction += cohort.Biomass;
                    RemoveCohort(i, cohort, disturbance.CurrentSite,
                                 disturbance.Type);
                    Cohort.KilledByAgeOnlyDisturbance(this, cohort, disturbance.CurrentSite, disturbance.Type);
                    
                    cohort = null;
                }
                else if (cohortData[i].Age >= species.Maturity)
                    isMaturePresent = true;
            }
            return totalReduction;
        }

        //---------------------------------------------------------------------

        IEnumerator<ICohort> IEnumerable<ICohort>.GetEnumerator()
        {
            //Console.Out.WriteLine("Itor 1");
            foreach (CohortData data in cohortData)
                yield return new Cohort(species, data, data.AdditionalParameters);
        }

        //---------------------------------------------------------------------

        IEnumerator IEnumerable.GetEnumerator()
        {
            //Console.Out.WriteLine("Itor 2");
            return ((IEnumerable<ICohort>)this).GetEnumerator();
        }

        //---------------------------------------------------------------------
    }
}
