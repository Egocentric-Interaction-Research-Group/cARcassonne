using System;

namespace Carcassonne.Models
{
    public static class Extensions
    {
        public static bool HasCity(this Geography geography) => (geography & Geography.City) == Geography.City;
        public static bool HasRoad(this Geography geography) => (geography & Geography.Road) == Geography.Road;
        public static bool HasCityOrRoad(this Geography geography) => (geography & Geography.City) == Geography.City ||
                                                                      (geography & Geography.Road) == Geography.Road;
    
        /// <summary>
        /// Get the simple feature that the subtile represents if a meeple is on it. For example, if a subtile is
        /// CityRoad, a meeple placed on the subtile is on the city.
        /// </summary>
        /// <param name="geography"></param>
        /// <returns></returns>
        public static Geography Simple(this Geography geography)
        {
            // If it is a compound geography with a city, its simple representation is City
            if (geography.HasCity()) return Geography.City;
            
            // If it is a compound geography with a road that DOESN'T have a city, its simple representation is a Road
            if (geography.HasRoad()) return Geography.Road;

            return geography;
        }

        /// <summary>
        /// Is this subtile a feature. Only cities, roads, and cloisters are features.
        /// </summary>
        /// <param name="geography"></param>
        /// <returns></returns>
        public static bool IsFeature(this Geography geography)
        {
            if ((geography & (Geography.City | Geography.Road | Geography.Cloister)) == geography) return true;

            return false;
        }
    }

    /// <summary>
    ///     Geography decides what is contained within each direction. If there is a road going out to the right and the
    ///     rotation is 0 then east will become "road".
    ///
    ///     Represented as a bitmask so that combination tiles (CityRoad) can be tested as City & X == City,
    ///     which returns True for X in [City, CityStream, CityRoad].
    /// </summary>
    [Flags]
    public enum Geography
    {
        Cloister,
        Village,
        Road,
        Field,
        City,
        Stream,
        CityStream = City + Stream,
        RoadStream = Road + Stream,
        CityRoad = City + Road
    }
}