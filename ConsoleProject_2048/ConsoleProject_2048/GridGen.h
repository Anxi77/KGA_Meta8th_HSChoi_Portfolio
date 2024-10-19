#pragma once
#include <iostream>
#include <vector>
#include <random>

using namespace std;

class GridGen
{
public:
	GridGen(int size);
	void GridInitialLize();
	bool SpawnRandomNumber();
	void testprint();
	int getSize() { return size; }
	vector<vector<int>>& getGrid() { return grid; }
	void setGrid(const vector<vector<int>>& newGrid) { grid = newGrid; }
	pair<int, int> getNewSpawn() { return newSpawn; }
protected:
	int size;
	vector<vector<int>> grid;
	mt19937 gen;
	bool isGridFUll;
	pair<int, int> newSpawn;
};

