#pragma once

// Макрос для экспорта/импорта функций
#ifdef DEVICEDRIVER_EXPORTS
#define DEVICEDRIVER_API __declspec(dllexport)
#else
#define DEVICEDRIVER_API __declspec(dllimport)
#endif

// Указываем C-линкировку для совместимости с C#
extern "C" {

    // Инициализация устройства
    DEVICEDRIVER_API int __stdcall InitializeDevice();

    // Чтение данных с устройства
    DEVICEDRIVER_API int __stdcall ReadData(int* value);

    // Запись данных на устройство
    DEVICEDRIVER_API int __stdcall WriteData(int value);

    // Получение статуса устройства
    DEVICEDRIVER_API int __stdcall GetDeviceStatus();

    // Закрытие соединения с устройством
    DEVICEDRIVER_API int __stdcall CloseDevice();
}