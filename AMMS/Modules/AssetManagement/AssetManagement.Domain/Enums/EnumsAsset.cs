using System;
using System.Collections.Generic;
using System.Text;

namespace AssetManagement.Domain.Enums
{

    public enum AssetCategory
    {
        Aircraft = 1,
        Engine = 2,
        Component = 3,
        CabinEquipment = 4,
        GroundSupportEquipment = 5,
        Tool = 6,
        SpecialApparatus = 7,
        CalibrationDevice = 8
    }


    public enum AssetStatus
    {
        InService = 1,
        UnderMaintenance = 2,
        Faulty = 3,
        OutOfService = 4,
        InStorage = 5,
        Passive = 6,
        Scrapped = 7,
        Sold = 8
    }


    public enum LocationType
    {
        Hangar = 1,
        Apron = 2,
        Warehouse = 3,
        FlightLine = 4,
        Workshop = 5,
        Other = 99
    }


    public enum AssetDocumentType
    {
        CeCertificate = 1,
        IsoCertificate = 2,
        EasaForm1 = 3,
        Faa8130 = 4,
        CalibrationCertificate = 5,
        ServiceBulletin = 6,
        AirworthinessDirective = 7,
        MaintenanceInstruction = 8,
        Other = 99
    }


    public enum LifeLimitType
    {
        Tbo = 1, // Time Between Overhaul
        Tsn = 2, // Time Since New
        Tso = 3, // Time Since Overhaul
        FlightHour = 4,
        LandingCycle = 5,
        CalendarDay = 6
    }
}
