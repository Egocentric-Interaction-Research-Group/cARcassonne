using System;

namespace Carcassonne.Models
{
    public static class Extensions
    {
        
        /// <summary>
        /// Does this <see cref="Geography"/> include a <see cref="Geography.City"/>?
        /// </summary>
        /// <param name="geography"></param>
        /// <returns>True if <see cref="Geography"/> is one of <see cref="Geography.City"/>,
        /// <see cref="Geography.CityRoad"/>, <see cref="Geography.CityStream"/></returns>
        public static bool HasCity(this Geography geography) => (geography & Geography.City) == Geography.City;
        
        /// <summary>
        /// Does this <see cref="Geography"/> include a <see cref="Geography.Road"/>?
        /// </summary>
        /// <param name="geography"></param>
        /// <returns>True if <see cref="Geography"/> is one of <see cref="Geography.Road"/>,
        /// <see cref="Geography.CityRoad"/>, <see cref="Geography.RoadStream"/></returns>
        public static bool HasRoad(this Geography geography) => (geography & Geography.Road) == Geography.Road;
        
        /// <summary>
        /// Does this <see cref="Geography"/> include a <see cref="Geography.City"/> or <see cref="Geography.Road"/>?
        /// </summary>
        /// <param name="geography"></param>
        /// <returns>True if <see cref="Geography"/> is one of <see cref="Geography.City"/>, <see cref="Geography.Road"/>,
        /// <see cref="Geography.CityRoad"/>, <see cref="Geography.CityStream"/>, <see cref="Geography.RoadStream"/></returns>
        public static bool HasCityOrRoad(this Geography geography) => (geography & Geography.City) == Geography.City ||
                                                                      (geography & Geography.Road) == Geography.Road;
    
        /// <summary>
        /// Get the simple feature that the subtile represents if a <see cref="Meeple"/> is on it. For example, if a subtile is
        /// <see cref="Geography.CityRoad"/>, a meeple placed on the subtile is on the <see cref="City"/>.
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
        /// Is this subtile represents a <see cref="Feature"/>. The only features are <see cref="City"/>,
        /// <see cref="Road"/>, and <see cref="Cloister"/>.
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
    /// Geography represents the type of each sub-tile.
    ///
    /// Represented as a bitmask so that combination tiles (<see cref="Geography.CityRoad"/>) can be tested as
    /// <see cref="Geography.City"/> & X == <see cref="Geography.City"/> which returns True for X in
    /// [<see cref="Geography.City"/>, <see cref="Geography.CityStream"/>, <see cref="Geography.CityRoad"/>].
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