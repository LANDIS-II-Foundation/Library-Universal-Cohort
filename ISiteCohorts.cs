//  Copyright The LANDIS-II Foundation
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Library.UniversalCohorts
{
	/// <summary>
	/// All the cohorts at a site.
	/// </summary>
	public interface ISiteCohorts
		: IISiteCohorts<ISpeciesCohorts>
    {
		//---------------------------------------------------------------------
		/// <summary>
		/// Computes who much a disturbance damages the cohorts by reducing
		/// their biomass.
		/// </summary>
		/// <returns>
		/// The total of all the cohorts' biomass reductions.
		/// </returns>
		void ReduceOrKillCohorts(IDisturbance disturbance);
		//---------------------------------------------------------------------
		/// <summary>
		/// Removes the cohorts which are damaged by a disturbance.
		/// </summary>
		void RemoveMarkedCohorts(ICohortDisturbance disturbance);
		//---------------------------------------------------------------------
		/// <summary>
		/// Removes the cohorts which are damaged by a disturbance.
		/// </summary>
		void RemoveMarkedCohorts(ISpeciesCohortsDisturbance disturbance);
		//---------------------------------------------------------------------

		/// <summary>
		/// Removes the cohorts which are damaged by a disturbance.
		/// </summary>
		void Grow(ActiveSite site, bool isSuccessionTimestep, bool annualTimestep);
		//---------------------------------------------------------------------
	}
}
