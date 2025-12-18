#include "pch.h"
#include "device_driver.h"
#include <windows.h>
#include <iostream>

// Приватные переменные для эмуляции работы с устройством
namespace {
    HANDLE g_hDevice = INVALID_HANDLE_VALUE;
    bool g_bInitialized = false;
    int g_lastValue = 0;

    // Приватная функция для "имитации" работы с реальным устройством
    BOOL SimulateDeviceIO(int command, int* pData = nullptr)
    {
        // Имитация задержки, как при работе с реальным устройством
        Sleep(10);

        switch (command)
        {
        case 0: // Инициализация
            g_lastValue = 100; // Начальное значение
            return TRUE;

        case 1: // Чтение
            if (pData) {
                // Имитация чтения с небольшим случайным изменением
                g_lastValue += rand() % 10 - 5;
                *pData = g_lastValue;
            }
            return TRUE;

        case 2: // Запись
            if (pData) {
                g_lastValue = *pData;
            }
            return TRUE;

        default:
            return FALSE;
        }
    }
}

// Инициализация устройства
DEVICEDRIVER_API int __stdcall InitializeDevice()
{
    if (g_bInitialized)
    {
        // Устройство уже инициализировано
        return 0; // Успех
    }

    // В реальном коде здесь была бы работа с Windows API:
    // g_hDevice = CreateFile(L"\\\\.\\PhysicalDevice", ...);
    // или вызов драйвера через DeviceIoControl

    // Имитация инициализации устройства через Windows API
    if (SimulateDeviceIO(0))
    {
        g_bInitialized = true;

        // В реальном приложении здесь может быть:
        // - Открытие порта (CreateFile для COM-порта)
        // - Подключение к драйверу устройства
        // - Настройка параметров связи
        // - Проверка наличия устройства

#ifdef _DEBUG
        OutputDebugString(L"DeviceDriver: Device initialized successfully\n");
#endif

        return 0; // Код успеха
    }

    return -1; // Код ошибки
}

// Чтение данных с устройства
DEVICEDRIVER_API int __stdcall ReadData(int* value)
{
    if (!g_bInitialized)
    {
        return -1; // Устройство не инициализировано
    }

    if (value == nullptr)
    {
        return -2; // Неверный указатель
    }

    // В реальном коде:
    // BOOL success = DeviceIoControl(g_hDevice, IOCTL_READ_DATA, ...);
    // или чтение из порта через ReadFile

    if (SimulateDeviceIO(1, value))
    {
#ifdef _DEBUG
        char debugMsg[100];
        sprintf_s(debugMsg, "DeviceDriver: Read value %d\n", *value);
        OutputDebugStringA(debugMsg);
#endif
        return 0; // Успех
    }

    return -3; // Ошибка чтения
}

// Запись данных на устройство
DEVICEDRIVER_API int __stdcall WriteData(int value)
{
    if (!g_bInitialized)
    {
        return -1; // Устройство не инициализировано
    }

    // В реальном коде:
    // DWORD bytesWritten;
    // WriteFile(g_hDevice, &value, sizeof(value), &bytesWritten, NULL);

    if (SimulateDeviceIO(2, &value))
    {
#ifdef _DEBUG
        char debugMsg[100];
        sprintf_s(debugMsg, "DeviceDriver: Written value %d\n", value);
        OutputDebugStringA(debugMsg);
#endif
        return 0; // Успех
    }

    return -4; // Ошибка записи
}

// Получение статуса устройства
DEVICEDRIVER_API int __stdcall GetDeviceStatus()
{
    if (!g_bInitialized)
    {
        return 0; // Статус: не инициализирован
    }

    // В реальном приложении здесь может быть проверка:
    // - Соединение с устройством
    // - Состояние устройства (готово/занято/ошибка)
    // - Температура, напряжение и другие параметры

    // Имитация различных состояний устройства
    int status = 1; // Статус: готово

    // С небольшим шансом вернуть ошибку (для тестирования)
    if ((rand() % 100) < 5) // 5% шанс
    {
        status = 2; // Статус: ошибка
    }

    return status;
}

// Закрытие соединения с устройством
DEVICEDRIVER_API int __stdcall CloseDevice()
{
    if (!g_bInitialized)
    {
        return 0; // Уже закрыто
    }

    // В реальном коде:
    // if (g_hDevice != INVALID_HANDLE_VALUE)
    // {
    //     CloseHandle(g_hDevice);
    //     g_hDevice = INVALID_HANDLE_VALUE;
    // }

    g_bInitialized = false;

#ifdef _DEBUG
    OutputDebugString(L"DeviceDriver: Device closed\n");
#endif

    return 0;
}