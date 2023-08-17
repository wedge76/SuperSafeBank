using System;
using System.Collections.Generic;

namespace SuperSafeBank.Domain;

public record Currency
{
    public Currency(string name, string symbol)
    {
        if (String.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentNullException(nameof(symbol));
        }

        if (String.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        Symbol = symbol;
        Name = name;
    }

    public string Name { get; }

    public string Symbol { get; }

    public override string ToString()
    {
        return Symbol;
    }

    #region Factory

    private static readonly IDictionary<string, Currency> Currencies;

    static Currency()
    {
        Currencies = new Dictionary<string, Currency>
                     {
                         { Euro.Name, Euro },
                         { CanadianDollar.Name, CanadianDollar },
                         { UsDollar.Name, UsDollar }
                     };
    }

    public static Currency FromCode(string code)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentNullException(nameof(code));
        }

        var normalizedCode = code.Trim().ToUpper();
        if (!Currencies.ContainsKey(normalizedCode))
        {
            throw new ArgumentException($"Invalid code: '{code}'", nameof(code));
        }

        return Currencies[normalizedCode];
    }

    public static Currency Euro => new("EUR", "€");

    public static Currency CanadianDollar => new("CAD", "CA$");

    public static Currency UsDollar => new("USD", "US$");

    #endregion Factory
}