#pragma once
#include <windows.h>
#include <iostream>
#include <conio.h>
#include "ConsoleUtils.h"

using namespace std;
using namespace ConsoleUtils;

class LobbyScreen
{
public:
	static int StageSelect();
	static void PrintLogo();
};

