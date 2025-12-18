#pragma once

#ifdef LEGACYMATH_EXPORTS
#define LEGACYMATH_API __declspec(dllexport)
#else
#define LEGACYMATH_API __declspec(dllimport)
#endif

extern "C" {
    LEGACYMATH_API int add(int a, int b);
}