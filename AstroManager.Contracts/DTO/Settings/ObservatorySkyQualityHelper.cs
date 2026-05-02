namespace Shared.Model.DTO.Settings;

/// <summary>
/// Helper methods for working with observatory sky brightness and Bortle classes.
/// </summary>
public static class ObservatorySkyQualityHelper
{
    public const double MinSkyQualityMpsas = 0d;
    public const double MaxSkyQualityMpsas = 30d;

    /// <summary>
    /// Returns the effective Bortle class, preferring a derived value from exact sky quality when available.
    /// </summary>
    public static int? GetEffectiveBortleClass(int? bortleClass, double? skyQualityMpsas)
    {
        if (skyQualityMpsas.HasValue)
        {
            return DeriveBortleClassFromSkyQuality(skyQualityMpsas.Value);
        }

        return bortleClass;
    }

    /// <summary>
    /// Derives an approximate Bortle class from sky quality in magnitudes per square arcsecond.
    /// </summary>
    public static int DeriveBortleClassFromSkyQuality(double skyQualityMpsas)
    {
        if (skyQualityMpsas >= 21.99d) return 1;
        if (skyQualityMpsas >= 21.89d) return 2;
        if (skyQualityMpsas >= 21.69d) return 3;
        if (skyQualityMpsas >= 20.49d) return 4;
        if (skyQualityMpsas >= 19.50d) return 5;
        if (skyQualityMpsas >= 18.94d) return 6;
        if (skyQualityMpsas >= 18.38d) return 7;
        if (skyQualityMpsas >= 17.80d) return 8;
        return 9;
    }
}
