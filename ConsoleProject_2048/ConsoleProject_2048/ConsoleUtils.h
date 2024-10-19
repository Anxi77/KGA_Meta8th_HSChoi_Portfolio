#pragma once
#include <iostream>
#include <windows.h>
#include <vector>
#include <conio.h>
#include <string>
#include <sstream>

using namespace std;

namespace ConsoleUtils
{
    enum class ConsoleColor
    {
        Black = 0,
        Blue = 1,
        Green = 2,
        Aqua = 3,
        Red = 4,
        Purple = 5,
        Yellow = 6,
        White = 7,
        Gray = 8,
        LightBlue = 9,
        LightGreen = 10,
        LightAqua = 11,
        LightRed = 12,
        LightPurple = 13,
        LightYellow = 14,
        BrightWhite = 15
    };
    void ColoredPrint(const string& text, ConsoleColor color);
    void SetColor(ConsoleColor color);
    void DefaultColor();
    void SetConsoleBufferSize(int width, int height);
    void SetConsoleWindowSize(int width, int height);
    void Fullscreen();
    void SetConsoleFontSize(int width, int height);
    void GetConsoleCursorPosition();
    void CursorView();
    void gotoXY();
    void SetConsoleCursorPosition(int x, int y);
    void Borders();
}

