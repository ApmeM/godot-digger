using System;
using System.Collections.Generic;

public static class Fibonacci
{
    // Dictionary to store computed Fibonacci numbers for memoization
    private static readonly Dictionary<uint, int> memorization = new Dictionary<uint, int>();

    // Method to calculate the n-th Fibonacci number
    public static int Calc(uint n)
    {
        // Base cases
        if (n == 0) return 0;
        if (n == 1) return 1;

        // Check if result is already in the memo
        if (memorization.ContainsKey(n))
            return memorization[n];

        // Recursive calculation with memoization
        int result = Calc(n - 1) + Calc(n - 2);
        memorization[n] = result; // Store the result in the dictionary

        return result;
    }
}