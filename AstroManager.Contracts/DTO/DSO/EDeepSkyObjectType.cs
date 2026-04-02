namespace Shared.Model.DTO.DSO;

public enum EDeepSkyObjectType
{
    Unknown, // Default or unknown type

    // Galaxies
    Galaxy,
    SpiralGalaxy,
    EllipticalGalaxy,
    IrregularGalaxy,
    LenticularGalaxy,
    ActiveGalaxyNucleus, // AGN
    SeyfertGalaxy,
    Quasar, // QSO
    Blazar,
    RadioGalaxy,
    InteractingGalaxies,
    GalaxyCluster,
    GalaxyGroup,

    // Nebulae
    Nebula,
    EmissionNebula,
    ReflectionNebula,
    DarkNebula,
    PlanetaryNebula,
    SupernovaRemnant, // SNR
    HIIRegion, // Ionized Hydrogen region
    HerbigHaroObject, // HH object

    // Star Clusters
    StarCluster,
    OpenCluster,
    GlobularCluster,
    StellarAssociation,

    // Stars & Stellar Objects
    Star,
    MultipleStarSystem,
    VariableStar,
    Pulsar,
    NeutronStar,
    BlackHole,
    ProtoStellarObject, // YSO, T-Tauri
    YoungStellarObject,
    WhiteDwarf,
    BrownDwarf,
    CarbonStar,
    WolfRayetStar,

    // Other Cosmic Phenomena
    GravitationalLens,
    CosmicMicrowaveBackground, // CMB Anisotropy
    InterstellarMedium,
    MolecularCloud,

    // Sources by Wavelength
    RadioSource,
    InfraredSource, // IR
    OpticalSource,
    UltravioletSource, // UV
    XRaySource,
    GammaRaySource, // GRB

    // Catalog-specific or general
    ObjectOfUnknownNature, // Simbad's '?'
    Transient,
    Other, // For types not listed
    BrightNebula
}
