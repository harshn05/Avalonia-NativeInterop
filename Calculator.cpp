// Calculator.cpp
#define DLLEXPORT extern "C" __declspec(dllexport)

// Simple addition function
DLLEXPORT int AddNumbers(int a, int b) {
    return a + b;
}

// Factorial calculation logic (Heavy calculation example)
extern "C" __declspec(dllexport) double CalculateFactorial(int n) {
    double fact = 1.0;
    for (int i = 1; i <= n; ++i) {
        fact *= i;
    }
    return fact;
}

