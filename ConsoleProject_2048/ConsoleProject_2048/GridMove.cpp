#include "GridMove.h"

bool GridMove::moveGridUP()
{
	bool moved = false;
	int size = gridGen.getSize();
	auto &grid = gridGen.getGrid();
	for (int x = 0; x < size; ++x)
	{
		int mergeTarget = 0;
		for (int y = 1; y < size; ++y)
		{
			if (grid[y][x] != 0)
			{
				int currentY = y;
				while (currentY > mergeTarget && grid[currentY - 1][x] == 0)
				{
					grid[currentY - 1][x] = grid[currentY][x];
					grid[currentY][x] = 0;
					currentY--;
					moved = true;
				}
				if (currentY > mergeTarget && grid[currentY - 1][x] == grid[currentY][x])
				{
					grid[currentY - 1][x] *= 2;
					grid[currentY][x] = 0;
					mergeTarget = currentY;
					moved = true;
				}
			}
		}
	}
	return moved;
}

bool GridMove::moveGridLeft()
{
	bool moved = false;
	int size = gridGen.getSize();
	auto& grid = gridGen.getGrid();
	for (int y = 0; y < size; ++y)
	{
		int mergeTarget = 0;
		for (int x = 1; x < size; ++x)
		{
			if (grid[y][x] != 0)
			{
				int currentX = x;
				while (currentX > mergeTarget && grid[y][currentX - 1] == 0)
				{
					grid[y][currentX - 1] = grid[y][currentX];
					grid[y][currentX] = 0;
					currentX--;
					moved = true;
				}
				if (currentX > mergeTarget && grid[y][currentX - 1] == grid[y][currentX])
				{
					grid[y][currentX - 1] *= 2;
					grid[y][currentX] = 0;
					mergeTarget = currentX;
					moved = true;
				}
			}
		}
	}
	return moved;
}

bool GridMove::moveGridDown()
{
	bool moved = false;
	int size = gridGen.getSize();
	auto& grid = gridGen.getGrid();
	for (int x = 0; x < size; ++x)
	{
		int mergeTarget = size - 1;
		for (int y = size - 1; y >= 0; --y)
		{
			if (grid[y][x] != 0)
			{
				int currentY = y;
				while (currentY < mergeTarget && grid[currentY + 1][x] == 0)
				{
					grid[currentY + 1][x] = grid[currentY][x];
					grid[currentY][x] = 0;
					currentY++;
					moved = true;
				}
				if (currentY < mergeTarget && grid[currentY + 1][x] == grid[currentY][x])
				{
					grid[currentY + 1][x] *= 2;
					grid[currentY][x] = 0;
					mergeTarget = currentY;
					moved = true;
				}
			}
		}
	}
	return moved;
}

bool GridMove::moveGridRight()
{
	bool moved = false;
	int size = gridGen.getSize();
	auto& grid = gridGen.getGrid();
	for (int y = 0; y < size; ++y)
	{
		int mergeTarget = size - 1;
		for (int x = size - 1; x >= 0; --x)
		{
			if (grid[y][x] != 0)
			{
				int currentX = x;
				while (currentX < mergeTarget && grid[y][currentX + 1] == 0)
				{
					grid[y][currentX + 1] = grid[y][currentX];
					grid[y][currentX] = 0;
					currentX++;
					moved = true;
				}
				if (currentX < mergeTarget && grid[y][currentX + 1] == grid[y][currentX])
				{
					grid[y][currentX + 1] *= 2;
					grid[y][currentX] = 0;
					mergeTarget = currentX;
					moved = true;
				}
			}
		}
	}
	return moved;
}
