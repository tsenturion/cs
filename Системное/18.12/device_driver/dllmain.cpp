// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        // Инициализация библиотеки
        // В реальном проекте здесь может быть:
        // - Инициализация библиотеки для работы с устройством
        // - Регистрация обработчиков
        break;

    case DLL_THREAD_ATTACH:
        break;

    case DLL_THREAD_DETACH:
        break;

    case DLL_PROCESS_DETACH:
        // Очистка при выгрузке DLL
        // Гарантируем, что устройство будет корректно закрыто
        // В реальном проекте здесь может быть вызов CloseDevice()
        break;
    }
    return TRUE;
}