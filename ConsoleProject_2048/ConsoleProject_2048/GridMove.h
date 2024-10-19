#pragma once
#include "GridGen.h"

class GridMove 
{
public:
    GridMove(GridGen& gridGen) : gridGen(gridGen) {}
    bool moveGridUP();
    bool moveGridDown();
    bool moveGridLeft();
    bool moveGridRight();
private:
    GridGen& gridGen;
};