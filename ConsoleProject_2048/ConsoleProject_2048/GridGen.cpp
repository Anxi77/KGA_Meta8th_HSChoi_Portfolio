#include "GridGen.h"

using namespace std;

GridGen::GridGen(int size) :size(size), gen(random_device{}()), isGridFUll(false)
{
	int x = size;
	int y = size;
	grid = vector<vector<int>>(y, vector<int>(x, 0));
}

void GridGen::GridInitialLize()
{
    grid = vector<vector<int>>(size, vector<int>(size, 0));
}

bool GridGen::SpawnRandomNumber()
{
#pragma region Genarate size*size Coordinate Method
    uniform_int_distribution<> dis(0, size * size - 1);
    vector<int> emptyCells;
    for (int i = 0; i < size * size; ++i) 
    {
        if (grid[i / size][i % size] == 0) 
        {
            emptyCells.push_back(i);
        }
    }
    if (!emptyCells.empty()) 
    {
        int index = emptyCells[dis(gen) % emptyCells.size()];
        int y = index / size;
        int x = index % size;
        grid[y][x] = (dis(gen) % 10 < 9) ? 2 : 4;
        newSpawn = {y,x};
    }
    else
    {
        return false;
    }
#pragma endregion
#pragma region Genarate Separate X,Y Coordinate Method
    /*
    random_device rd;
    mt19937 gen(rd());
    vector<vector<int>> grid(5, vector<int>(5, 0));
    vector<vector<int>> emptycells(5, vector<int>(5));
    uniform_int_distribution<> ranCoord(0, 4);
    uniform_int_distribution<> ranNumProb(0, 100);

    for (int i = 0; i < 5; ++i)
    {
        for (int j = 0; j < 5; ++j)
        {
            if (grid[i][j] == 0)
            {
                emptycells[i][j] = 0;
            }
        }

    }

    if (!emptycells.empty())
    {
        int x = ranCoord(gen);
        int y = ranCoord(gen);
        if (grid[y][x] == 0)
        {
            grid[y][x] = (ranNumProb(gen) < 90) ? 2 : 4;
        }
    }
    */
#pragma endregion
}

void GridGen::testprint()
{
	for (int y = 0; y < size; ++y)
	{
		for (int x = 0; x < size; ++x)
		{
            if(grid[y][x] == 0)
            {
                cout << "[ ]" << "\t";
            }
            else 
            {
                cout << "[" << grid[y][x] << "]" << "\t";
            }
		}
		cout << endl << endl;
	}
}


