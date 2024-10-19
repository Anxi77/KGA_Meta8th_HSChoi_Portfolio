#pragma once
#include <iostream>
#include <windows.h>
#include <conio.h>
#include "ConsoleUtils.h"

using namespace std;
using namespace ConsoleUtils;

class GameOverScreen
{
public:
	static void PrintGameover();
	static int ContinueSelect(int topscore);
};

