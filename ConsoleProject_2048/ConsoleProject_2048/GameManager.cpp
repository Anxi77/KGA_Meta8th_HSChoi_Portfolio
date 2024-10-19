#include "GameManager.h"

int checkRow(const vector<vector<int>>& grid, int size, int y, int x)
{
	int count = 0;
	for (int x = size - 1; x >= 0; --x)
	{
		if (x == size - 1)
		{
			if (grid[y][x] == grid[y][x - 1])
			{
				count++;
			}
		}
		else if (grid[y][x] == grid[y][x + 1])
		{
			count++;
		}
	}
	return count;

}

int checkColumn(const vector<vector<int>>& grid, int size, int y, int x)
{
	int count = 0;
	for (int y = size - 1; y >= 0; --y)
	{
		if (y == size - 1)
		{
			if (grid[y][x] == grid[y - 1][x])
			{
				count++;
			}
		}
		else if (grid[y][x] == grid[y + 1][x])
		{
			count++;
		}
	}
	return count;
}

bool GameManager::GameVali(vector<vector<int>>& grid, int size)
{
	bool Zeros = false;
	int count = 0;
	for (int y = 0; y < size; ++y)
	{
		for (int x = 0; x < size; ++x)
		{
			if (grid[x][y] == 0)
			{
				Zeros = true;
			}
		}
	}
	if (Zeros != true)
	{
		for (int y = 0; y < size; ++y)
		{
			for (int x = 0; x < size; ++x)
			{
				count += checkRow(grid, size, y, x);
				count += checkColumn(grid, size, y, x);
			}
		}
		if (count == 0)
		{
			return false;
		}
	}
	return true;

}

void GameManager::GameStart()
{
	int topscore = 2;
	int size = 0;
	SceneManager& S = SceneManager::getInstance();
	bool GameTrig = true;
	while (GameTrig)
	{
		Borders();
		S.LobbyScene();
		switch (S.StageSelect())
		{
		case 12:
			size = 4;
			break;
		case 14:
			size = 5;
			break;
		case 16:
			GameTrig = false;
			continue;
		}
		GridManager& G = GridManager::getInstance(size);
		if (!savedGrid.empty() && savedGrid.size() == size)
		{           
			if (GameVali(savedGrid, size))
			{
				G.setGrid(savedGrid);
			}
			else
			{
				G.initialize();
			}
		}
		else
		{
			G.initialize();
		}
		G.SpawnNewNumber();
		previousGrid = G.getGrid();
		while (GameVali(G.getGrid(), size))
		{
			int currentScore = G.getCurrentScore(size);
			if (currentScore > topscore) 
			{
				topscore = currentScore;
			}
			S.StageScene(size, topscore, currentScore, G.getGrid(), G.getNewSpawn());
			vector<vector<int>> tempGrid = G.getGrid();			
			bool moved = G.move();
			if (G.getReturn() == true)
			{
				G.setGrid(previousGrid);
				G.resetNewSpawn();
				continue;
			}
			else if(moved == true)
			{
				previousGrid = tempGrid;
				G.SpawnNewNumber();
			}
			if (G.getbEsc() == true)
			{
				break;
			}
		}

		savedGrid = G.getGrid();
		SaveGame(topscore, savedGrid);

		S.GameOverScene();
		if(S.ContinueSelect(topscore) == 18)
		{
			GameTrig = false;
		}
		system("cls");
	}
}

GameManager::GameManager()
{
	LoadGame(topscore, savedGrid);
}

GameManager::~GameManager()
{
	SaveGame(topscore, savedGrid);
}

void GameManager::SaveGame(int topscore, const vector<vector<int>>& grid)
{
	ofstream saveFile(SAVE_FILE);
	if (saveFile.is_open()) 
	{
		saveFile << topscore << endl;
		saveFile << grid.size() << endl;
		for (const auto& row : grid) 
		{
			for (int cell : row) 
			{
				saveFile << cell << " ";
			}
			saveFile << endl;
		}
		saveFile.close();
	}
}

void GameManager::LoadGame(int& topscore, vector<vector<int>>& grid)
{
	ifstream loadFile(SAVE_FILE);
	if (loadFile.is_open()) 
	{
		loadFile >> topscore;
		int size;
		loadFile >> size;
		grid.resize(size, vector<int>(size));
		for (int i = 0; i < size; ++i) 
		{
			for (int j = 0; j < size; ++j) 
			{
				loadFile >> grid[i][j];
			}
		}
		loadFile.close();
	}
}





