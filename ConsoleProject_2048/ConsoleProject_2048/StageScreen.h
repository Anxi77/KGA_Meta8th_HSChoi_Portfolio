#pragma once

#include <iostream>
#include <windows.h>
#include <vector>
#include "ConsoleUtils.h"

using namespace std;
using namespace ConsoleUtils;

class StageScreen
{
public:
    static void GridDisplay(int size);
    static void Scores(int topscore, int currentScore);
    static void TileDisplay(int size, vector<vector<int>>& grid, pair<int, int> newTilePos);
    static void Instructions();
};

