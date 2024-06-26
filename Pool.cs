﻿//  Authors:  Robert M. Scheller, James B. Domingo

using System;

namespace Landis.Library.UniversalCohorts
{
    /// <summary>
    /// The pool of dead biomass at a site.
    /// </summary>
    public class Pool
    {
        private double mass;
        private double decayValue;
        private double initialMass;


        //---------------------------------------------------------------------
        /// <summary>
        /// Dead pool with initial values set to 0
        /// </summary>
        public Pool()
        {
            this.mass = 0;
            this.decayValue = 0.0;
            this.initialMass = 0.0;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The litter pool's biomass.
        /// </summary>
        public double Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool decay rate.
        /// </summary>
        public double DecayValue
        {
            get
            {
                return decayValue;
            }
            set
            {
                decayValue = value;
            }
        }

        /// <summary>
        /// Initial mass of litter pool.
        /// </summary>
        public double InitialMass
        {
            get
            {
                return initialMass;
            }
            set
            {
                initialMass = value;
            }
        }
        /// <summary>
        /// Clone a copy of the pool
        /// </summary>
        /// <returns></returns>
        public Pool Clone()
        {
            Pool newPool = new Pool();

            newPool.Mass = this.mass;
            newPool.DecayValue = this.decayValue;
            newPool.InitialMass = this.initialMass;
            return newPool;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds some dead biomass (and associated C/N/P contents) to the pool.
        /// </summary>
        /// <remarks>
        /// The pool's decomposition rate is adjusted by computing a weighted
        /// average of the its current decay rate and the decay rate 
        /// associated with the incoming biomass.
        /// </remarks>
        public void AddMass(
                            double inputMass,
                            double inputDecayValue)
        {

            double totalBiomass = (Mass + inputMass);
            if (totalBiomass == 0)
            {
                DecayValue = 0;
            }
            else
            {
                DecayValue = ((Mass * DecayValue) + (inputMass * inputDecayValue)) /
                            totalBiomass;
            }
            Mass = totalBiomass;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Reduces the pool's biomass by a specified percentage.
        /// </summary>
        public double ReduceMass(double percentage)
        {
            if (percentage < 0.0 || percentage > 1.0)
                throw new ArgumentException("Percentage must be between 0% and 100%");
            double reduction = (mass * percentage);
            mass -= (mass * percentage);

            return reduction;
        }

        /// <summary>
        /// Decomposes the pool's biomass for a year.
        /// </summary>
      
	public double Decompose()
        {
            //if (PlugIn.CalibrateMode && mass > 0)
            //    PlugIn.ModelCore.UI.WriteLine("Pool mass = {0}.", mass);
            double oldmass = mass;
            double newmass  = (uint)(mass * Math.Exp(-decayValue));
            mass = newmass;
            return oldmass - newmass; 
        }

    }
}
