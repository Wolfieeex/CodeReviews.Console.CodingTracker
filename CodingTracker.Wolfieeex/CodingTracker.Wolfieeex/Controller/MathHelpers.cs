namespace CodingTracker.Wolfieeex.Controller;

internal class MathHelpers()
{
    internal static long RandomExponentialValueInRange(long min, long max, double lambda)
    {
        if (lambda < -1 || lambda > 1)
            throw new ArgumentException("Lambda value has to be in range of -1 and 1.");


        Random ran = new Random();
        double roll = ran.NextDouble();

        return min + (long)Math.Round((max - min) * Math.Pow(roll, lambda));
    }
    
    internal static bool PercentageChanceGenerator(double num)
    {
        if (num < 0 || num > 1)
            throw new ArgumentException("This method's argument needs to range from 0 to 1.");

        Random ran = new Random();
        double roll = ran.NextDouble();

        return num >= roll ? true : false;
    }

    public static TimeSpan CalculateDuration(string s, string e)
    {
        return DateTime.Parse(e) - DateTime.Parse(s);
    }
}